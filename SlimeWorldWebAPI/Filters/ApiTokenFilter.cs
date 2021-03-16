using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using SlimeWorldWebAPI.Configs;

namespace SlimeWorldWebAPI.Filters
{
    public class ApiTokenFilter : IAuthorizationFilter
    {
        private readonly IOptions<AppSettings> _appSettings;

        public ApiTokenFilter(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings ?? throw new ArgumentException(nameof(appSettings));
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var apiToken = context.HttpContext.Request.Headers["Authorization"];
            if ("ApiToken " + _appSettings.Value.ApiToken != apiToken)
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
