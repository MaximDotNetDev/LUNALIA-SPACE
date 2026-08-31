using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Subjects;

namespace SchoolJournal.Application.Features.Core.Subject.GetSubjectById;

public sealed record GetSubjectByIdQuery(
    Guid SubjectId
) : IRequest<ErrorOr<SubjectResponse>>;