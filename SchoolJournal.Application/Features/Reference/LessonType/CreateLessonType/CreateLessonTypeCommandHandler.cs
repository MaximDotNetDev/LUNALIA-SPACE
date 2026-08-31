using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.LessonType.CreateLessonType;

public sealed class CreateLessonTypeCommandHandler(
    ILessonTypeRepository lessonTypeRepository,
    IAuditContext auditContext)
    : IRequestHandler<CreateLessonTypeCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateLessonTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await lessonTypeRepository.ExistsByNameAsync(request.TypeName, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "LessonType.NameConflict",
                description: $"Тип уроку з назвою '{request.TypeName}' вже існує.");
        }

        var lessonType = new Domain.Entities.Reference.LessonType
        {
            TypeName = request.TypeName
        };

        var lessonTypeId = await lessonTypeRepository.AddAsync(lessonType, cancellationToken).ConfigureAwait(false);

        var newState = await lessonTypeRepository.GetByIdAsync(lessonTypeId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return lessonTypeId;
    }
}