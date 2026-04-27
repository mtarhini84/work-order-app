namespace WorkOrderApp.Entities
{
    public enum RequestStatus
    {
        Pending,
        Approved,
        Done,
        Declined
    }

    public enum WorkOrderStatus
    {
        Open,
        OnHold,
        InProgress,
        Done
    }

    public enum Priority
    {
        Low,
        Medium,
        High
    }
}
