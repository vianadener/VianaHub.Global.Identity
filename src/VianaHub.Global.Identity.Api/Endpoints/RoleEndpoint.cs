using EBL.FIG.Common.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Api.Endpoints.Base;
using VianaHub.Global.Identity.Api.Helpers;
using VianaHub.Global.Identity.Application.Dto.Base;
using VianaHub.Global.Identity.Application.Dto.Request.Role;
using VianaHub.Global.Identity.Application.Dto.Response.Role;
using VianaHub.Global.Identity.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace VianaHub.Global.Identity.Api.Endpoints;

[EndpointMapper]
public static class RoleEndpoint
{
    public static void MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var groupV1 = app.MapGroup("/v1/roles").WithTags("Roles").WithGroupName("v1").RequireAuthorization();

        groupV1.MapGet("/", async ([FromServices] IRoleAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var result = await appService.GetAllAsync(ct);
            return notify.CustomResponse(result);
        })
        .CustomAuthorize("Admin,BackOffice,Manager", "Roles", "Read")
        .WithName("GetAllRoles")
        .WithSummary("Swagger.Endpoint.Role.GetAll.Summary")
        .Produces<IEnumerable<RoleResponse>>(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        groupV1.MapGet("/{id}", async ([FromRoute] int id, [FromServices] IRoleAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var result = await appService.GetByIdAsync(id, ct);
            return notify.CustomResponse(result);
        })
        .CustomAuthorize("Admin,BackOffice,Manager", "Roles", "Read")
        .WithName("GetRoleById")
        .WithSummary("Swagger.Endpoint.Role.GetById.Summary")
        .Produces<RoleResponse>(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        groupV1.MapGet("/paged", async ([AsParameters] PagedFilterRequest request, [FromServices] IRoleAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var result = await appService.GetPagedAsync(request, ct);
            return notify.CustomResponse(result);
        })
        .CustomAuthorize("Admin,BackOffice,Manager", "Roles", "Read")
        .WithName("GetPagedRoles")
        .WithSummary("Swagger.Endpoint.Role.GetPaged.Summary")
        .Produces<ListPageResponse<RoleResponse>>(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);


        groupV1.MapPost("/", async ([FromBody] CreateRoleRequest request, [FromServices] IRoleAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var created = await appService.CreateAsync(request, ct);
            return notify.CustomResponse(201);
        })
        .CustomAuthorize("Admin,BackOffice,Manager", "Roles", "Create")
        .WithName("CreateRole")
        .WithSummary("Swagger.Endpoint.Role.Create.Summary")
        .Produces(StatusCodes.Status201Created)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError)
        .WithValidation<CreateRoleRequest>();

        groupV1.MapPut("/{id}", async ([FromRoute] int id, [FromBody] UpdateRoleRequest request, [FromServices] IRoleAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var ok = await appService.UpdateAsync(id, request, ct);
            return notify.CustomResponse();
        })
        .CustomAuthorize("Admin,BackOffice,Manager", "Roles", "Update")
        .WithName("UpdateRole")
        .WithSummary("Swagger.Endpoint.Role.Update.Summary")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError)
        .WithValidation<UpdateRoleRequest>();

        groupV1.MapPatch("/{id}/activate", async ([FromRoute] int id, [FromServices] IRoleAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var ok = await appService.ActivateAsync(id, ct);
            return notify.CustomResponse();
        })
        .CustomAuthorize("Admin,BackOffice,Manager", "Roles", "Activate")
        .WithName("ActivateRole")
        .WithSummary("Swagger.Endpoint.Role.Activate.Summary")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        groupV1.MapPatch("/{id}/deactivate", async ([FromRoute] int id, [FromServices] IRoleAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var ok = await appService.DeactivateAsync(id, ct);
            return notify.CustomResponse();
        })
        .CustomAuthorize("Admin,BackOffice,Manager", "Roles", "Deactivate")
        .WithName("DeactivateRole")
        .WithSummary("Swagger.Endpoint.Role.Deactivate.Summary")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        groupV1.MapDelete("/{id}", async ([FromRoute] int id, [FromServices] IRoleAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var ok = await appService.DeleteAsync(id, ct);
            return notify.CustomResponse();
        })
        .CustomAuthorize("Admin,BackOffice,Manager", "Roles", "Delete")
        .WithName("DeleteRole")
        .WithSummary("Swagger.Endpoint.Role.Delete.Summary")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        // Upload massivo de roles via CSV
        groupV1.MapPost("/bulk-upload", async (HttpRequest request, [FromServices] IRoleAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            if (!request.HasFormContentType || request.Form.Files.Count == 0)
            {
                notify.Add("Api.Upload.NoFileProvided", 400);
                return notify.CustomResponse();
            }

            var file = request.Form.Files[0];
            var success = await appService.BulkUploadAsync(file, ct);
            return notify.CustomResponse(success);
        })
        .CustomAuthorize("Admin,BackOffice,Manager", "Roles", "BulkUpload")
        .WithName("BulkUploadRoles")
        .WithSummary("Swagger.Endpoint.Role.BulkUpload.Summary")
        .DisableAntiforgery()
        .Accepts<IFormFile>("multipart/form-data")
        .Produces(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);
    }
}
