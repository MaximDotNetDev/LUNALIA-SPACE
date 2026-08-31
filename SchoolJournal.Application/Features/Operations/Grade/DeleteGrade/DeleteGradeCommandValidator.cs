using FluentValidation;

namespace SchoolJournal.Application.Features.Operations.Grade.DeleteGrade;

public sealed class DeleteGradeCommandValidator : AbstractValidator<DeleteGradeCommand>
{
    public DeleteGradeCommandValidator()
    {
        RuleFor(x => x.GradeId)
            .NotEmpty().WithMessage("Ідентифікатор оцінки є обов'язковим.");

        RuleFor(x => x.RowVersionBase64)
            .NotEmpty().WithMessage("Версія рядка (RowVersion) є обов'язковою для запобігання конфліктів паралелізму.");
    }
}