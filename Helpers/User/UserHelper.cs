using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WorkOrderApp.Helpers.User
{
    public static class UserHelper
    {
        public static (string Id, string Name) GetCurrentUser(this ControllerBase controller)
        {
            var userId = controller.User.FindFirst("UserId")?.Value ??
                        controller.User.FindFirst("sub")?.Value ??
                        controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                        "system";

            var userName = controller.User.FindFirst("UserName")?.Value ??
                          controller.User.FindFirst("name")?.Value ??
                          controller.User.FindFirst(ClaimTypes.Name)?.Value ??
                          controller.User.Identity?.Name ??
                          "System User";

            return (userId, userName);
        }

        public static string GetCurrentUserId(this ControllerBase controller)
        {
            return controller.User.FindFirst("UserId")?.Value ??
                   controller.User.FindFirst("sub")?.Value ??
                   controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                   "system";
        }

        public static string GetCurrentUserName(this ControllerBase controller)
        {
            return controller.User.FindFirst("UserName")?.Value ??
                   controller.User.FindFirst("name")?.Value ??
                   controller.User.FindFirst(ClaimTypes.Name)?.Value ??
                   controller.User.Identity?.Name ??
                   "System User";
        }
    }
}
