using VetGest.Contracts.Chat;

namespace VetGest.Application.Chat;

public sealed record ChatSendResult(ChatMessageDto Message, string RecipientUserId);