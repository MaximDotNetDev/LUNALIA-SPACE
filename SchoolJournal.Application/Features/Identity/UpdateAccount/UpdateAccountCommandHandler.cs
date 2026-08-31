using ErrorOr;
using MediatR;
using SchoolJournal.Application.Features.Identity.Common.Interfaces;
using SchoolJournal.Domain.Entities.Identity.IRepositories;

namespace SchoolJournal.Application.Features.Identity.UpdateAccount;

public sealed class UpdateAccountCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher)
    : IRequestHandler<UpdateAccountCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Шукаємо користувача
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return Error.NotFound("User.NotFound", "Користувача не знайдено.");
        }

        // 2. Якщо логін змінюється, перевіряємо, чи не зайнятий він іншим юзером
        if (!string.Equals(user.Login, request.Login, StringComparison.OrdinalIgnoreCase))
        {
            var existingUser = await userRepository.GetByLoginAsync(request.Login, cancellationToken).ConfigureAwait(false);
            if (existingUser is not null)
            {
                return Error.Conflict("User.DuplicateLogin", $"Логін '{request.Login}' вже використовується.");
            }
        }

        // 3. Оновлюємо дані (User - це record, тому створюємо копію через 'with')
        var updatedUser = user with
        {
            Login = request.Login
        };

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            updatedUser = updatedUser with { PasswordHash = passwordHasher.Hash(request.NewPassword) };
        }

        // 4. Зберігаємо зміни
        await userRepository.UpdateCredentialsAsync(updatedUser, cancellationToken).ConfigureAwait(false);

        return Result.Success;
    }
}