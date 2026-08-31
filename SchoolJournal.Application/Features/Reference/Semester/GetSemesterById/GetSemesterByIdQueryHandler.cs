using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Reference.Semesters;
using SchoolJournal.Domain.Entities.Reference.IRepositories;

namespace SchoolJournal.Application.Features.Reference.Semester.GetSemesterById;

public sealed class GetSemesterByIdQueryHandler(ISemesterRepository semesterRepository)
    : IRequestHandler<GetSemesterByIdQuery, ErrorOr<SemesterResponse>>
{
    public async Task<ErrorOr<SemesterResponse>> Handle(GetSemesterByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var semester = await semesterRepository.GetByIdAsync(request.SemesterId, cancellationToken).ConfigureAwait(false);

        if (semester is null)
        {
            return Error.NotFound(code: "Semester.NotFound", description: "Семестр не знайдено.");
        }

        return new SemesterResponse(
            semester.SemesterId,
            semester.SemesterName,
            semester.StartDate,
            semester.EndDate,
            Convert.ToBase64String(semester.RowVersion.ToArray()));
    }
}