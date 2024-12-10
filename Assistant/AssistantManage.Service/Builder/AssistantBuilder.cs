using Assistant.Functions.Abstractions.Interfaces;
using Assistant.Service.Services;
using OpenAI.Assistants;

namespace Assistant.Service.Builder;

internal class AssistantBuilder
{
    private readonly AssistantClient _assistantClient;

    private AssistantCreationOptions _options;

    private readonly string _model = "gpt-4o";

    public AssistantBuilder(AssistantClient assistantClient)
    {
        _assistantClient = assistantClient;
        _options = new AssistantCreationOptions()
        {
            ToolResources = new ToolResources(),
            ResponseFormat = AssistantResponseFormat.Auto
        };
    }

    public AssistantBuilder WithName(string name)
    {
        _options.Name = name;
        return this;
    }

    public AssistantBuilder WithDescription(string? description)
    {
        _options.Description = description ?? string.Empty;
        return this;
    }

    public AssistantBuilder WithTemperature(float temperature)
    {
        _options.Temperature = temperature;
        return this;
    }

    public AssistantBuilder WithFileSearchResource(FileSearchToolResources resources)
    {
        _options.ToolResources.FileSearch = resources;
        return this;
    }

    public AssistantBuilder WithCodeInterpreterResource(CodeInterpreterToolResources resources)
    {
        _options.ToolResources.CodeInterpreter = resources;
        return this;
    }

    public AssistantBuilder WithCodeInterpreter()
    {
        _options.Tools.Add(new CodeInterpreterToolDefinition());
        return this;
    }

    public AssistantBuilder WithFileSearch()
    {
        _options.Tools.Add(new FileSearchToolDefinition());
        return this;
    }

    public AssistantBuilder WithInstructions(string instructions)
    {
        _options.Instructions = instructions;
        return this;
    }

    public AssistantBuilder WithMetadata(Dictionary<string, string> metadata)
    {
        _options.Metadata = metadata;
        return this;
    }

    public AssistantBuilder AddTool(AssistantTools tool)
    {
        _options.Tools.Add(FunctionService.GetFunctionsForRegistration()[tool.ToString()]);

        return this;
    }

    public OpenAI.Assistants.Assistant Create()
        => _assistantClient.CreateAssistant(_model, _options);

    public async Task<OpenAI.Assistants.Assistant> CreateAsync(CancellationToken cancellationToken = default) =>
        await _assistantClient.CreateAssistantAsync(_model, _options, cancellationToken);
}