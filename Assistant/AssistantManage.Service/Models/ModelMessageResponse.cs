namespace Assistant.Service.Models;

public class ModelMessageResponse
{
    public string Text { get; set; }
    public int TokenCount { get; set; }
    public string MessageId { get; set; }
}