using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.Subjects;

namespace SchoolJournal.Application.Features.Core.Subject.GetSubjects;

public sealed record GetSubjectsQuery(
    PageRequest PageRequest,
    string? SearchTerm = null
) : IRequest<ErrorOr<PagedResponse<SubjectResponse>>>;