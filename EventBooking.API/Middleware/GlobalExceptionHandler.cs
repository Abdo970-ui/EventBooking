using EventBooking.Application.Common.Exceptions;
using EventBooking.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var problem = new ProblemDetails { Instance = context.Request.Path };

        switch (exception)
        {
            case ValidationException v: // FluentValidation
                problem.Status = StatusCodes.Status400BadRequest;
                problem.Title = "Validation failed";
                problem.Extensions["errors"] = v.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                break;

            case BadRequestException or DomainException:
                problem.Status = StatusCodes.Status400BadRequest;
                problem.Title = "Bad request";
                problem.Detail = exception.Message;
                break;

            case UnauthorizedException:
                problem.Status = StatusCodes.Status401Unauthorized;
                problem.Title = "Unauthorized";
                problem.Detail = exception.Message;
                break;

            case ForbiddenException:
                problem.Status = StatusCodes.Status403Forbidden;
                problem.Title = "Forbidden";
                problem.Detail = exception.Message;
                break;

            case NotFoundException:
                problem.Status = StatusCodes.Status404NotFound;
                problem.Title = "Not found";
                problem.Detail = exception.Message;
                break;

            case ConflictException:
                problem.Status = StatusCodes.Status409Conflict;
                problem.Title = "Conflict";
                problem.Detail = exception.Message;
                break;

            case DbUpdateConcurrencyException:
                problem.Status = StatusCodes.Status409Conflict;
                problem.Title = "Conflict";
                problem.Detail = "The record was changed by another request. Please try again.";
                break;

            default:
                _logger.LogError(exception, "Unhandled exception");
                problem.Status = StatusCodes.Status500InternalServerError;
                problem.Title = "Server error";
                problem.Detail = "An unexpected error occurred.";
                break;
        }

        context.Response.StatusCode = problem.Status!.Value;
        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken: ct);
        return true;
    }
}