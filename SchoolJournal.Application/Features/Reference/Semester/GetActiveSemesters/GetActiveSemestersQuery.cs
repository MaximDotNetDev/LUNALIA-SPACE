using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference;
using SchoolJournal.Contracts.DTOs.Reference.Semesters;

namespace SchoolJournal.Application.Features.Reference.Semester.GetActiveSemesters;

public sealed record GetActiveSemestersQuery(PageRequest Pagination) : IRequest<PagedResponse<SemesterResponse>>;