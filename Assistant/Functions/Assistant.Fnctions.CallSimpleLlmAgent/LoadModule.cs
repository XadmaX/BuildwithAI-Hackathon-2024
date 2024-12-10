using Assistant.Agents.SimpleLlmAgent;
using CallSimpleLlmAgent.Options;
using Assistant.Functions.Abstractions.Attributes;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[assembly: AssistantFunction]
namespace CallSimpleLlmAgent;

public class LoadModule : Module
{
    /// <inheritdoc />
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);
        var services = new ServiceCollection();
        services.Configure<SimpleLlmAgentServiceOptions>(opts =>
        {
            opts.Url = "https://localhost:5001";
        });
        services.AddHttpClient<SimpleLlmAgentService>();
        services.AddSingleton(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(SimpleLlmAgentService));
            var options = sp.GetRequiredService<IOptions<SimpleLlmAgentServiceOptions>>().Value;
            return new SimpleLlmAgentService(options.Url, httpClient);
        });

        builder.Populate(services);
    }
}