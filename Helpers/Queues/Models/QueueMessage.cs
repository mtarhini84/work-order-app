namespace WorkOrderApp.Helpers.Queues
{
    public class QueueMessage
    {
        public string Content { get; set; }
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset EnqueuedAt { get; set; } = DateTime.UtcNow;
        public string MessageType { get; set; } = string.Empty;
        public int RetryCount { get; set; } = 0;
        public int MaxRetries { get; set; } = 3;
    }
}
