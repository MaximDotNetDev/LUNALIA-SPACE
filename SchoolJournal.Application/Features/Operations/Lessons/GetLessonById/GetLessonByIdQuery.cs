using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.Lessons;

namespace SchoolJournal.Application.Features.Operations.Lessons.GetLessonById;

public sealed record GetLessonByIdQuery(Guid LessonId) : IRequest<ErrorOr<LessonResponse>>;