using MediatR;
using SchoolJournal.Domain.Entities.Reference.IRepositories;
using SchoolJournal.Contracts.DTOs.Reference.GradeTypes;
using Mapster;

namespace SchoolJournal.Application.Features.Reference.GradeType.GetActiveGradeTypes;

public sealed class GetActiveGradeTypesQueryHandler(IGradeTypeRepository gradeTypeRepository)
    : IRequestHandler<GetActiveGradeTypesQuery, IEnumerable<GradeTypeResponse>>
{
    public async Task<IEnumerable<GradeTypeResponse>> Handle(GetActiveGradeTypesQuery request, CancellationToken cancellationToken)
    {
        var types = await gradeTypeRepository.GetActiveAsync(cancellationToken).ConfigureAwait(false);
        return types.Adapt<IEnumerable<GradeTypeResponse>>();
    }
}