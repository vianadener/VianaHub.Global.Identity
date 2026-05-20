using EBL.FIG.Common.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Api.Endpoints.Base;
using VianaHub.Global.Identity.Api.Helpers;
using VianaHub.Global.Identity.Application.Dto.Request.Auth;
using VianaHub.Global.Identity.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace VianaHub.Global.Identity.Api.Endpoints;

[EndpointMapper]
public static class AuthEndpoint
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var groupV1 = app.MapGroup("/v1/auth").WithTags("Auth").WithGroupName("v1").AllowAnonymous();

        groupV1.MapPost("/register", async ([FromBody] RegisterRequest request, [FromServices] IAuthAppService authService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var result = await authService.RegisterAsync(request, ct);
            return notify.CustomResponse(result);
        })
        .WithName("Auth.Register")
        .RequireRateLimiting("authentication")
        .WithValidation<RegisterRequest>()
        .WithSummary("Swagger.Endpoint.Auth.Register.Summary")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        groupV1.MapPost("/login", async ([FromBody] LoginRequest request, [FromServices] IAuthAppService authService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var result = await authService.LoginAsync(request, ct);
            return notify.CustomResponse(result);
        })
        .WithName("Auth.Login")
        .RequireRateLimiting("authentication")
        .WithValidation<LoginRequest>()
        .WithSummary("Swagger.Endpoint.Auth.Login.Summary")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        groupV1.MapPost("/refresh", async ([FromBody] RefreshRequest request, [FromServices] IAuthAppService authService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var result = await authService.RefreshAsync(request, ct);
            return notify.CustomResponse(result);
        })
        .WithName("Auth.Refresh")
        .RequireRateLimiting("refreshtoken")
        .WithValidation<RefreshRequest>()
        .WithSummary("Swagger.Endpoint.Auth.Refresh.Summary")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        groupV1.MapPost("/logout", async ([FromBody] RevokeRequest request, [FromServices] IAuthAppService authService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            await authService.LogoutAsync(request, ct);
            return notify.CustomResponse<object>(null);
        })
        .WithName("Auth.Logout")
        .RequireAuthorization()
        .WithSummary("Swagger.Endpoint.Auth.Logout.Summary")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        groupV1.MapPost("/forgot-password", async ([FromBody] ForgotPasswordRequest request, [FromServices] IForgotPasswordAppService forgotPasswordService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var result = await forgotPasswordService.ForgotPasswordAsync(request, ct);
            return notify.CustomResponse(result);
        })
        .WithName("Auth.ForgotPassword")
        .RequireRateLimiting("authentication")
        .WithValidation<ForgotPasswordRequest>()
        .WithSummary("Swagger.Endpoint.Auth.ForgotPassword.Summary")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        groupV1.MapGet("/reset-password/validate", async ([AsParameters] ValidateResetTokenRequest request, [FromServices] IForgotPasswordAppService forgotPasswordService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var result = await forgotPasswordService.ValidateResetTokenAsync(request, ct);
            return notify.CustomResponse(result);
        })
        .WithName("Auth.ValidateResetToken")
        .WithValidation<ValidateResetTokenRequest>()
        .WithSummary("Swagger.Endpoint.Auth.ValidateResetToken.Summary")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        groupV1.MapPost("/reset-password", async ([FromBody] ResetPasswordRequest request, [FromServices] IForgotPasswordAppService forgotPasswordService, [FromServices] INotify notify, HttpContext httpContext, CancellationToken ct) =>
        {
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = httpContext.Request.Headers.UserAgent.ToString();
            var result = await forgotPasswordService.ResetPasswordAsync(request, ipAddress, userAgent, ct);
            return notify.CustomResponse(result);
        })
        .WithName("Auth.ResetPassword")
        .RequireRateLimiting("authentication")
        .WithValidation<ResetPasswordRequest>()
        .WithSummary("Swagger.Endpoint.Auth.ResetPassword.Summary")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status410Gone);
    }
}
