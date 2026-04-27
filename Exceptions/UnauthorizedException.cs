using System.Net;

namespace WorkOrderApp.Exceptions
{
	public class UnauthorizedException : BaseException
	{
		public UnauthorizedException(string message = "Unauthorized") 
		{ 
			StatusCode = (int)HttpStatusCode.Unauthorized;
			ErrorMessage = message;
		}
	}
}
