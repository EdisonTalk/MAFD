namespace LoopFlow.Models.ValueObjects;

internal record ReplyDraft(string TicketId, string Content, int Attempt);