namespace WorkOrderApp.Exceptions
{
	public abstract class BaseException: Exception
	{
		public int StatusCode { get; set; }
		public string? ErrorMessage { get; set; }
	}
}
