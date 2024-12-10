using Assistant.Functions.Abstractions.Interfaces;
using Assistant.Service.Interfaces;
using Assistant.Service.Models;
using AssistantManage.Data.Enums;
using AssistantManage.Data.Models;
using AssistantManage.Repositories;
using MediatR;

namespace AssistantManage.Requests.Assistant;

public class CreateAssistant : IRequest<Guid?>
{
    public string Name { get; }
    public string? Description { get; }
    public string Instructions { get; }
    public IFormFileCollection? Files { get; }
    public AssistantTools[] Functions { get; }
    public float Temperature { get; }
    public bool FileSearch { get; }
    public bool CodeInterpreter { get; }

    public CreateAssistant(string name,
        string? description,
        string instructions,
        IFormFileCollection? files,
        AssistantTools[] functions,
        float temperature = 1f,
        bool fileSearch = false,
        bool codeInterpreter = false)
    {
        Name = name;
        Description = description;
        Instructions = instructions;
        Files = files;
        Functions = functions;
        Temperature = temperature;
        FileSearch = fileSearch;
        CodeInterpreter = codeInterpreter;
    }
}

public class CreateAssistantHandler : IRequestHandler<CreateAssistant, Guid?>
{
    private readonly IStoreRepository _repository;
    private readonly IAssistantService _assistantService;

    public CreateAssistantHandler(IStoreRepository repository, IAssistantService assistantService)
    {
        _repository = repository;
        _assistantService = assistantService;
    }
    
    /// <inheritdoc />
    public async Task<Guid?> Handle(CreateAssistant request, CancellationToken cancellationToken)
    {
        var createdAssistant = await _assistantService.AddAssistantAsync(new AssistantCreateData(request.Name, request.Description,
            request.Instructions, request.Files, request.Functions, request.Temperature, request.FileSearch,
            request.CodeInterpreter), cancellationToken); 
        
        VectorStoreEntity? vectoredStoreEntity = default;

        if (createdAssistant.Assistant.ToolResources != null && createdAssistant.Assistant.ToolResources.FileSearch != null)
        {
            if (createdAssistant.Store != null)
            {
                vectoredStoreEntity = await _repository.AddStoreAsync(new VectorStoreEntity()
                {
                    InModelId = createdAssistant.Store.Id,
                    Name = createdAssistant.Store.Name ?? $"store_{createdAssistant.Assistant.Id}",
                    Files = createdAssistant.AIFiles?.Select(s => new VectoredFileEntity()
                    {
                        InModelId = s.Id,
                        Filename = s.Filename,
                        Purpose = s.Purpose.ToString(),
                        SizeInBytes = s.SizeInBytes.Value,
                    }).ToList()
                }, cancellationToken);
            }
        }

        var tools = request.Functions.Any() ? request.Functions.Aggregate((old, tool) => old | tool) : AssistantTools.None;

        return (await _repository.AddAssistantAsync(new AssistantEntity()
        {
            Name = createdAssistant.Assistant.Name,
            Description = createdAssistant.Assistant.Description,
            Instructions = createdAssistant.Assistant.Instructions,
            Temperature = request.Temperature,
            HasCodeInterpreter = request.CodeInterpreter,
            HasFileSearch = request.FileSearch,
            Tools = tools,
            Status = Status.Enabled,
            InModelId = createdAssistant.Assistant.Id,
            StoreId = vectoredStoreEntity?.Id,
            Store = vectoredStoreEntity
        }, cancellationToken))?.Id;
    }
}