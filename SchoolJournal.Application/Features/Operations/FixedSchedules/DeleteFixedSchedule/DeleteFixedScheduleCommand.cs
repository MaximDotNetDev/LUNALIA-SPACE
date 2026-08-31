using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Operations.FixedSchedules.DeleteFixedSchedule;

public sealed record DeleteFixedScheduleCommand(
    Guid ScheduleId,
    string RowVersionBase64
) : IRequest<ErrorOr<Success>>;