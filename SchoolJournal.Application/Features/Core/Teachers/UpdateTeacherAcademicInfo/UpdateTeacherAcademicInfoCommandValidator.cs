using FluentValidation;

namespace SchoolJournal.Application.Features.Core.Teachers.UpdateTeacherAcademicInfo;

public sealed class UpdateTeacherAcademicInfoCommandValidator : AbstractValidator<UpdateTeacherAcademicInfoCommand>
{
    public UpdateTeacherAcademicInfoCommandValidator()
    {
        RuleFor(x => x.TeacherId).NotEmpty();

        RuleFor(x => x.PositionId).NotEmpty().WithMessage("Посада є обов'язковою.");

        RuleFor(x => x.QualificationId).NotEmpty().WithMessage("Кваліфікація є обов'язковою.");

        RuleFor(x => x.Workload)
            .InclusiveBetween(0, 2.0m).WithMessage("Навантаження має бути в межах від 0 до 2.0.")
            .When(x => x.Workload.HasValue);

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія даних є обов'язковою.");
    }
}