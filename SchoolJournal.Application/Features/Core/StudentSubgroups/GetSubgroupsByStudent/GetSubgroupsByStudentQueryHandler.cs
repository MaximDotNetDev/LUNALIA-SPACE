using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.StudentSubgroups;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.StudentSubgroups.GetSubgroupsByStudent;

public sealed class GetSubgroupsByStudentQueryHandler(
    IStudentSubgroupRepository repository)
    : IRequestHandler<GetSubgroupsByStudentQuery, ErrorOr<StudentSubgroupsDetail>>
{
    public async Task<ErrorOr<StudentSubgroupsDetail>> Handle(GetSubgroupsByStudentQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var items = await repository.GetSubgroupsByStudentIdAsync(request.StudentId, cancellationToken).ConfigureAwait(false);

        var subgroupModels = items.Adapt<IEnumerable<SubgroupItemModel>>();

        return new StudentSubgroupsDetail(
            StudentId: request.StudentId,
            Subgroups: subgroupModels
        );
    }
}