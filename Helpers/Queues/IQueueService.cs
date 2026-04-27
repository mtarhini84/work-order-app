namespace WorkOrderApp.Helpers.Queues
{
	public interface IQueueService
    {
        Task<bool> EnqueueMessageAsync(string queueName, string content);

        Task<QueueMessage> DequeueMessageAsync(string queueName, int messageCount);
    }
}
