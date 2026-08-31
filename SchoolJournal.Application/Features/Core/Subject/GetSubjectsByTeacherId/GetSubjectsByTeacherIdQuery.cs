using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.Subjects;

namespace SchoolJournal.Application.Features.Core.Subject.GetSubjectsByTeacherId;

public sealed record GetSubjectsByTeacherIdQuery(Guid TeacherId)
    : IRequest<ErrorOr<IReadOnlyCollection<SubjectResponse>>>;