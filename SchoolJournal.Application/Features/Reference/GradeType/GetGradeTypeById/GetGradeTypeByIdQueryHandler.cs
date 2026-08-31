using ErrorOr;
using MediatR;
using SchoolJournal.Domain.Entities.Reference.IRepositories;
using SchoolJournal.Contracts.DTOs.Reference.GradeTypes;
using Mapster;

namespace SchoolJournal.Application.Features.Reference.GradeType.GetGradeTypeById;

public sealed class GetGradeTypeByIdQueryHandler(IGradeTypeRepository gradeTypeRepository)
    : IRequestHandler<GetGradeTypeByIdQuery, ErrorOr<GradeTypeResponse>>
{
    public async Task<ErrorOr<GradeTypeResponse>> Handle(GetGradeTypeByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var gradeType = await gradeTypeRepository.GetByIdAsync(request.GradeTypeId, cancellationToken).ConfigureAwait(false);

        return gradeType is null
            ? Error.NotFound("GradeType.NotFound", "Тип оцінки не знайдено.")
            : gradeType.Adapt<GradeTypeResponse>();
    }
}