using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Students;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Student.GetStudentsByClassId;

public sealed class GetStudentsByClassIdQueryHandler(IStudentRepository studentRepository)
    : IRequestHandler<GetStudentsByClassIdQuery, ErrorOr<IEnumerable<StudentLookupResponse>>>
{
    public async Task<ErrorOr<IEnumerable<StudentLookupResponse>>> Handle(GetStudentsByClassIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var students = await studentRepository.GetActiveByClassIdAsync(request.ClassId, cancellationToken).ConfigureAwait(false);

        return students.Adapt<List<StudentLookupResponse>>();
    }
}