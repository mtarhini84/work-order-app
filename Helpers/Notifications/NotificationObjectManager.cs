namespace WorkOrderApp.Helpers.Notifications
{
	public class NotificationObjectManager
	{
		private readonly NotificationService _notificationService;
		private readonly BlobService _blobService;


		public NotificationObjectManager(NotificationService notificationService, BlobService blobService)
		{
			_notificationService = notificationService;
			_blobService = blobService;
		}

		public async Task<bool> CreateNotificationObject(NotificationObjectDto data, IFormFile? formFile)
		{
			var customerId = data.CustomerId;

			if (string.IsNullOrEmpty(customerId))
			{
				customerId = "all";
			}

			NotificationObject notiObj = new NotificationObject(data);

			if (formFile != null)
			{
				using var memoryStream = new MemoryStream();
				formFile.CopyTo(memoryStream);
				string fileExtension = Path.GetExtension(formFile.FileName).ToLowerInvariant();
				memoryStream.Position = 0;

				var fileName = notiObj.Id + fileExtension;
				var uploadedBlob = await _blobService.CreateBlobAsync("entities", $"Notifications", fileName, memoryStream, true);

				if (uploadedBlob)
				{
					notiObj.Image = await _blobService.GetBlobLinkAsync("entities", $"Notifications", fileName);
				}
			}

			bool result = await _notificationService.CreateNotificationObject(notiObj);

			if (result)
			{
				await _notificationService.EnqueueNotification(new SendNotificationObject { 
					Id = customerId, 
					Title = data.Title, 
					Message = data.Body,
					ContentAvailable = data.ContentAvailable ?? false
				});
			}

			return result;
		}

		public async Task<IList<string>> GetNotificationsReadByUser(string id)
		{
			return await _notificationService.GetCustomerReadNotificationIds(id);
		}

		public async Task<bool> MarkNotificationReadByUser(string userId, string notificationId)
		{
			return await _notificationService.NotificationReadAsync(userId, notificationId);
		}

		public async Task<IList<NotificationModel>> GetUserNotifications(string userId, DateTime? registeredDate)
		{

			var list = new List<NotificationModel>();

			var dataList = new List<NotificationObject>();


			//var allNotifications = await _notificationService.GetNotificationsListing(registeredDate);

			//if (allNotifications != null)
			//{
			//    dataList.AddRange(allNotifications);
			//}

			var userSpecificNotifications = await _notificationService.GetNotificationObjectByUser(userId);

			if (userSpecificNotifications != null)
			{
				dataList.AddRange(userSpecificNotifications);
			}

			if (dataList.Count > 0)
			{
				dataList.OrderByDescending(a => a.CreatedOn).Take(100);

				list = dataList.Select(NotificationModel.FromNotificationObject).ToList();
				if (list.Count > 0)
				{
					var readNotifications = await _notificationService.GetCustomerReadNotificationIds(userId);
					foreach (var item in list)
					{
						if (readNotifications.Contains(item.Id))
						{
							item.Read = true;
						}
					}
				}
			}

			return list.OrderByDescending(a => a.DateTime).ToList();

		}

		public async Task<List<NotificationObject>> GetNotificationsListing()
		{
			var result = await _notificationService.GetNotificationsListing(null);
			return result.OrderBy(a => a.CreatedOn).ToList();
		}

		public async Task<bool> CreateDelayedNotificationObject(NotificationObjectDto data, IFormFile? formFile, int delayInMinutes = 15)
		{
			var customerId = data.CustomerId;

			if (string.IsNullOrEmpty(customerId))
			{
				customerId = "all";
			}

			DelayedNotificationObject notiObj = new DelayedNotificationObject(data);
			notiObj.Delay = delayInMinutes;

			if (formFile != null)
			{
				using var memoryStream = new MemoryStream();
				formFile.CopyTo(memoryStream);
				string fileExtension = Path.GetExtension(formFile.FileName).ToLowerInvariant();
				memoryStream.Position = 0;

				var fileName = notiObj.Id + fileExtension;
				var uploadedBlob = await _blobService.CreateBlobAsync("entities", $"Notifications", fileName, memoryStream, true);

				if (uploadedBlob)
				{
					notiObj.Image = await _blobService.GetBlobLinkAsync("entities", $"Notifications", fileName);
				}
			}

			bool result = await _notificationService.CreateDelayedNotificationObject(notiObj);

			return result;
		}
	}
}
