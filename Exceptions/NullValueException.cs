using System.Net;

namespace WorkOrderApp.Exceptions
{
	public class NullValueException : BaseException
	{
		public NullValueException(string message = "Null Value")
		{
			StatusCode = (int)HttpStatusCode.NotAcceptable;
			ErrorMessage = message;
		}
	}
}
