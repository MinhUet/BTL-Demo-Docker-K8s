using Auth.AuthenticationConfig;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Nancy.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auth.MiddleWares
{
    public class AuthenMiddleWare
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenMiddleWare> _logger;
        private readonly Authentication _authenticationConfig;
        public AuthenMiddleWare(Authentication authenticationConfig,
            ILogger<AuthenMiddleWare> logger, RequestDelegate next)
        {
            _authenticationConfig = authenticationConfig;
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var headers = context.Request.Headers;
            StringValues bearerToken = new StringValues();
            headers.TryGetValue("Authorization", out bearerToken);
            if(bearerToken.Count > 0)
            {
                try
                {
                    var accessToken = bearerToken.ToString().Split(' ').ToList()[1];
                    if (accessToken != null)
                    {
                        var decodedToken = _authenticationConfig.DecodeToken(accessToken);
                        if (decodedToken != null)
                        {
                            var json_serializer = new JavaScriptSerializer();
                            var claims = (IDictionary<string, object>)json_serializer.DeserializeObject(decodedToken);
                            var expiredTimeStamp = Convert.ToInt64(claims["Exp"].ToString());
                            var now = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
                            context.Items.Add("x-full-name", claims["Full-name"].ToString());
                            context.Items.Add("x-userID", claims["UserID"].ToString());
                        }
                    }
                }
                catch (SecurityTokenException ex)
                {
                    _logger.LogError("Localhost exception in AuthenMiddleWare: " + ex.ToString());
                    context.Items.Add("TenantId", "-1");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Localhost exception in AuthenMiddleWare: " + ex.ToString());
                }
            }
            await _next(context);
        }
    }
}
