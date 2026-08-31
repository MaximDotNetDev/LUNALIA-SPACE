using FluentValidation;

namespace SchoolJournal.Application.Features.Core.Student.SearchStudents;

public sealed class SearchStudentsQueryValidator : AbstractValidator<SearchStudentsQuery>
{
    public SearchStudentsQueryValidator()
    {
        RuleFor(x => x.Page.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Page.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.SearchTerm).MaximumLength(100).When(x => x.SearchTerm is not null);
    }
}