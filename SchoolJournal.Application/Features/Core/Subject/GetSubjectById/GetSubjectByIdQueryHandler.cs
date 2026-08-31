using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Subjects;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Subject.GetSubjectById;

public sealed class GetSubjectByIdQueryHandler(
    ISubjectRepository subjectRepository)
    : IRequestHandler<GetSubjectByIdQuery, ErrorOr<SubjectResponse>>
{
    public async Task<ErrorOr<SubjectResponse>> Handle(GetSubjectByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var subject = await subjectRepository.GetByIdAsync(request.SubjectId, cancellationToken).ConfigureAwait(false);

        if (subject is null || subject.IsDeleted)
        {
            return Error.NotFound(
                code: "Subject.NotFound",
                description: $"Активний предмет з ідентифікатором '{request.SubjectId}' не знайдено.");
        }

        return subject.Adapt<SubjectResponse>();
    }
}