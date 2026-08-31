using ErrorOr;
using MediatR;
using SchoolJournal.Application.Common.Interfaces;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.GradeType.CreateGradeType;

public sealed class CreateGradeTypeCommandHandler(
    IGradeTypeRepository gradeTypeRepository,
    IAuditContext auditContext)
    : IRequestHandler<CreateGradeTypeCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(CreateGradeTypeCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await gradeTypeRepository.ExistsByNameAsync(request.TypeName, cancellationToken).ConfigureAwait(false))
        {
            return Error.Conflict(
                code: "GradeType.NameConflict",
                description: $"Тип оцінки з назвою '{request.TypeName}' вже існує.");
        }

        var gradeType = new Domain.Entities.Reference.GradeType
        {
            TypeName = request.TypeName
        };

        var gradeTypeId = await gradeTypeRepository.AddAsync(gradeType, cancellationToken).ConfigureAwait(false);

        var newState = await gradeTypeRepository.GetByIdAsync(gradeTypeId, cancellationToken).ConfigureAwait(false);
        if (newState is not null)
        {
            auditContext.TrackNewState(newState);
        }

        return gradeTypeId;
    }
}