using System.Net;

namespace WorkOrderApp.Exceptions
{
	public class ForbiddenException : BaseException
	{
		public ForbiddenException(string message = "Forbidden")
		{
			StatusCode = (int)HttpStatusCode.Forbidden;
			ErrorMessage = message;
		}
	}
}
