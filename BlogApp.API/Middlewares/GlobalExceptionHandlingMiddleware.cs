using BlogApp.API.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace BlogApp.API.Middlewares
{
    public class GlobalExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger _logger;

        public GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (ValidationException ve)
            {
                HandleException(context, HttpStatusCode.BadRequest, ve.Message, ve);
            }
            catch (BadRequestException bre)
            {
                _logger.LogError(bre, bre.Message);
                HandleException(context, HttpStatusCode.BadRequest, bre.Message, bre);
            }
            // Uncomment if needed
            //catch (InvalidOperationException ioe)
            //{
            //    HandleException(context, HttpStatusCode.BadRequest, ioe.Message, ioe);
            //}
            catch (NotFoundException nfe)
            {
                HandleException(context, HttpStatusCode.NotFound, nfe.Message, nfe);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                HandleException(context, HttpStatusCode.InternalServerError, e.Message, e);
            }
        }

        private async void HandleException(
            HttpContext context,
            HttpStatusCode statusCode,
            string title,
            Exception e
        )
        {
            context.Response.StatusCode = (int)statusCode;

            var problem = new ProblemDetails
            {
                Status = (int)statusCode,
                Type =
                    statusCode == HttpStatusCode.InternalServerError
                        ? "Server Error"
                        : "Bad Request",
                Title = title,
                // Uncomment if needed
                //Detail = exception.ToString()
            };

            var json = JsonSerializer.Serialize(problem);
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(json);
        }
    }
}
