using Microsoft.EntityFrameworkCore;
using SmartPark.DbSeeder.Options;
using SmartPark.Identity.Application;
using SmartPark.Identity.Domain;
using SmartPark.Identity.Infrastructure;

namespace SmartPark.DbSeeder.Seeders;

public sealed class IdentitySeeder(IdentityDbContext dbContext, IPasswordService passwordService)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        var roleSpecs = new[]
        {
            new RoleSeedSpec(RoleCodes.Admin, "系统管理员"),
            new RoleSeedSpec(RoleCodes.Dispatcher, "调度员"),
            new RoleSeedSpec(RoleCodes.Operator, "处理员"),
            new RoleSeedSpec(RoleCodes.Reviewer, "验收员")
        };

        var roleCodes = roleSpecs.Select(item => item.Code).ToArray();
        var roles = await dbContext.Roles
            .Where(item => roleCodes.Contains(item.Code))
            .ToDictionaryAsync(item => item.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var spec in roleSpecs)
        {
            if (!roles.TryGetValue(spec.Code, out var role))
            {
                role = new Role
                {
                    Code = spec.Code
                };

                dbContext.Roles.Add(role);
                roles[spec.Code] = role;
            }

            role.Name = spec.Name;
        }

        var userSpecs = new[]
        {
            new UserSeedSpec(DemoSeedData.AdminUserName, "系统管理员", "admin@smartpark.local", RoleCodes.Admin),
            new UserSeedSpec(DemoSeedData.DispatcherUserName, "工单调度员", "dispatcher@smartpark.local", RoleCodes.Dispatcher),
            new UserSeedSpec(DemoSeedData.OperatorUserName, "现场处理员", "operator@smartpark.local", RoleCodes.Operator),
            new UserSeedSpec(DemoSeedData.ReviewerUserName, "验收员", "reviewer@smartpark.local", RoleCodes.Reviewer)
        };

        var userNames = userSpecs.Select(item => item.UserName).ToArray();
        var users = await dbContext.Users
            .Include(item => item.UserRoles)
            .ThenInclude(item => item.Role)
            .Where(item => userNames.Contains(item.UserName))
            .ToDictionaryAsync(item => item.UserName, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var spec in userSpecs)
        {
            if (!users.TryGetValue(spec.UserName, out var user))
            {
                user = new User
                {
                    UserName = spec.UserName
                };

                dbContext.Users.Add(user);
                users[spec.UserName] = user;
            }

            user.DisplayName = spec.DisplayName;
            user.Email = spec.Email;
            user.IsActive = true;
            user.PasswordHash = passwordService.HashPassword(user, DemoSeedData.DefaultPassword);

            if (!user.UserRoles.Any(item => string.Equals(item.Role.Code, spec.RoleCode, StringComparison.OrdinalIgnoreCase)))
            {
                user.UserRoles.Add(new UserRole
                {
                    User = user,
                    Role = roles[spec.RoleCode]
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

file sealed record RoleSeedSpec(string Code, string Name);

file sealed record UserSeedSpec(string UserName, string DisplayName, string Email, string RoleCode);
