using System.Net;

namespace WorkOrderApp.Exceptions
{
	public class InternalErrorException : BaseException
	{
		public InternalErrorException(string message = "Internal Server Error")
		{
			StatusCode = (int)HttpStatusCode.InternalServerError;
			ErrorMessage = message;
		}
	}
}
