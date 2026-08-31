using ErrorOr;
using MediatR;
using SchoolJournal.Contracts.DTOs.Operations.TeachingAssignments;

namespace SchoolJournal.Application.Features.Operations.TeachingAssignments.GetTeachingAssignmentById;

public sealed class GetTeachingAssignmentByIdQueryHandler(
    ITeachingAssignmentQueries teachingAssignmentQueries)
    : IRequestHandler<GetTeachingAssignmentByIdQuery, ErrorOr<TeachingAssignmentResponse>>
{
    public async Task<ErrorOr<TeachingAssignmentResponse>> Handle(GetTeachingAssignmentByIdQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var assignment = await teachingAssignmentQueries.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (assignment is null)
        {
            return Error.NotFound(
                code: "TeachingAssignment.NotFound",
                description: "Призначення не знайдено або воно було видалене.");
        }

        return assignment;
    }
}