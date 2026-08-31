using FluentValidation;

namespace SchoolJournal.Application.Features.Reference.Classroom.DeleteClassroom;

public sealed class DeleteClassroomCommandValidator : AbstractValidator<DeleteClassroomCommand>
{
    public DeleteClassroomCommandValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("ID аудиторії є обов'язковим.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія рядка (RowVersion) є обов'язковою.")
            .Must(BeAValidBase64).WithMessage("Неправильний формат версії рядка (RowVersion).");
    }

    private static bool BeAValidBase64(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64)) return false;
        Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
        return Convert.TryFromBase64String(base64, buffer, out _);
    }
}