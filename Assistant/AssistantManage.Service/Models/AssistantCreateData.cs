using Assistant.Functions.Abstractions.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Assistant.Service.Models;

public record AssistantCreateData(
    string Name,
    string? Description,
    string Instructions,
    IFormFileCollection? Files,
    AssistantTools[] Functions,
    float Temperature = 1f,
    bool FileSearch = false,
    bool CodeInterpreter = false);