using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Contracts.DTOs.Operations.Grades;
using SchoolJournal.Domain.Entities.Core.IRepositories;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.Grade.GetGradesByStudent;

public sealed class GetGradesByStudentQueryHandler(
    IGradeRepository gradeRepository,
    IStudentRepository studentRepository,
    IParentRepository parentRepository,
    IStudentParentRepository studentParentRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetGradesByStudentQuery, ErrorOr<IReadOnlyCollection<GradeResponse>>>
{
    public async Task<ErrorOr<IReadOnlyCollection<GradeResponse>>> Handle(GetGradesByStudentQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var currentRole = currentUserService.GetUserRole();
        var currentUserId = currentUserService.GetUserId();

        if (currentRole == RoleType.Student)
        {
            var student = await studentRepository.GetByIdAsync(request.StudentId, cancellationToken).ConfigureAwait(false);
            if (student is null || student.IsDeleted || student.UserId != currentUserId)
            {
                return Error.Forbidden(
                    code: "Grade.StudentAccessDenied",
                    description: "Ви можете переглядати виключно власні оцінки.");
            }
        }
        else if (currentRole == RoleType.Parent)
        {
            var parent = await parentRepository.GetByUserIdAsync(currentUserId, cancellationToken).ConfigureAwait(false);
            if (parent is null || parent.IsDeleted)
            {
                return Error.Forbidden(
                    code: "Grade.ParentNotFound",
                    description: "Профіль батьків не знайдено або деактивовано.");
            }

            var isLinked = await studentParentRepository.ExistsAsync(request.StudentId, parent.ParentId, cancellationToken).ConfigureAwait(false);
            if (!isLinked)
            {
                return Error.Forbidden(
                    code: "Grade.ParentAccessDenied",
                    description: "Ви не маєте доступу до оцінок цього студента.");
            }
        }

        var grades = await gradeRepository.GetByStudentIdAsync(request.StudentId, cancellationToken).ConfigureAwait(false);

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