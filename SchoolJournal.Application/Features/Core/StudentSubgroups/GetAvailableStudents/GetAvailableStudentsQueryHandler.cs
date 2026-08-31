using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.StudentSubgroups;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.StudentSubgroups.GetAvailableStudents;

public sealed class GetAvailableStudentsQueryHandler(
    IStudentSubgroupRepository repository)
    : IRequestHandler<GetAvailableStudentsQuery, ErrorOr<IEnumerable<AvailableStudentModel>>>
{
    public async Task<ErrorOr<IEnumerable<AvailableStudentModel>>> Handle(GetAvailableStudentsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var items = await repository.GetAvailableStudentsForSubgroupIdAsync(request.SubgroupId, cancellationToken).ConfigureAwait(false);

        return items.Adapt<IEnumerable<AvailableStudentModel>>().ToList();
    }
}