using ErrorOr;
using MediatR;

namespace SchoolJournal.Application.Features.Core.Student.UpdateMedicalNotes;

public sealed record UpdateMedicalNotesCommand(
    Guid StudentId,
    string? MedicalNotes,
    string RowVersionBase64) : IRequest<ErrorOr<Success>>;