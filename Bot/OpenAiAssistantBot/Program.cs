using Azure.Identity;
using Azure.Storage.Blobs;
using Ca.Ai.Assistant;
using ChatGPTBot.Bots;
using ChatGPTBot.Handlers;
using ChatGPTBot.Options.Azure;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Converters;
using OpenAiAssistantBot.Options;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
        options.SerializerSettings.Converters.Add(new StringEnumConverter());
    });

builder.Services.AddOptions<AssistantServiceOptions>().BindConfiguration("AssistantService");
builder.Services.AddOptions<BlobStorageOptions>().BindConfiguration("Azure:Storage");

// Create the Bot Framework Authentication to be used with the Bot Adapter.
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
// Create the Bot Adapter with error handling enabled.
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
builder.Services.AddTransient<IBot, AssistentBot>();

builder.Services.AddScoped(provider =>
{
    var blobOptions = provider.GetRequiredService<IOptions<BlobStorageOptions>>();

    var blobClient = new BlobServiceClient(blobOptions.Value.ConnectionString);
    var containerClient = blobClient.GetBlobContainerClient(blobOptions.Value.ContainerName);
    containerClient.CreateIfNotExists();

    return containerClient;
});
builder.Services.AddScoped<BlobsStorage>(provider =>
{
    var blobOptions = provider.GetRequiredService<IOptions<BlobStorageOptions>>();
    return new(blobOptions.Value.ConnectionString, blobOptions.Value.ContainerName);
});
builder.Services.AddScoped<ConversationState>(provider => new(provider.CreateScope().ServiceProvider.GetRequiredService<BlobsStorage>()));

builder.Services.Configure<TelemetryConfiguration>(config =>
{
config.SetAzureTokenCredential(new DefaultAzureCredential());
});

builder.Services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
{
    ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
});

builder.Services.AddHttpClient<AssistantService>();
builder.Services.AddSingleton<IAssistantService>((provider) =>
{
    var options = provider.GetRequiredService<IOptions<AssistantServiceOptions>>().Value;
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient(nameof(AssistantService));
    return new AssistantService(options.Url, httpClient);
});

builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseWebSockets();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();