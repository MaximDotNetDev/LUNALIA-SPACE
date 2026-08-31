using ErrorOr;
using MediatR;
using SchoolJournal.Domain.Entities.Identity;
using SchoolJournal.Domain.Entities.Identity.IRepositories;
using SchoolJournal.Application.Features.Identity.Common.Interfaces;

namespace SchoolJournal.Application.Features.Identity.Register;

public sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher)
    : IRequestHandler<RegisterCommand, ErrorOr<Guid>>
{
    public async Task<ErrorOr<Guid>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Перевіряємо, чи немає вже такого користувача
        var existingUser = await userRepository.GetByLoginAsync(request.Login, cancellationToken).ConfigureAwait(false);
        if (existingUser is not null)
        {
            return Error.Conflict("User.Duplicate", $"Користувач з логіном '{request.Login}' вже існує.");
        }

        var userId = Guid.NewGuid();

        // 2. Створюємо нового користувача
        var newUser = new User
        {
            UserId = userId,
            Login = request.Login,
            Email = null,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = (SchoolJournal.Domain.Enums.Identity.RoleType)request.Role,
            IsActive = true,
            IsDeleted = false,
            FailedLoginAttempts = 0,
            LockoutEndUtc = null,
            LastLoginUtc = null,
            CreatedAt = DateTimeOffset.UtcNow,
            RowVersion = []
        };

        // 3. Зберігаємо в базу
        await userRepository.AddAsync(newUser, cancellationToken).ConfigureAwait(false);

        return userId;
    }
}