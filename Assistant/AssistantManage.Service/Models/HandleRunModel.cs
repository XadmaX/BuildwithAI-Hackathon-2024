namespace Assistant.Service.Models;

internal class HandleRunModel(string messageId, string runId)
{
    public string MessageId { get; set; } = messageId;
    public string RunId { get; set; } = runId;

    public List<string> AttachmentFiles { get; set; } = [];

    public List<TextAnnotation> Annotations { get; set; } = [];

    public int InputTokens  { get; set; }
    public int OutputTokens { get; set; }

    public HandleRunModel() : this("", "")
    {
    }
}