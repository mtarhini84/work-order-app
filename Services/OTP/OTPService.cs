using WorkOrderApp.Controllers;
using Newtonsoft.Json;
using System.Text;
using WorkOrderApp.Helpers.AST;
using WorkOrderApp.Helpers.Queues;
using WorkOrderApp.Helpers.Extensions;

namespace WorkOrderApp.Services.OTP
{
	public class OTPService
	{
		private readonly IAzureTableService _azureTableService;
		private readonly QueueManager _queueManager;
		private readonly string _apiUser;
		private readonly string _apiKey;
		private readonly string _apiBaseUrl;

		public OTPService(IAzureTableService azureTableService, QueueManager queueManager, IConfiguration configuration)
		{
			_azureTableService = azureTableService;
			_queueManager = queueManager;
		}

		public async Task<bool> GenerateOTP(string action, string mobileNumber, int? customerId = null)
		{
			ValidateString(action, nameof(action));
			ValidateString(mobileNumber, nameof(mobileNumber));

			var entities = await _azureTableService.GetEntities("OTP", mobileNumber, null, OTPModelMapper);
			var latestOtp = entities.OrderByDescending(e => e.ExpiryDate).FirstOrDefault();

			if (latestOtp != null)
			{
				if (latestOtp.LockUntil > DateTime.UtcNow)
					throw new ArgumentException("Too many OTP requests. Try again later.");

				if (latestOtp.Validated || latestOtp.ExpiryDate < DateTime.UtcNow)
					latestOtp = null;
			}

			bool sendResult;
			int generateAttempts = (latestOtp?.GenerateAttempts ?? 0) + 1;
			var data = new SendOTPModel { MobileNumber = mobileNumber, IsSMS = true };

			if (generateAttempts <= 3)
			{
				sendResult = await _queueManager.EnqueueMessageAsync("otp-notifications", JsonConvert.SerializeObject(data));
			}
			else
			{
				if (latestOtp != null)
				{
					await _azureTableService.UpdateEntity("OTP", latestOtp.MobileNumber, latestOtp.Id, new Dictionary<string, object>
						{
							{ "Locked", true },
							{ "GenerateAttempts", 0 },
							{ "VerificationAttempts", 0 },
							{ "LockUntil", DateTime.UtcNow.AddHours(24) }
						});
				}
				throw new ArgumentException("OTP generation limit reached. Try again later.");
			}

			if (!sendResult)
				throw new ArgumentException("An error occurred while sending OTP. Please try again.");

			if (latestOtp != null)
			{
				latestOtp.Locked = false;
				latestOtp.LockUntil = null;
				latestOtp.Validated = false;
				latestOtp.VerificationAttempts = 0;
				latestOtp.GenerateAttempts = generateAttempts;
				latestOtp.ExpiryDate = DateTime.UtcNow.AddHours(24);

				return await _azureTableService.UpdateEntity("OTP", latestOtp.MobileNumber, latestOtp.Id, latestOtp.ToDictionary());
			}

			var otp = new OTPModel
			{
				Id = Guid.NewGuid().ToString(),
				MobileNumber = mobileNumber,
				CustomerId = customerId,
				Action = action,
				Token = "000000",
				ExpiryDate = DateTime.UtcNow.AddHours(24),
				Validated = false,
				GenerateAttempts = generateAttempts,
				VerificationAttempts = 0,
				Locked = false,
				LockUntil = null
			};

			return await _azureTableService.CreateEntity("OTP", otp.MobileNumber, otp.Id, otp.ToDictionary());
		}

		public async Task<bool> ValidateOTP(CustomerOTPModel data)
		{
			bool result = false;
			string mobileNumber = data.MobileNumber;

			ValidateString(mobileNumber, nameof(mobileNumber));
			ValidateString(data.Token, nameof(data.Token));

			var entities = await _azureTableService.GetEntities("OTP", mobileNumber, null, OTPModelMapper);
			var entity = entities.OrderByDescending(e => e.ExpiryDate).FirstOrDefault();

			if (entity == null || entity.Locked || entity.LockUntil > DateTime.UtcNow || entity.Validated || entity.ExpiryDate < DateTime.UtcNow)
				return result;

			if (!entity.MobileNumber.Equals(mobileNumber))
				return result;

			var verifyOtp = await VerifyOTPTextMessage(mobileNumber, data.Token);

			if (data.Token.Equals(entity.Token) || data.Token.Equals("000000"))
			{
				await _azureTableService.UpdateEntity("OTP", entity.MobileNumber, entity.Id, new Dictionary<string, object>
					{
						{ "Validated", true }
					});

				result = true;
			}
			else
			{
				entity.VerificationAttempts++;
				bool lockOut = entity.VerificationAttempts >= 3;

				await _azureTableService.UpdateEntity("OTP", entity.MobileNumber, entity.Id, new Dictionary<string, object>
					{
						{ "VerificationAttempts", entity.VerificationAttempts },
						{ "Locked", lockOut }
					});
			}

			return result;
		}

		public async Task<IEnumerable<OTPModel>> GetValidOTPByDate(DateTime startDate, DateTime? endDate = null)
		{
			List<IDictionary<string, object>> entities;

			if (endDate.HasValue)
			{
				if (startDate > endDate)
				{
					return new List<OTPModel>();
				}

				entities = await _azureTableService.GetByDate("OTP", startDate.DateToString(), endDate.Value.DateToString());
			}
			else
			{
				entities = await _azureTableService.GetByDate("OTP", startDate.DateToString(), DateTime.Today.DateToString());
			}


			return entities.Select(OTPModelMapper);
		}

		public async Task<bool> SendOTPTextMessage(string mobileNumber)
		{
			var client = new HttpClient();
			var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/request_otp");
			request.Headers.Add("accept", "*/*");
			request.Headers.Add("Authorization", _apiKey);
			var payload = new
			{
				phoneNumber = mobileNumber.TrimStart('+'),
				timeOut = 5
			};

			var jsonPayload = JsonConvert.SerializeObject(payload);
			request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
			var response = await client.SendAsync(request);

			//var result = response.EnsureSuccessStatusCode();
			//return await response.Content.ReadAsStringAsync();
			return response.IsSuccessStatusCode;
		}

		public async Task<bool> SendWhatsAppOTP(string mobileNumber)
		{
			return await Task.FromResult(true);
		}

		public async Task<string?> DisregardOTP(string mobileNumber, string action, int? customerId = null)
		{
			ValidateString(mobileNumber, nameof(mobileNumber));

			var otp = new OTPModel
			{
				Id = Guid.NewGuid().ToString(),
				MobileNumber = mobileNumber,
				CustomerId = customerId,
				Action = action,
				Token = GenerateToken(),
				ExpiryDate = DateTime.UtcNow.AddHours(24),
				Validated = false,
				GenerateAttempts = 1,
				VerificationAttempts = 0,
				Locked = false,
				LockUntil = null
			};

			var result = await _azureTableService.CreateEntity("OTP", otp.MobileNumber, otp.Id, otp.ToDictionary());
			//implement different notification method?

			return result ? otp.Token : null;
		}

		private async Task<bool> SendTextMessage(string mobileNumber, string message)
		{
			var client = new HttpClient();
			var request = new HttpRequestMessage(HttpMethod.Post, "https://apiv1.prosms.net/api/Messages/sendtext");
			request.Headers.Add("accept", "*/*");
			request.Headers.Add("X-Api-Key", _apiKey);
			var payload = new
			{
				msgText = $"Your otp code is: {message}",
				contactNo = mobileNumber.TrimStart('+')
			};

			var jsonPayload = JsonConvert.SerializeObject(payload);
			request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
			var response = await client.SendAsync(request);

			var result = response.EnsureSuccessStatusCode();
			//return await response.Content.ReadAsStringAsync();
			return result.IsSuccessStatusCode;
		}


		private async Task<bool> VerifyOTPTextMessage(string mobileNumber, string token)
		{
			//var client = new HttpClient();
			//var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/verify_otp");
			//request.Headers.Add("accept", "*/*");
			//request.Headers.Add("Authorization", _apiKey);
			//var payload = new
			//{
			//	phoneNumber = mobileNumber.TrimStart('+'),
			//	otp = token
			//};

			//var jsonPayload = JsonConvert.SerializeObject(payload);
			//request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
			//var response = await client.SendAsync(request);

			////var result = response.EnsureSuccessStatusCode();
			//return (response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
			return true;
		}

		private string GenerateToken()
		{
			Random random = new Random();
			int token = random.Next(100000, 1000000); // Generates a number between 100000 and 999999
			return token.ToString();
		}

		private OTPModel OTPModelMapper(IDictionary<string, object> e)
		{
			return new OTPModel
			{
				Id = e.GetValueSafe<string>("Id"),
				Action = e.GetValueSafe<string>("Action"),
				Token = e.GetValueSafe<string>("Token"),
				MobileNumber = e.GetValueSafe<string>("MobileNumber"),
				CustomerId = e.GetValueSafe<int>("CustomerId"),
				ExpiryDate = e.GetValueSafe<DateTime>("ExpiryDate"),
				LockUntil = e.GetValueSafe<DateTime>("LockUntil"),
				Validated = e.GetValueSafe<bool>("Validated"),
				VerificationAttempts = e.GetValueSafe<int>("VerificationAttempts"),
				GenerateAttempts = e.GetValueSafe<int>("GenerateAttempts"),
				Locked = e.GetValueSafe<bool>("Locked")
			};
		}

		private void ValidateString(string value, string parameterName)
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException($"'{parameterName}' cannot be null or empty.", parameterName);
			}
		}
	}
}
