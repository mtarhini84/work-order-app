using System.Net;

namespace WorkOrderApp.Exceptions
{
	public class BadRequestException : BaseException
	{
		public BadRequestException(string message = "Bad Request")
		{
			StatusCode = (int)HttpStatusCode.BadRequest;
			ErrorMessage = message;
		}
	}
}
