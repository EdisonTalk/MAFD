using MixedOrchestration.Models;
using System.Text.Json;

namespace MixedOrchestration.Services;

public interface IEmailService
{
    Task SendEmailAsync(string body, CancellationToken cancellationToken = default);
}

public sealed class EmailService : IEmailService
{
    public Task SendEmailAsync(string body, CancellationToken cancellationToken = default)
    {
        var emailDto = new EmailMessage { Body = body };
        Console.WriteLine("📧 已发送邮件通知：");
        Console.WriteLine(JsonSerializer.Serialize(emailDto));

        return Task.CompletedTask;
    }
}