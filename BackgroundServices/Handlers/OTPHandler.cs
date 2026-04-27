using WorkOrderApp.Services.OTP;
using Newtonsoft.Json;
using WorkOrderApp.Helpers.Queues;

namespace WorkOrderApp.BackgroundServices.Handlers
{
	public class OTPHandler : IQueueHandler
	{
		private readonly OTPService _otpService;

		public OTPHandler(OTPService otpService)
		{
			_otpService = otpService;
		}

		public async Task ExecuteMessage(QueueMessage message)
		{
			if (message != null && !string.IsNullOrEmpty(message.Content))
			{
				var payload = JsonConvert.DeserializeObject<SendOTPModel>(message.Content);

				if (payload != null && !string.IsNullOrEmpty(payload.MobileNumber))
				{
					if (payload.IsSMS)
						await _otpService.SendOTPTextMessage(payload.MobileNumber);
					else
						await _otpService.SendWhatsAppOTP(payload.MobileNumber);
				}
			}
		}
	}
}
