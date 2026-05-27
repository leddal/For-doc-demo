using Microsoft.EntityFrameworkCore;
using SmartPark.Identity.Application;
using SmartPark.Identity.Domain;

namespace SmartPark.Identity.Infrastructure;

public sealed class IdentityDatabaseInitializer(
    IdentityDbContext dbContext,
    IPasswordService passwordService)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var roles = RoleCodes.All
            .Select(code => new Role
            {
                Code = code,
                Name = code switch
                {
                    RoleCodes.Admin => "系统管理员",
                    RoleCodes.Dispatcher => "调度员",
                    RoleCodes.Operator => "处理员",
                    RoleCodes.Reviewer => "验收员",
                    _ => code
                }
            })
            .ToDictionary(item => item.Code, StringComparer.OrdinalIgnoreCase);

        dbContext.Roles.AddRange(roles.Values);

        var users = new[]
        {
            new User { UserName = "admin", DisplayName = "系统管理员", Email = "admin@smartpark.local" },
            new User { UserName = "dispatcher", DisplayName = "工单调度员", Email = "dispatcher@smartpark.local" },
            new User { UserName = "operator", DisplayName = "现场处理员", Email = "operator@smartpark.local" },
            new User { UserName = "reviewer", DisplayName = "验收员", Email = "reviewer@smartpark.local" }
        };

        foreach (var user in users)
        {
            user.PasswordHash = passwordService.HashPassword(user, "SmartPark@123");
        }

        dbContext.Users.AddRange(users);
        dbContext.UserRoles.AddRange(
            new UserRole { User = users[0], Role = roles[RoleCodes.Admin] },
            new UserRole { User = users[1], Role = roles[RoleCodes.Dispatcher] },
            new UserRole { User = users[2], Role = roles[RoleCodes.Operator] },
            new UserRole { User = users[3], Role = roles[RoleCodes.Reviewer] });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
