using OpenAI.Assistants;

namespace Assistant.Service.Models;

public class TextAnnotation
{
    public int? StartIndex { get; set; }
    public int? EndIndex { get; set; }
    
    public string TextToReplace { get; set; }
    
    public string InputFileId   { get; set; }
    public string OutputFileId { get; set; }

    public static TextAnnotation FromAnnotationContent(AnnotationContent update)
        => new ()
        {
            StartIndex = update.StartIndex,
            EndIndex = update.EndIndex,
            TextToReplace = update.TextToReplace,
            InputFileId = update.InputFileId,
            OutputFileId = update.OutputFileId
        };
    
    public static TextAnnotation FromTextAnnotation(TextAnnotationUpdate update)
        => new ()
        {
            StartIndex = update.StartIndex,
            EndIndex = update.EndIndex,
            TextToReplace = update.TextToReplace,
            InputFileId = update.InputFileId,
            OutputFileId = update.OutputFileId
        };
}