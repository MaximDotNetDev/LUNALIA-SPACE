using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;

namespace SchoolJournal.Application.Features.Operations.Quizzes.GetQuizzesBySubject;

public sealed record GetQuizzesBySubjectQuery(
    Guid SubjectId,
    PageRequest PageRequest
) : IRequest<ErrorOr<PagedResponse<QuizResponse>>>;