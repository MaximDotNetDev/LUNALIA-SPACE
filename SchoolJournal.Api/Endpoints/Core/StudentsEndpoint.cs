using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SchoolJournal.Api.Common.Extensions;
using SchoolJournal.Application.Features.Core.Student.CreateStudent;
using SchoolJournal.Application.Features.Core.Student.DeleteStudent;
using SchoolJournal.Application.Features.Core.Student.GetStudentById;
using SchoolJournal.Application.Features.Core.Student.GetStudentsByClassId;
using SchoolJournal.Application.Features.Core.Student.LinkUserToStudent;
using SchoolJournal.Application.Features.Core.Student.TransferStudent;
using SchoolJournal.Application.Features.Core.Student.UpdateMedicalNotes;
using SchoolJournal.Application.Features.Core.Student.UpdateStudent;
using SchoolJournal.Application.Features.Core.Student.SearchStudents;
using SchoolJournal.Application.Features.Core.Student.GetStudentHistory;
using SchoolJournal.Contracts.DTOs.Core.Students;
using SchoolJournal.Domain.Enums.Identity;
using SchoolJournal.Contracts.Common;

namespace SchoolJournal.Api.Endpoints.Core;

internal static class StudentsEndpoint
{
    public static void MapStudents(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/students").WithTags("Students");

        group.MapGet("/{id:guid}", GetStudentById)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithName("GetStudentById")
            .WithSummary("Отримання деталей учня за ID")
            .Produces<StudentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateStudent)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateStudent)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", DeleteStudent)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/transfer", TransferStudent)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithSummary("Переведення учня до іншого класу")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/link-user", LinkUserToStudent)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithSummary("Прив'язка користувача до учня")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPatch("/{id:guid}/medical-notes", UpdateMedicalNotes)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithSummary("Оновлення медичних нотаток учня")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/class/{classId:guid}", GetStudentsByClassId)
            .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
            .WithSummary("Отримання списку учнів конкретного класу (для журналу)")
            .Produces<IEnumerable<StudentLookupResponse>>(StatusCodes.Status200OK);

        group.MapGet("/search", SearchStudents)
            .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Teacher)
            .WithSummary("Розширений пошук учнів з пагінацією")
            .Produces<PagedResponse<StudentSearchResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}/history", GetStudentHistory)
            .RequireRoles(RoleType.Admin, RoleType.Director)
            .WithSummary("Отримання історії змін картки учня (Temporal Table)")
            .Produces<IEnumerable<StudentHistoryResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/by-user/{userId:guid}", GetStudentByUserId)
            .RequireRoles(RoleType.Admin, RoleType.Director, RoleType.Student)
            .WithName("GetStudentByUserId")
            .WithSummary("Отримання деталей учня за його UserId")
            .Produces<StudentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetStudentById(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetStudentByIdQuery(id), ct).ConfigureAwait(false);
        return result.Match(Results.Ok, errors => errors[0].Type == ErrorType.NotFound
            ? Results.NotFound(new { errors[0].Description })
            : Results.BadRequest(new { errors[0].Description }));
    }

    private static async Task<IResult> CreateStudent([FromBody] CreateStudentRequest request, ISender sender, CancellationToken ct)
    {
        var command = new CreateStudentCommand(
            request.LastName, request.FirstName, request.MiddleName, request.DateOfBirth,
            request.ClassId, request.Gender, request.DocumentType, request.DocumentSeries,
            request.DocumentNumber, request.EnrollmentDate, request.EnrollmentReason,
            request.Address, request.MedicalNotes, request.UserId);

        var result = await sender.Send(command, ct).ConfigureAwait(false);
        return result.Match(
            id => Results.CreatedAtRoute("GetStudentById", new { id }, new { StudentId = id }),
            errors => Results.Problem(
                statusCode: errors[0].Type == ErrorType.Conflict ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest,
                title: errors[0].Description));
    }

    private static async Task<IResult> UpdateStudent(Guid id, [FromBody] UpdateStudentRequest request, ISender sender, CancellationToken ct)
    {
        var command = new UpdateStudentCommand(
            id, request.LastName, request.FirstName, request.MiddleName, request.DateOfBirth,
            request.ClassId, request.Gender, request.DocumentType, request.DocumentSeries,
            request.DocumentNumber, request.EnrollmentDate, request.EnrollmentReason,
            request.Address, request.MedicalNotes, request.RowVersionBase64);

        var result = await sender.Send(command, ct).ConfigureAwait(false);
        return result.Match(_ => Results.NoContent(), errors => errors[0].Type switch
        {
            ErrorType.NotFound => Results.NotFound(new { errors[0].Description }),
            ErrorType.Conflict => Results.Conflict(new { errors[0].Description }),
            _ => Results.BadRequest(new { errors[0].Description })
        });
    }

    private static async Task<IResult> DeleteStudent(Guid id, [FromBody] DeleteStudentRequest request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteStudentCommand(id, request.RowVersionBase64), ct).ConfigureAwait(false);
        return result.Match(_ => Results.NoContent(), errors => errors[0].Type switch
        {
            ErrorType.NotFound => Results.NotFound(new { errors[0].Description }),
            ErrorType.Conflict => Results.Conflict(new { errors[0].Description }),
            _ => Results.BadRequest(new { errors[0].Description })
        });
    }

    private static async Task<IResult> TransferStudent(Guid id, [FromBody] TransferStudentRequest request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new TransferStudentCommand(id, request.NewClassId, request.RowVersionBase64), ct).ConfigureAwait(false);
        return result.Match(_ => Results.Ok(), errors => errors[0].Type switch
        {
            ErrorType.NotFound => Results.NotFound(new { errors[0].Description }),
            ErrorType.Conflict => Results.Conflict(new { errors[0].Description }),
            _ => Results.BadRequest(new { errors[0].Description })
        });
    }

    private static async Task<IResult> LinkUserToStudent(Guid id, [FromBody] LinkUserToStudentRequest request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new LinkUserToStudentCommand(id, request.UserId, request.RowVersionBase64), ct).ConfigureAwait(false);
        return result.Match(_ => Results.Ok(), errors => errors[0].Type switch
        {
            ErrorType.NotFound => Results.NotFound(new { errors[0].Description }),
            ErrorType.Conflict => Results.Conflict(new { errors[0].Description }),
            _ => Results.BadRequest(new { errors[0].Description })
        });
    }

    private static async Task<IResult> UpdateMedicalNotes(Guid id, [FromBody] UpdateMedicalNotesRequest request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateMedicalNotesCommand(id, request.MedicalNotes, request.RowVersionBase64), ct).ConfigureAwait(false);
        return result.Match(_ => Results.NoContent(), errors => errors[0].Type switch
        {
            ErrorType.NotFound => Results.NotFound(new { errors[0].Description }),
            ErrorType.Conflict => Results.Conflict(new { errors[0].Description }),
            _ => Results.BadRequest(new { errors[0].Description })
        });
    }

    private static async Task<IResult> GetStudentsByClassId(Guid classId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetStudentsByClassIdQuery(classId), ct).ConfigureAwait(false);
        return result.Match(Results.Ok, errors => Results.BadRequest(new { errors[0].Description }));
    }

    private static async Task<IResult> SearchStudents(
        [FromQuery] string? searchTerm,
        [FromQuery] Guid? classId,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        ISender sender,
        CancellationToken ct)
    {
        var query = new SearchStudentsQuery(searchTerm, classId, isActive, new PageRequest(pageNumber, pageSize));
        var result = await sender.Send(query, ct).ConfigureAwait(false);
        return result.Match(Results.Ok, errors => Results.BadRequest(new { errors[0].Description }));
    }

    private static async Task<IResult> GetStudentHistory(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetStudentHistoryQuery(id), ct).ConfigureAwait(false);
        return result.Match(Results.Ok, errors => errors[0].Type == ErrorType.NotFound
            ? Results.NotFound(new { errors[0].Description })
            : Results.BadRequest(new { errors[0].Description }));
    }

    private static async Task<IResult> GetStudentByUserId(Guid userId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new SchoolJournal.Application.Features.Core.Student.GetStudentByUserId.GetStudentByUserIdQuery(userId), ct).ConfigureAwait(false);

        return result.Match(Results.Ok, errors => errors[0].Type == ErrorType.NotFound
            ? Results.NotFound(new { errors[0].Description })
            : Results.BadRequest(new { errors[0].Description }));
    }
}