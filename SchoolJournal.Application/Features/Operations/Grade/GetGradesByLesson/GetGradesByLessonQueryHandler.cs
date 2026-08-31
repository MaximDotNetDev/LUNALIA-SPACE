using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Contracts.DTOs.Operations.Grades;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.Grade.GetGradesByLesson;

public sealed class GetGradesByLessonQueryHandler(
    IGradeRepository gradeRepository,
    ILessonRepository lessonRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetGradesByLessonQuery, ErrorOr<IReadOnlyCollection<GradeResponse>>>
{
    public async Task<ErrorOr<IReadOnlyCollection<GradeResponse>>> Handle(GetGradesByLessonQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var currentRole = currentUserService.GetUserRole();
        var currentUserId = currentUserService.GetUserId();

        if (currentRole == RoleType.Teacher)
        {
            var isOwner = await lessonRepository.VerifyLessonOwnershipAsync(request.LessonId, currentUserId, cancellationToken).ConfigureAwait(false);
            if (!isOwner)
            {
                return Error.Forbidden(
                    code: "Grade.Forbidden",
                    description: "Ви не маєте права переглядати журнал оцінок для уроку, який ви не ведете.");
            }
        }

        var grades = await gradeRepository.GetByLessonIdAsync(request.LessonId, cancellationToken).ConfigureAwait(false);

        var response = grades.Select(grade => new GradeResponse(
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
        )).ToList().AsReadOnly();

        return response;
    }
}