using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using API.Errors;
using Microsoft.Extensions.Hosting.Internal;

namespace API.Middleware
{
    public class APIExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<APIExceptionMiddleware> logger;
        private readonly IHostEnvironment env;

        public APIExceptionMiddleware(RequestDelegate next, ILogger<APIExceptionMiddleware> logger,
        IHostEnvironment env)
        {
            this._next = next;
            this.logger = logger;
            this.env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = env.IsDevelopment() ?
                new APIException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString()):
                new APIException(context.Response.StatusCode, ex.Message, "Internal Server Error");

                var options = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);



            }


        }
    }
}