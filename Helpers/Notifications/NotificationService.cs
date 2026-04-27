using Newtonsoft.Json;
using WorkOrderApp.Helpers.AST;
using WorkOrderApp.Helpers.Expo;
using WorkOrderApp.Helpers.Extensions;
using WorkOrderApp.Helpers.Queues;

namespace WorkOrderApp.Helpers.Notifications
{
	public class NotificationService
	{
		private readonly IAzureTableService _tableService;
		private readonly QueueManager _queueManager;
		private readonly PushApiClient _expoSDKClient;

		public NotificationService(IAzureTableService tableService, QueueManager queueManager)
		{
			_tableService = tableService;
			_expoSDKClient = new PushApiClient();
			_queueManager = queueManager;
		}

		public async Task<bool> SendToAllAsync(SendNotificationObject data)
		{
			var activeTokens = await GetAllActiveTokens();

			var tokens = activeTokens.Select(a => a.Token).Distinct().ToList();

			int batchSize = 100;

			for (int i = 0; i < tokens.Count; i += batchSize)
			{
				var batchTokens = tokens.Skip(i).Take(batchSize).ToList();

				var pushTicketReq = new PushTicketRequest()
				{
					PushTo = batchTokens,
					PushBadgeCount = 1,
					PushTitle = data.Title,
					PushBody = data.Message,
					PushSound = "default",
					PushData = data.Data,
					_ContentAvailable = false,
				};

				var result = await _expoSDKClient.PushSendAsync(pushTicketReq);

				if (result?.PushTicketErrors?.Any() == true)
				{

				}
			}

			return true;
		}

		public async Task<bool> SendToUserAsync(SendNotificationObject data)
		{
			var activeTokens = await GetUserFCMTokens(data.Id);

			if (activeTokens != null & activeTokens.Count() > 0)
			{
				var pushTicketReq = new PushTicketRequest()
				{
					PushTo = activeTokens,
					PushTitle = data.Title,
					PushBody = data.Message,
					PushChannelId = string.IsNullOrEmpty(data.ChannelId) ? "default" : data.ChannelId,
					PushSound = string.IsNullOrEmpty(data.PushSound) ? "default" : data.PushSound,
					PushPriority = "high",
					PushData = data.Data,
					_ContentAvailable = data.ContentAvailable
				};
				var result = await _expoSDKClient.PushSendAsync(pushTicketReq);

				if (result?.PushTicketErrors?.Count() > 0)
				{
					//
				}
			}

			return true;
		}

		public async Task<bool> EnqueueNotification(SendNotificationObject data)
		{
			var result = await _queueManager.EnqueueMessageAsync("notifications", JsonConvert.SerializeObject(data));
			return result;
		}

		public async Task<bool> CreateUserToken(string userId, string tokenId)
		{
			bool result = await _tableService.CreateEntity("DeviceTokens", userId, tokenId, new Dictionary<string, object>
			{
				{ "CreatedAt", DateTimeOffset.UtcNow },
				{ "Active", true }
			});

			return result;
		}

		public async Task<bool> DeactivateToken(DeviceToken token)
		{
			var result = await _tableService.UpdateEntity("DeviceTokens", token.UserId, token.Token, new Dictionary<string, object> { { "Active", token.Active } });

			return result;
		}

		public async Task<List<DeviceToken>> GetActiveUserTokens(string userId)
		{
			var tokens = await _tableService.GetEntities("DeviceTokens", userId, null, DeviceTokenMapper);

			if (tokens == null)
			{
				throw new Exception();
			}

			return tokens.ToList();
		}

		public async Task<bool> DeleteUserToken(string userId, string token)
		{
			var result = await _tableService.DeleteEntity("DeviceTokens", userId, token);

			return result;
		}

		public async Task<bool> DeleteUserTokens(string userId)
		{
			var tokens = await _tableService.GetEntities("DeviceTokens", userId, null, DeviceTokenMapper);

			foreach (var token in tokens)
			{
				await DeleteUserToken(token.UserId, token.Token);
			}

			return true;
		}

		public async Task<List<DeviceToken>> GetAllActiveTokens()
		{
			var tokens = await _tableService.GetEntities("DeviceTokens", DeviceTokenMapper);

			if (tokens == null)
			{
				throw new Exception();
			}

			return tokens.ToList();
		}

		public async Task<bool> CreateNotificationObject(NotificationObject notiObj)
		{
			bool result = await _tableService.CreateEntity("NotificationObjects", notiObj.CustomerId, notiObj.Id, notiObj.ToDictionary());

			return result;
		}

		public async Task<bool> CreateDelayedNotificationObject(DelayedNotificationObject notiObj)
		{
			bool result = await _tableService.CreateEntity("DelayedNotificationObjects", notiObj.CustomerId, notiObj.OnlineOrderId, notiObj.ToDictionary());

			return result;
		}

		public async Task<bool> NotificationReadAsync(string customerId, string notificationId)
		{
			bool result = await _tableService.CreateEntity("notificationread", customerId, notificationId, new Dictionary<string, object>
			{
				{ "CustomerId", customerId },
				{ "Active", notificationId }
			});

			return result;
		}

		public async Task<IList<string>> GetCustomerReadNotificationIds(string customerId)
		{
			var result = await _tableService.GetEntities("notificationread", customerId);

			return result.Keys.ToList();
		}

		public async Task<List<NotificationObject>> GetNotificationObjectByUser(string id)
		{
			var result = await _tableService.GetEntities("NotificationObjects", id, null, NotificationObjectMapper);
			if (result == null)
			{
				throw new ArgumentException("An error occured.");
			}

			return result.ToList();
		}

		public async Task<DelayedNotificationObject?> GetDelayedNotificationObject(int customerId, string onlineOrderId)
		{
			var result = await _tableService.GetEntity("DelayedNotificationObjects", $"customer-{customerId}", onlineOrderId);
			if (result == null)
			{
				return null;
			}

			return DelayedNotificationObjectMapper(result);
		}

		public async Task<IList<NotificationObject>> GetNotificationsListing(DateTime? registeredDate)
		{
			var result = await _tableService.GetEntities("NotificationObjects", "all", registeredDate, NotificationObjectMapper);
			if (result != null)
			{
				return result.ToList();
			}

			return new List<NotificationObject>();
		}

		public async Task<IList<DelayedNotificationObject>> GetDelayedNotificationsListing()
		{
			var result = await _tableService.GetEntities("DelayedNotificationObjects", DelayedNotificationObjectMapper);
			if (result == null)
			{
				return new List<DelayedNotificationObject>();
			}

			return result.ToList();
		}

		public async Task<bool> DeleteDelayedNotification(DelayedNotificationObject data)
		{
			return await _tableService.DeleteEntity("DelayedNotificationObjects", data.CustomerId, data.OnlineOrderId);
		}

		private async Task<List<string>> GetUserFCMTokens(string userId)
		{
			var tokens = await GetActiveUserTokens(userId);
			List<string> result = new List<string>();

			foreach (var token in tokens)
			{
				result.Add(token.Token);
			}

			return result;
		}

		private DeviceToken DeviceTokenMapper(IDictionary<string, object> dictionary)
		{
			return new DeviceToken
			{
				UserId = dictionary.GetValueSafe<string>("PartitionKey"),
				Token = dictionary.GetValueSafe<string>("RowKey"),
				CreatedAt = dictionary.GetValueSafe<DateTime>("CreatedAt"),
				Active = dictionary.GetValueSafe<bool>("Active")
			};
		}

		private NotificationObject NotificationObjectMapper(IDictionary<string, object> dictionary)
		{
			return new NotificationObject
			{
				Id = dictionary.GetValueSafe<string>("RowKey"),
				CustomerId = dictionary.GetValueSafe<string>("PartitionKey"),
				Image = dictionary.GetValueSafe<string>("Image"),
				Title = dictionary.GetValueSafe<string>("Title"),
				Body = dictionary.GetValueSafe<string>("Body"),
				CreatedOn = dictionary.GetValueSafe<DateTime>("CreatedOn")
			};
		}

		private DelayedNotificationObject DelayedNotificationObjectMapper(IDictionary<string, object> dictionary)
		{
			return new DelayedNotificationObject
			{
				OnlineOrderId = dictionary.GetValueSafe<string>("RowKey"),
				CustomerId = dictionary.GetValueSafe<string>("PartitionKey"),
				Image = dictionary.GetValueSafe<string>("Image"),
				Title = dictionary.GetValueSafe<string>("Title"),
				Body = dictionary.GetValueSafe<string>("Body"),
				Delay = dictionary.GetValueSafe<int>("Delay"),
				CreatedOn = dictionary.GetValueSafe<DateTime>("CreatedOn")
			};
		}
	}
}