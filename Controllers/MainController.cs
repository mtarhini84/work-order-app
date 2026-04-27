using Microsoft.AspNetCore.Mvc;
using WorkOrderApp.Exceptions;
using System.Security.Claims;

namespace WorkOrderApp.Controllers
{
    public abstract class MainController : ControllerBase
    {
        protected IActionResult IntoActionResult<T>(T result, string message = "An error occurred.")
        {
            var apiResult = new ApiResultModel();

            if (result is null)
            {
                apiResult.Success = false;
                apiResult.Message = message;
                return BadRequest(apiResult);
            }

            apiResult.Success = true;

            if (result is AuthResult auth)
            {
                apiResult.Token = auth.Token;
                apiResult.Data  = auth.Data;
            }
            else
            {
                apiResult.Data = result;
            }

            return Ok(apiResult);
        }

        protected string GetUserId()
            => HttpContext.User.FindFirst(ClaimTypes.Sid)?.Value
               ?? throw new ForbiddenException();

        protected string GetUserRole()
            => HttpContext.User.FindFirst(ClaimTypes.Role)?.Value
               ?? throw new ForbiddenException();
    }
}
