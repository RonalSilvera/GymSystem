using Core.CustomEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;

namespace Infrastructure.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeFilter : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.HttpContext.User.Identity == null || !context.HttpContext.User.Identity.IsAuthenticated) {
            context.Result = new CustomResponseResult(StatusCodes.Status401Unauthorized, ReasonPhrases.GetReasonPhrase(StatusCodes.Status401Unauthorized), "Authorization failed.");
            context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
    }
}