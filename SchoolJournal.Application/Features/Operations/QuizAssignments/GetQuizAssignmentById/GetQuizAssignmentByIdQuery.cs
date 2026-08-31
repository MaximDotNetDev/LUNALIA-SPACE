using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.QuizAssignments;

namespace SchoolJournal.Application.Features.Operations.QuizAssignments.GetQuizAssignmentById;

public sealed record GetQuizAssignmentByIdQuery(Guid AssignmentId) : IRequest<ErrorOr<QuizAssignmentResponse>>;