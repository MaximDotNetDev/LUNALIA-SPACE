using FluentValidation;

namespace SchoolJournal.Application.Features.Reference.Classroom.CreateClassroom;

public sealed class CreateClassroomCommandValidator : AbstractValidator<CreateClassroomCommand>
{
    public CreateClassroomCommandValidator()
    {
        RuleFor(x => x.RoomNumber)
            .NotEmpty().WithMessage("Номер аудиторії не може бути порожнім.")
            .MaximumLength(20).WithMessage("Номер аудиторії не може перевищувати 20 символів.")
            .Must(HaveNoLeadingOrTrailingSpaces).WithMessage("Номер аудиторії не повинен містити пробіли на початку або в кінці.");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Назва аудиторії не може перевищувати 100 символів.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Місткість повинна бути більшою за 0.")
            .LessThanOrEqualTo(200).WithMessage("Місткість не може перевищувати 200 осіб.");
    }

    private static bool HaveNoLeadingOrTrailingSpaces(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return value.Trim().Length > 0 && value == value.Trim();
    }
}