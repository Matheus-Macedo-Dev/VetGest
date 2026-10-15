using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VetGest.Domain.Entities;

namespace VetGest.Infrastructure.Persistence.EntityConfigurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");
        builder.HasKey(message => message.Id);
        builder.Property(message => message.Id).ValueGeneratedNever();
        builder.Property(message => message.SenderUserId).IsRequired().HasMaxLength(128);
        builder.Property(message => message.Text).IsRequired().HasMaxLength(ChatMessage.MaxTextLength);
        builder.HasIndex(message => new { message.ConnectionId, message.SentAtUtc });
        builder.HasIndex(message => message.SentAtUtc);

        builder.HasOne<VetConnection>()
            .WithMany()
            .HasForeignKey(message => message.ConnectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}