using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using SimpleLlmAgents.Options;
using SimpleLlmAgents.Services.SmpleLlmAgent;
using SimpleLlmAgents.Services.Template;
using Microsoft.Extensions.Azure;
using Azure.Identity;
using Microsoft.OpenApi.Models;
using static Microsoft.IO.RecyclableMemoryStreamManager;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

# region Options

builder.Services.Configure<AzureStorageOptions>(builder.Configuration.GetSection("AzureStorage"));
builder.Services.Configure<AzureOpenAiOptions>(builder.Configuration.GetSection("AzureOpenAi"));
#endregion

#region Clients
builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<AzureStorageOptions>>().Value;
    return new BlobServiceClient(options.ConnectionString);
});
builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<AzureStorageOptions>>().Value;
    var blobServiceClient = sp.GetRequiredService<BlobServiceClient>();
    var containerClient = blobServiceClient.GetBlobContainerClient(options.ContainerName);
    containerClient.CreateIfNotExists();
    return containerClient;
});
builder.Services.AddKernel();
builder.Services.AddSingleton<TemplateService>();
builder.Services.AddSingleton<SimpleLlmAgentService>();

var azureOpenAiOptions = builder.Configuration.GetSection("AzureOpenAi").Get<AzureOpenAiOptions>();
foreach (var deployment in azureOpenAiOptions.Deployments)
{
    builder.Services.AddAzureOpenAIChatCompletion(deployment, azureOpenAiOptions.Endpoint, azureOpenAiOptions.ApiKey, modelId: deployment);
}
#endregion

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Simple LLM Agent API",
        Version = "v1",
        Description = "API для управління та виклику агентів LLM"
    });
    options.EnableAnnotations();
});
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["AzureStorage:ConnectionString1:blobServiceUri"]!).WithName("AzureStorage:ConnectionString1");
    clientBuilder.AddQueueServiceClient(builder.Configuration["AzureStorage:ConnectionString1:queueServiceUri"]!).WithName("AzureStorage:ConnectionString1");
    clientBuilder.AddTableServiceClient(builder.Configuration["AzureStorage:ConnectionString1:tableServiceUri"]!).WithName("AzureStorage:ConnectionString1");
});
builder.Services.Configure<Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration>(config =>
{
config.SetAzureTokenCredential(new DefaultAzureCredential());
});

builder.Services.AddApplicationInsightsTelemetry(new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions
{
    ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
});

var app = builder.Build();

app.MapDefaultEndpoints();


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Simple LLM Agent API V1");
});


app.UseAuthorization();

app.MapControllers();

app.Run();
