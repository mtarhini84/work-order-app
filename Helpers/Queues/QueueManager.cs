namespace WorkOrderApp.Helpers.Queues
{
	public class QueueManager
    {
        private readonly IQueueService _queueService;
        public QueueManager(IQueueService queueService)
        {
            _queueService = queueService;
        }

        public async Task<bool> EnqueueMessageAsync(string queueName, string content)
        {
            return await _queueService.EnqueueMessageAsync(queueName, content);
        }
        public async Task<QueueMessage> DequeueMessageAsync(string queueName, int messageCount)
        {
            return await _queueService.DequeueMessageAsync(queueName, messageCount);
        }
    }
}
