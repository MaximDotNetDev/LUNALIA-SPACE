using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Contracts.DTOs.Operations.Grades;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.Grade.GetGradeById;

public sealed class GetGradeByIdQueryHandler(
    IGradeRepository gradeRepository,
    ILessonRepository lessonRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetGradeByIdQuery, ErrorOr<GradeResponse>>
{
    public async Task<ErrorOr<GradeResponse>> Handle(GetGradeByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var grade = await gradeRepository.GetByIdAsync(request.GradeId, cancellationToken).ConfigureAwait(false);
        if (grade is null || grade.IsDeleted)
        {
            return Error.NotFound(
                code: "Grade.NotFound",
                description: "Оцінку не знайдено або вона була видалена.");
        }

        var currentRole = currentUserService.GetUserRole();
        var currentUserId = currentUserService.GetUserId();

        if (currentRole == RoleType.Teacher)
        {
            var isOwner = await lessonRepository.VerifyLessonOwnershipAsync(grade.LessonId, currentUserId, cancellationToken).ConfigureAwait(false);
            if (!isOwner)
            {
                return Error.Forbidden(
                    code: "Grade.Forbidden",
                    description: "Ви не маєте права переглядати оцінки за урок, який ви не ведете.");
            }
        }

        var response = new GradeResponse(
            grade.GradeId,
            grade.LessonId,
            grade.StudentId,
            grade.GradeValue,
            grade.Comment,
            grade.CreatedByUserId,
            grade.UpdatedByUserId,
            grade.GradeTypeId,
            grade.CreatedAt,
            grade.UpdatedAt,
            Convert.ToBase64String(grade.RowVersion.ToArray())
        );

        return response;
    }
}