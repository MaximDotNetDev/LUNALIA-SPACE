using ErrorOr;
using MediatR;
using SchoolJournal.Domain.Entities.Identity;
using SchoolJournal.Domain.Entities.Identity.IRepositories;
using SchoolJournal.Application.Features.Identity.Common.Interfaces;

namespace SchoolJournal.Application.Features.Identity.Register;

public sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository, // Додано IRoleRepository
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

        // 2. Отримуємо всі ролі з бази даних
        var allRoles = await roleRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        // 3. Знаходимо потрібну роль за її назвою (Enum конвертується в string)
        var roleName = request.Role.ToString();

        var selectedRole = allRoles.FirstOrDefault(r => r.RoleName.ToString() == request.Role.ToString());

        if (selectedRole is null)
        {
            // Якщо роль не знайдена, повертаємо помилку валідації
            return Error.Validation("Role.NotFound", $"Роль '{roleName}' не знайдена в базі даних.");
        }

        var userId = Guid.NewGuid();

        // 4. Створюємо нового користувача, ПЕРЕДАЮЧИ RoleId
        var newUser = new User
        {
            UserId = userId,
            Login = request.Login,
            Email = null,
            PasswordHash = passwordHasher.Hash(request.Password),
            RoleId = selectedRole.RoleId, // Ось тут ми призначаємо валідний GUID!
            Role = (SchoolJournal.Domain.Enums.Identity.RoleType)request.Role,
            IsActive = true,
            IsDeleted = false,
            FailedLoginAttempts = 0,
            LockoutEndUtc = null,
            LastLoginUtc = null,
            CreatedAt = DateTimeOffset.UtcNow,
            RowVersion = []
        };

        // 5. Зберігаємо в базу
        await userRepository.AddAsync(newUser, cancellationToken).ConfigureAwait(false);

        return userId;
    }
}