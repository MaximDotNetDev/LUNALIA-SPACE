using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Operations.IRepositories;
using SchoolJournal.Domain.Enums.Identity;

namespace SchoolJournal.Application.Features.Operations.Grade.UpdateGrade;

public sealed class UpdateGradeCommandHandler(
    IGradeRepository gradeRepository,
    ILessonRepository lessonRepository,
    ICurrentUserService currentUserService,
    IAuditContext auditContext)
    : IRequestHandler<UpdateGradeCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateGradeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingGrade = await gradeRepository.GetByIdAsync(request.GradeId, cancellationToken).ConfigureAwait(false);
        if (existingGrade is null || existingGrade.IsDeleted)
        {
            return Error.NotFound(
                code: "Grade.NotFound",
                description: "Оцінку не знайдено або вона була видалена.");
        }

        var currentRole = currentUserService.GetUserRole();
        var currentUserId = currentUserService.GetUserId();

        if (currentRole == RoleType.Teacher)
        {
            var isOwner = await lessonRepository.VerifyLessonOwnershipAsync(existingGrade.LessonId, currentUserId, cancellationToken).ConfigureAwait(false);
            if (!isOwner)
            {
                return Error.Forbidden(
                    code: "Grade.Forbidden",
                    description: "Ви не маєте права редагувати оцінки за урок, який ви не ведете.");
            }
        }

        auditContext.TrackOldState(existingGrade);

        byte[] rowVersionBytes;
        try
        {
            rowVersionBytes = Convert.FromBase64String(request.RowVersionBase64);
        }
        catch (FormatException)
        {
            return Error.Validation(
                code: "Grade.InvalidRowVersion",
                description: "Некоректний формат версії рядка (RowVersion).");
        }

        var updatedGrade = new Domain.Entities.Operations.Grade
        {
            GradeId = existingGrade.GradeId,
            LessonId = existingGrade.LessonId,
            StudentId = existingGrade.StudentId,
            GradeValue = request.GradeValue,
            Comment = request.Comment,
            GradeTypeId = request.GradeTypeId,
            CreatedByUserId = existingGrade.CreatedByUserId,
            UpdatedByUserId = currentUserId,
            IsDeleted = existingGrade.IsDeleted,
            CreatedAt = existingGrade.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = rowVersionBytes
        };

        var result = await gradeRepository.UpdateAsync(updatedGrade, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            return Error.Conflict(
                code: "Grade.ConcurrencyConflict",
                description: "Оцінка була змінена або видалена іншим користувачем. Оновіть сторінку.");
        }

        auditContext.TrackNewState(result);

        return Result.Success;
    }
}