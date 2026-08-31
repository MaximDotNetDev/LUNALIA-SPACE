using FluentValidation;

namespace SchoolJournal.Application.Features.Reference.Classroom.UpdateClassroom;

public sealed class UpdateClassroomCommandValidator : AbstractValidator<UpdateClassroomCommand>
{
    public UpdateClassroomCommandValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("ID аудиторії є обов'язковим.");

        RuleFor(x => x.RoomNumber)
            .NotEmpty().WithMessage("Номер аудиторії не може бути порожнім.")
            .MaximumLength(20).WithMessage("Номер аудиторії не може перевищувати 20 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Номер аудиторії не повинен містити пробіли на початку або в кінці.");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Назва аудиторії не може перевищувати 100 символів.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Місткість повинна бути більшою за 0.")
            .LessThanOrEqualTo(200).WithMessage("Місткість не може перевищувати 200 осіб.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія рядка (RowVersion) є обов'язковою.")
            .Must(BeAValidBase64).WithMessage("Неправильний формат версії рядка (RowVersion).");
    }

    private static bool HaveNoLeadingOrTrailingSpaces(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        return value.Trim().Length > 0 && value == value.Trim();
    }

    private static bool BeAValidBase64(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64)) return false;
        Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
        return Convert.TryFromBase64String(base64, buffer, out _);
    }
}