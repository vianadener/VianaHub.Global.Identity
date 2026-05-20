using EBL.FIG.Common.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Api.Endpoints.Base;
using VianaHub.Global.Identity.Api.Helpers;
using VianaHub.Global.Identity.Application.Dto.Request.UserRole;
using VianaHub.Global.Identity.Application.Dto.Response.UserRole;
using VianaHub.Global.Identity.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace VianaHub.Global.Identity.Api.Endpoints;

[EndpointMapper]
public static class UserRoleEndpoint
{
    public static void MapUserRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var groupV1 = app.MapGroup("/v1/user-roles").WithTags("UserRoles").WithGroupName("v1").RequireAuthorization();

        groupV1.MapGet("/", async ([FromServices] IUserRoleAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var result = await appService.GetAllAsync(ct);
            return notify.CustomResponse(result);
        })
        .CustomAuthorize("Admin,BackOffice,Manager,Operator", "UserRoles", "Read")
        .WithName("GetAllUserRoles")
        .WithSummary("Swagger.Endpoint.UserRole.GetAll.Summary")
        .Produces<IEnumerable<UserRoleResponse>>(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        groupV1.MapGet("/{id}", async ([FromRoute] int id, [FromServices] IUserRoleAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var result = await appService.GetByIdAsync(id, ct);
            return notify.CustomResponse(result);
        })
        .CustomAuthorize("Admin,BackOffice,Manager,Operator", "UserRoles", "Read")
        .WithName("GetUserRoleById")
        .WithSummary("Swagger.Endpoint.UserRole.GetById.Summary")
        .Produces<UserRoleResponse>(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        groupV1.MapPost("/", async ([FromBody] CreateUserRoleRequest request, [FromServices] IUserRoleAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var created = await appService.CreateAsync(request, ct);
            return notify.CustomResponse(201);
        })
        .CustomAuthorize("Admin,BackOffice,Manager", "UserRoles", "Create")
        .WithName("CreateUserRole")
        .WithSummary("Swagger.Endpoint.UserRole.Create.Summary")
        .Produces(StatusCodes.Status201Created)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError)
        .WithValidation<CreateUserRoleRequest>();

        groupV1.MapDelete("/{id}", async ([FromRoute] int id,[FromServices] IUserRoleAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            await appService.DeleteAsync(id, ct);
            return notify.CustomResponse();
        })
        .CustomAuthorize("Admin,BackOffice", "UserRoles", "Delete")
        .WithName("DeleteUserRole")
        .WithSummary("Swagger.Endpoint.UserRole.Delete.Summary")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        groupV1.MapPost("/bulk-upload", async (HttpRequest request, [FromServices] IUserRoleAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            if (!request.HasFormContentType)
            {
                notify.Add("Api.Upload.NoFileProvided", 400);
                return notify.CustomResponse();
            }

            var form = await request.ReadFormAsync(ct);
            if (form.Files.Count == 0)
            {
                notify.Add("Api.Upload.NoFileProvided", 400);
                return notify.CustomResponse();
            }

            var file = form.Files[0];
            var success = await appService.BulkUploadAsync(file, ct);
            return notify.CustomResponse(success);
        })
        .CustomAuthorize("Admin,BackOffice,Manager", "UserRoles", "BulkUpload")
        .WithName("BulkUploadUserRole")
        .WithSummary("Swagger.Endpoint.UserRole.BulkUpload.Summary")
        .DisableAntiforgery()
        .Accepts<IFormFile>("multipart/form-data")
        .Produces(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);
    }
}
