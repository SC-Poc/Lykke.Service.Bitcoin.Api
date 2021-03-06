﻿using System;
using System.IO;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Common.ApiLibrary.Exceptions;
using Lykke.Common.Log;
using Lykke.Service.Bitcoin.Api.Core.Services.Exceptions;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Lykke.Service.Bitcoin.Api.Middleware
{
    public class CustomGlobalErrorHandlerMiddleware
    {
        private readonly ILog _log;
        private readonly RequestDelegate _next;

        public CustomGlobalErrorHandlerMiddleware(RequestDelegate next, ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (ValidationApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                LogError(context, ex);
                await CreateErrorResponse(context, ex);
            }
        }

        private void LogError(HttpContext context, Exception ex)
        {
            _log.Warning(ReadBody(context), ex, context.Request.GetUri().AbsoluteUri);
        }

        private async Task CreateErrorResponse(HttpContext ctx, Exception ex)
        {
            ctx.Response.ContentType = "application/json";
            ctx.Response.StatusCode = GetStatusCode(ex);

            var response = ErrorResponse.Create(ex is BusinessException ? ex.Message : ex.ToString());

            var responseJson = JsonConvert.SerializeObject(response);

            await ctx.Response.WriteAsync(responseJson);
        }

        private int GetStatusCode(Exception ex)
        {
            switch (ex)
            {
                case BusinessException e when e.Code == ErrorCode.Conflict:
                    return 409;
                case BusinessException e when e.Code == ErrorCode.BadInputParameter:
                    return 400;
                default:
                    return 500;
            }
        }

        private string ReadBody(HttpContext context)
        {
            var body = string.Empty;

            if (context.Request.Body.CanSeek &&
                +context.Request.Body.Length > 0)
            {
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                body = new StreamReader(context.Request.Body).ReadToEnd();
            }

            return body;
        }
    }
}
