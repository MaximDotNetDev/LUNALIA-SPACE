using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Teachers;

namespace SchoolJournal.Application.Features.Core.Teachers.GetTeacherWorkloadSummary;

public sealed record GetTeacherWorkloadSummaryQuery(bool OnlyActive = true)
    : IRequest<ErrorOr<IEnumerable<TeacherWorkloadResponse>>>;