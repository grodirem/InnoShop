using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;
using UserService.Domain.Enums;

namespace UserService.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Status)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (UserStatus)Enum.Parse(typeof(UserStatus), v))
            .HasDefaultValue(UserStatus.Active);

        builder.Property(u => u.IsEmailConfirmed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.EmailConfirmationToken)
            .HasMaxLength(100);

        builder.Property(u => u.PasswordResetToken)
            .HasMaxLength(100);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt);

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.EmailConfirmationToken);
        builder.HasIndex(u => u.PasswordResetToken);

        builder.HasData(
           new User
           {
               Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
               Name = "Admin",
               Email = "admin@1.1",
               PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass1!"),
               Role = "Admin",
               Status = UserStatus.Active,
               IsEmailConfirmed = true,
               CreatedAt = DateTime.UtcNow
           },
           new User
           {
               Id = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f23456789012"),
               Name = "User",
               Email = "user@1.1",
               PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass1!"),
               Role = "User",
               Status = UserStatus.Active,
               IsEmailConfirmed = true,
               CreatedAt = DateTime.UtcNow
           }
        );
    }
}