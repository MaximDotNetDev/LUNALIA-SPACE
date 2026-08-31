using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Core.SchoolClasses;

namespace SchoolJournal.Application.Features.Core.SchoolClasses.GetActiveSchoolClasses;

public sealed record GetActiveSchoolClassesQuery(
    PageRequest PageRequest,
    string? AcademicYear
) : IRequest<PagedResponse<SchoolClassItemResponse>>;