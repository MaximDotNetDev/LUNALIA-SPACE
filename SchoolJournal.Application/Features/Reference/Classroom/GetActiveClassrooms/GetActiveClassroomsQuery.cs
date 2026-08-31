using MediatR;
using SchoolJournal.Contracts.Common;
using SchoolJournal.Contracts.DTOs.Reference.Classrooms;

namespace SchoolJournal.Application.Features.Reference.Classroom.GetActiveClassrooms;

public sealed record GetActiveClassroomsQuery(
    PageRequest PageRequest,
    string? SearchTerm
) : IRequest<PagedResponse<ClassroomResponse>>;