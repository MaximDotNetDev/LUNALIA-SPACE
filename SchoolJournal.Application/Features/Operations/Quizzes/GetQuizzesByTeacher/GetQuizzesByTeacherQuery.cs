using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Operations.Quizzes;

namespace SchoolJournal.Application.Features.Operations.Quizzes.GetQuizzesByTeacher;

public sealed record GetQuizzesByTeacherQuery(
    Guid TeacherId,
    PageRequest PageRequest
) : IRequest<ErrorOr<PagedResponse<QuizResponse>>>;