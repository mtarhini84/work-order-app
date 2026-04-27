using WorkOrderApp.Helpers.Queues;

namespace WorkOrderApp.BackgroundServices.Handlers
{
    public interface IQueueHandler
    {
        public Task ExecuteMessage(QueueMessage message);
    }
}
