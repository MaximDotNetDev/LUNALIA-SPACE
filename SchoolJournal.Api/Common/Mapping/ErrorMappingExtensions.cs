using ErrorOr;

namespace SchoolJournal.Api.Common.Mapping;

internal static class ErrorMappingExtensions
{
    public static IResult ToProblem(this List<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (errors.Count == 0)
        {
            return Results.Problem();
        }

        if (errors.All(error => error.Type == ErrorType.Validation))
        {
            var validationErrors = errors
                .GroupBy(e => e.Code)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(e => e.Description).ToArray());

            return Results.ValidationProblem(validationErrors);
        }

        var firstError = errors[0];

        return firstError.Type switch
        {
            ErrorType.Conflict => Results.Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflict", detail: firstError.Description),
            ErrorType.Validation => Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Bad Request", detail: firstError.Description),
            ErrorType.NotFound => Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Not Found", detail: firstError.Description),
            ErrorType.Unauthorized => Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthorized", detail: firstError.Description),
            ErrorType.Forbidden => Results.Problem(statusCode: StatusCodes.Status403Forbidden, title: "Forbidden", detail: firstError.Description),
            _ => Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal Server Error", detail: firstError.Description)
        };
    }
}