using AssistantManage.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using MimeTypes;
using OpenAI.Files;
using Swashbuckle.AspNetCore.Annotations;
using System.ClientModel;
using System.Net.Mime;

namespace AssistantManage.Controllers;

[ApiController]
[Route("file")]
public class FileController : ControllerBase
{
    private readonly AssistantDbContext _context;
    private readonly OpenAIFileClient _openAIFileClient;

    public FileController(AssistantDbContext context, OpenAIFileClient openAIFileClient)
    {
        _context = context;
        _openAIFileClient = openAIFileClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetAssistantFilesAsync(Guid assistantId,
        CancellationToken cancellationToken = default)
    {
        return Ok((await _context.Assistants.IgnoreAutoIncludes().Include(i => i.Store).ThenInclude(i => i.Files).IgnoreAutoIncludes()
                .AsSplitQuery().FirstOrDefaultAsync(f => f.Id == assistantId, cancellationToken: cancellationToken))!.Store!.Files);
    }

    [HttpPatch]
    public async Task<IActionResult> UpdateExternalFileSources(Guid fileId, string? source,
        CancellationToken cancellationToken = default)
    {
        (await _context.Files.FindAsync(fileId))!.ExternalSource = source;
        await _context.SaveChangesAsync(cancellationToken);
        return Ok();
    }

    [HttpGet("download")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(FileStreamResult), ContentTypes = [MediaTypeNames.Application.Octet])]
    public async Task<IActionResult> DownloadFile([FromQuery] string fileId, CancellationToken cancellationToken)
    {
        OpenAIFile file = null;
        BinaryData content = null;

        Parallel.Invoke(() => { file = _openAIFileClient.GetFile(fileId, cancellationToken); },
            () => { content = _openAIFileClient.DownloadFile(fileId, cancellationToken); });

        var mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(file.Filename)) == "application/octet-stream" ? GetFileTypeFromBytes(content.ToArray()) : MimeTypeMap.GetMimeType(Path.GetExtension(file.Filename));
        return File(content.ToStream(), mimeType, file.Filename);
    }

    private string GetFileTypeFromBytes(byte[] bytes)
    {
        if (IsPng(bytes))
        {
            return "image/png";
        }
        else
        {
            return "application/octet-stream";
        }
    }

    private bool IsPng(byte[] bytes)
    {
        return bytes.Length >= 8 && bytes[0] == 137 && bytes[1] == 80 && bytes[2] == 78 && bytes[3] == 71 && bytes[4] == 13 && bytes[5] == 10 && bytes[6] == 26 && bytes[7] == 10;
    }
}