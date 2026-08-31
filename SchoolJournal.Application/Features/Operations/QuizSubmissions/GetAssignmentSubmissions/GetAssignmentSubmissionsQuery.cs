using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.QuizSubmissions;

namespace SchoolJournal.Application.Features.Operations.QuizSubmissions.GetAssignmentSubmissions;

public sealed record GetAssignmentSubmissionsQuery(Guid AssignmentId) : IRequest<ErrorOr<List<QuizSubmissionResultDto>>>;