using VianaHub.Global.Middleware.Lib.Notifications;
using VianaHub.Global.Identity.Api.Endpoints.Base;
using VianaHub.Global.Identity.Api.Helpers;
using VianaHub.Global.Identity.Application.Dto.Request.Job;
using VianaHub.Global.Identity.Application.Interfaces;
using VianaHub.Global.Identity.Domain.ReadModels;
using Microsoft.AspNetCore.Mvc;

namespace VianaHub.Global.Identity.Api.Endpoints;

[EndpointMapper]
public static class JobEndpoint
{
    public static void MapJobEndpoints(this IEndpointRouteBuilder app)
    {
        var groupV1 = app.MapGroup("/v1/job-definitions").WithTags("JobDefinitions").WithGroupName("v1").RequireAuthorization();

        groupV1.MapGet("/", async ([FromServices] IJobAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var response = await appService.GetAllAsync(ct);
            return notify.CustomResponse(response);
        })
        .CustomAuthorize("Admin,BackOffice", "JobDefinitions", "Read")
        .WithName("GetAllJob")
        .WithSummary("Swagger.Endpoint.Job.GetAll.Summary")
        .Produces(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        groupV1.MapGet("/{id}", async ([FromRoute] int id, [FromServices] IJobAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var response = await appService.GetByIdAsync(id, ct);
            return notify.CustomResponse(response);
        })
        .CustomAuthorize("Admin,BackOffice", "JobDefinitions", "Read")
        .WithName("GetJobById")
        .WithSummary("Swagger.Endpoint.Job.GetById.Summary")
        .Produces(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        groupV1.MapGet("/paged", async ([AsParameters] JobPagedFilter request, [FromServices] IJobAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var response = await appService.GetPagedAsync(request, ct);
            return notify.CustomResponse(response);
        })
        .CustomAuthorize("Admin,BackOffice", "JobDefinitions", "Read")
        .WithName("GetPagedJobs")
        .WithSummary("Swagger.Endpoint.Job.GetPaged.Summary")
        .Produces(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        groupV1.MapPost("/", async ([FromBody] CreateJobRequest request, [FromServices] IJobAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var response = await appService.CreateAsync(request, ct);
            return notify.CustomResponse(response, 201);
        })
        .CustomAuthorize("Admin,BackOffice", "JobDefinitions", "Create")
        .WithName("CreateJob")
        .WithSummary("Swagger.Endpoint.Job.Create.Summary")
        .Produces(StatusCodes.Status201Created)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError)
        .WithValidation<CreateJobRequest>();

        groupV1.MapPost("/{id}/execute", async ([FromRoute] int id, [FromServices] IJobAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var response = await appService.ExecuteAsync(id, ct);
            return notify.CustomResponse(response);
        })
        .CustomAuthorize("Admin,BackOffice", "JobDefinitions", "Execute")
        .WithName("ExecuteJob")
        .WithSummary("Swagger.Endpoint.Job.Execute.Summary")
        .Produces(StatusCodes.Status200OK)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        groupV1.MapPut("/{id}", async ([FromRoute] int id, [FromBody] UpdateJobRequest request, [FromServices] IJobAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var updated = await appService.UpdateAsync(id, request, ct);
            return notify.CustomResponse(updated, 200);
        })
        .CustomAuthorize("Admin,BackOffice", "JobDefinitions", "Update")
        .WithName("UpdateJob")
        .WithSummary("Swagger.Endpoint.Job.Update.Summary")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError)
        .WithValidation<UpdateJobRequest>();

        groupV1.MapPatch("/{id}/activate", async ([FromRoute] int id, [FromServices] IJobAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var ok = await appService.ActivateAsync(id, ct);
            return notify.CustomResponse(ok);
        })
        .CustomAuthorize("Admin,BackOffice", "JobDefinitions", "Activate")
        .WithName("ActivateJob")
        .WithSummary("Swagger.Endpoint.Job.Activate.Summary")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        groupV1.MapPatch("/{id}/deactivate", async ([FromRoute] int id, [FromServices] IJobAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var ok = await appService.DeactivateAsync(id, ct);
            return notify.CustomResponse(ok);
        })
        .CustomAuthorize("Admin,BackOffice", "JobDefinitions", "Deactivate")
        .WithName("DeactivateJob")
        .WithSummary("Swagger.Endpoint.Job.Deactivate.Summary")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);

        groupV1.MapDelete("/{id}", async ([FromRoute] int id, [FromServices] IJobAppService appService, [FromServices] INotify notify, CancellationToken ct) =>
        {
            var ok = await appService.DeleteAsync(id, ct);
            return notify.CustomResponse();
        })
        .CustomAuthorize("Admin,BackOffice", "JobDefinitions", "Delete")
        .WithName("DeleteJob")
        .WithSummary("Swagger.Endpoint.Job.Delete.Summary")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);
    }
}
