using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Teachers;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Teachers.GetTeacherWorkloadSummary;

public sealed class GetTeacherWorkloadSummaryQueryHandler(ITeacherRepository teacherRepository)
    : IRequestHandler<GetTeacherWorkloadSummaryQuery, ErrorOr<IEnumerable<TeacherWorkloadResponse>>>
{
    public async Task<ErrorOr<IEnumerable<TeacherWorkloadResponse>>> Handle(GetTeacherWorkloadSummaryQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var results = await teacherRepository.GetWorkloadSummaryAsync(request.OnlyActive, cancellationToken).ConfigureAwait(false);

        var response = results.Select(x => new TeacherWorkloadResponse(
            x.TeacherId,
            $"{x.LastName} {x.FirstName} {x.MiddleName}".Trim(),
            x.PositionName,
            x.Workload,
            x.IsActive
        ));

        return Array.AsReadOnly(response.ToArray());
    }
}