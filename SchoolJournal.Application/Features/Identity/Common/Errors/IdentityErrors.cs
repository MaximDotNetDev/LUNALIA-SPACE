using ErrorOr;

namespace SchoolJournal.Application.Features.Identity.Common.Errors;

public static class IdentityErrors
{
    public static Error InvalidCredentials => Error.Validation(
        code: "Identity.InvalidCredentials",
        description: "Невірний логін або пароль.");

    public static Error UserLockedOut => Error.Conflict(
        code: "Identity.LockedOut",
        description: "Обліковий запис тимчасово заблоковано через перевищення ліміту спроб.");

    public static Error UserInactive => Error.Forbidden(
            code: "Identity.Inactive",
            description: "Обліковий запис деактивовано або видалено.");

    public static Error InvalidRefreshToken => Error.Unauthorized(
        code: "Identity.InvalidRefreshToken",
        description: "Токен оновлення недійсний, прострочений або відкликаний.");
}