using ErrorOr;
using Mapster;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Students;
using SchoolJournal.Domain.Entities.Core.IRepositories;

namespace SchoolJournal.Application.Features.Core.Student.GetStudentHistory;

public sealed class GetStudentHistoryQueryHandler(IStudentRepository studentRepository)
    : IRequestHandler<GetStudentHistoryQuery, ErrorOr<IEnumerable<StudentHistoryResponse>>>
{
    public async Task<ErrorOr<IEnumerable<StudentHistoryResponse>>> Handle(GetStudentHistoryQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var history = await studentRepository.GetHistoryAsync(request.StudentId, cancellationToken).ConfigureAwait(false);

        var historyList = history.ToList();
        if (historyList.Count == 0)
        {
            return Error.NotFound("Student.HistoryNotFound", "Історію змін для цього учня не знайдено.");
        }

        return historyList.Adapt<List<StudentHistoryResponse>>();
    }
}