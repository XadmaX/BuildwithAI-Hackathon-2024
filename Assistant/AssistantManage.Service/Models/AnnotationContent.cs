using Microsoft.Extensions.AI;
using OpenAI.Assistants;

namespace Assistant.Service.Models;

public class AnnotationContent : AIContent
{
    public int? StartIndex { get; set; }
    public int? EndIndex { get; set; }

    public string TextToReplace { get; set; }

    public string InputFileId { get; set; }
    public string OutputFileId { get; set; }

    public AnnotationContent(string textToReplace, string inputFileId, string outputFileId, int? startIndex,
        int? endIndex)
    {
        TextToReplace = textToReplace;
        InputFileId = inputFileId;
        OutputFileId = outputFileId;
        StartIndex = startIndex;
        EndIndex = endIndex;
    }
    
    public static AnnotationContent FromAnnotationUpdate(TextAnnotationUpdate update)
     => new (update.TextToReplace, update.InputFileId, update.OutputFileId, update.StartIndex, update.EndIndex);
}