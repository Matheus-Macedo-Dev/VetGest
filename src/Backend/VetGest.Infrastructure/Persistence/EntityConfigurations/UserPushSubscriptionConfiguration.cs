using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetGest.Domain.Entities;

namespace VetGest.Infrastructure.Persistence.EntityConfigurations;

public class UserPushSubscriptionConfiguration : IEntityTypeConfiguration<UserPushSubscription>
{
    public void Configure(EntityTypeBuilder<UserPushSubscription> builder)
    {
        builder.ToTable("PushSubscriptions");
        builder.HasKey(subscription => subscription.Id);
        builder.Property(subscription => subscription.Id).ValueGeneratedNever();
        builder.Property(subscription => subscription.UserId).IsRequired().HasMaxLength(128);
        builder.Property(subscription => subscription.Endpoint).IsRequired().HasMaxLength(2048);
        builder.Property(subscription => subscription.P256dh).IsRequired().HasMaxLength(256);
        builder.Property(subscription => subscription.Auth).IsRequired().HasMaxLength(128);
        builder.HasIndex(subscription => subscription.Endpoint).IsUnique();
        builder.HasIndex(subscription => subscription.UserId);
    }
}