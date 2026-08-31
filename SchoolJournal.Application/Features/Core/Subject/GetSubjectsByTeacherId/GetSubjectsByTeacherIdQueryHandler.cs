using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Subjects;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Subject.GetSubjectsByTeacherId;

public sealed class GetSubjectsByTeacherIdQueryHandler(ISubjectRepository subjectRepository)
    : IRequestHandler<GetSubjectsByTeacherIdQuery, ErrorOr<IReadOnlyCollection<SubjectResponse>>>
{
    public async Task<ErrorOr<IReadOnlyCollection<SubjectResponse>>> Handle(GetSubjectsByTeacherIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var subjects = await subjectRepository.GetByTeacherIdAsync(request.TeacherId, cancellationToken).ConfigureAwait(false);

        return subjects.Select(s => new SubjectResponse(
            s.SubjectId,
            s.SubjectName
        )).ToList();
    }
}