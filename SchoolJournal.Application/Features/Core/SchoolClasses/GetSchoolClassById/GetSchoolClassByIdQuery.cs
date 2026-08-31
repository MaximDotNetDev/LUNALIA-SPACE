using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Core.SchoolClasses;

namespace SchoolJournal.Application.Features.Core.SchoolClasses.GetSchoolClassById;

public sealed record GetSchoolClassByIdQuery(
    Guid ClassId
) : IRequest<ErrorOr<SchoolClassResponse>>;