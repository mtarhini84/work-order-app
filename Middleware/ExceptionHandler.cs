using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using WorkOrderApp.Controllers;
using WorkOrderApp.Exceptions;
using System.Net;

namespace WorkOrderApp.Middleware
{
	public class ExceptionHandler
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ExceptionHandler> _logger;

		public ExceptionHandler(RequestDelegate next, ILoggerFactory loggerFactory)
		{
			_next = next;
			_logger = loggerFactory.CreateLogger<ExceptionHandler>();
		}

		public async Task Invoke(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unhandled exception");
				await HandleExceptionAsync(context, ex);
			}
		}

		private static Task HandleExceptionAsync(HttpContext context, Exception ex)
		{
			var APIResult = new ApiResultModel { Success = false };

			var exception = ex as BaseException;

			string? message = exception is null ? ex.Message : exception.ErrorMessage;
			APIResult.Message = $"An error occured: {message}.";

			context.Response.StatusCode = exception is null ? (int)HttpStatusCode.InternalServerError : exception.StatusCode;

			var payload = JsonConvert.SerializeObject(APIResult);
			context.Response.ContentType = "application/json";
			return context.Response.WriteAsync(payload);
		}
	}
}
