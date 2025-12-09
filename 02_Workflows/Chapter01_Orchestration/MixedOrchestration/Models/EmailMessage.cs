namespace MixedOrchestration.Models;

public sealed class EmailMessage
{
    public string Sender { get; set; } = "sewc.system.cn@siemens.com";
    public string Recipient { get; set; } = "edisontalk@company.com";
    public string Subject { get; set; } = "[Warning] Jailbreak Detected!";
    public string Body { get; set; } = string.Empty;
}