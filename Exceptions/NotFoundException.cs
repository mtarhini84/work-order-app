using System.Net;

namespace WorkOrderApp.Exceptions
{
	public class NotFoundException : BaseException
	{
		public NotFoundException(string message = "Not Found")
		{
			StatusCode = (int)HttpStatusCode.NotFound;
			ErrorMessage = message;
		}
	}
}
