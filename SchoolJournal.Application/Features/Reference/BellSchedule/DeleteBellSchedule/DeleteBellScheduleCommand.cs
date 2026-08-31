using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Reference.BellSchedule.DeleteBellSchedule;

public sealed record DeleteBellScheduleCommand(Guid ScheduleId) : IRequest<ErrorOr<Success>>;