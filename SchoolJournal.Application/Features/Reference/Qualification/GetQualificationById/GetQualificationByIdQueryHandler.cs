using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Reference.Qualifications;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.Qualification.GetQualificationById;

public sealed class GetQualificationByIdQueryHandler(
    IQualificationRepository qualificationRepository)
    : IRequestHandler<GetQualificationByIdQuery, ErrorOr<QualificationResponse>>
{
    public async Task<ErrorOr<QualificationResponse>> Handle(GetQualificationByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var qualification = await qualificationRepository.GetByIdAsync(request.QualificationId, cancellationToken).ConfigureAwait(false);

        if (qualification is null)
        {
            return Error.NotFound(
                code: "Qualification.NotFound",
                description: "Кваліфікацію не знайдено.");
        }

        return new QualificationResponse(
            qualification.QualificationId,
            qualification.QualificationName,
            Convert.ToBase64String(qualification.RowVersion.ToArray()));
    }
}