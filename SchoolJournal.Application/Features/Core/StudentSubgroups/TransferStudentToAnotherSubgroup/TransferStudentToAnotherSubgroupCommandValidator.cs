using FluentValidation;

namespace SchoolJournal.Application.Features.Core.StudentSubgroups.TransferStudentToAnotherSubgroup;

public sealed class TransferStudentToAnotherSubgroupCommandValidator : AbstractValidator<TransferStudentToAnotherSubgroupCommand>
{
    public TransferStudentToAnotherSubgroupCommandValidator()
    {
        RuleFor(x => x.StudentSubgroupId)
            .NotEmpty().WithMessage("Ідентифікатор зв'язку обов'язковий.");

        RuleFor(x => x.NewSubgroupId)
            .NotEmpty().WithMessage("Ідентифікатор нової підгрупи обов'язковий.");
    }
}