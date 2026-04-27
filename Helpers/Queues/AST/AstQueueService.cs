using Azure.Storage.Queues;

namespace WorkOrderApp.Helpers.Queues
{
	public class AstQueueService : IQueueService
	{
		private readonly QueueServiceClient _queueClient;

		public AstQueueService(IConfiguration configuration)
		{
			_queueClient = new QueueServiceClient(configuration.GetConnectionString("AST"));
		}
		public async Task<bool> EnqueueMessageAsync(string queueName, string content)
		{
			try
			{
				var queue = _queueClient.GetQueueClient(queueName);

				var base64Message = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(content));

				await queue.SendMessageAsync(base64Message);
			}
			catch (Exception ex)
			{
				throw new Exception();
			}

			return true;
		}

		public async Task<QueueMessage> DequeueMessageAsync(string queueName, int messageCount)
		{
			var queueClient = _queueClient.GetQueueClient(queueName);

			var receivedMessages = await queueClient.ReceiveMessagesAsync(messageCount);

			if (receivedMessages.Value.Count() > 0)
			{
				var receivedMessage = receivedMessages.Value.First();

				await queueClient.DeleteMessageAsync(receivedMessage.MessageId, receivedMessage.PopReceipt);

				var decodedMessage = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(receivedMessage.MessageText));

				return new QueueMessage
				{
					Content = decodedMessage,
				};
			}

			return null;
		}
	}
}
