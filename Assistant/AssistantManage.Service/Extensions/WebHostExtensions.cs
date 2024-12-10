using System.ClientModel;
using System.Runtime.Loader;
using System.Threading.DistributedLock;
using Assistant.Functions.Abstractions.Attributes;
using Assistant.Service.Interfaces;
using Assistant.Service.Options;
using Assistant.Service.Services;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Azure.AI.OpenAI;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Mono.Cecil;
using OpenAI.Assistants;
using OpenAI.Files;
using OpenAI.VectorStores;
using StackExchange.Redis;

namespace Assistant.Service.Extensions;

public static class WebHostExtensions
{
    public static IHostBuilder AddAssistantServices(
        this IHostBuilder builder,
        string distributedLockOptionsPath = "DistributedLock",
        string openAiOptionsPath = "Azure:OpenAI")
    {
        builder.ConfigureServices((context, services) =>
        {
            services.Configure<OpenAIOptions>(context.Configuration.GetSection(openAiOptionsPath));
            services.Configure<DistributedLockOptions>(context.Configuration.GetSection(distributedLockOptionsPath));

            services.AddScoped<IConversationService, ConversationService>();
            services.AddScoped<IAssistantService, AssistantService>();
            services.AddSingleton<IFunctionService, FunctionService>();

            services.AddScoped<AzureOpenAIClient>(provider =>
            {
                var openAiOptions = provider.GetRequiredService<IOptions<OpenAIOptions>>().Value;
                return new AzureOpenAIClient(new Uri(openAiOptions.Host), new ApiKeyCredential(openAiOptions.Key));
            });
            services.AddScoped<VectorStoreClient>(provider =>
                provider.GetRequiredService<AzureOpenAIClient>().GetVectorStoreClient());
            services.AddScoped<AssistantClient>(provider =>
                provider.GetRequiredService<AzureOpenAIClient>().GetAssistantClient());
            services.AddScoped<OpenAIFileClient>(provider =>
                provider.GetRequiredService<AzureOpenAIClient>().GetOpenAIFileClient());
            
            services.AddSingleton(sp =>
            {
                var azureOpenAiClient = sp.GetRequiredService<AzureOpenAIClient>();
                return azureOpenAiClient.GetAssistantClient();
            });
            services.AddSingleton(sp =>
            {
                var blobServiceClient = new BlobServiceClient(context.Configuration.GetConnectionString("AzureStorage"));
                return blobServiceClient;
            });
            services.AddSingleton<IDistributedLock>(sp =>
            {
                var blobServiceClient = sp.GetRequiredService<BlobServiceClient>();
                var blobContainerClient = blobServiceClient.GetBlobContainerClient("locks");
                var options = sp.GetRequiredService<IOptions<DistributedLockOptions>>().Value;
                return new BlobStorageDistributedLock(blobContainerClient, options.Ttl, options.Timeout);
            });
        });

        builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

        builder.ConfigureContainer<ContainerBuilder>(container =>
        {
            var dir = new FileInfo(typeof(ServiceCollectionExtensions).Assembly.Location).Directory!;
            Console.WriteLine($"Dir: {dir.FullName}");
            var dlls = dir.GetFiles("*.dll");
            var asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var dll in dlls.Where(w => IsSuitable(w.FullName)))
            {
                var defaultContext = AssemblyLoadContext.Default;
                var loaded =
                    asms.FirstOrDefault(f => f.FullName != null && f.FullName.Equals(dll.FullName, StringComparison.InvariantCulture));

                if (loaded == null)
                    loaded = defaultContext.LoadFromAssemblyPath(dll.FullName);

                container.RegisterAssemblyModules(loaded);
            }
        });

        return builder;
    }

    private static bool IsSuitable(string path)
    {
        try
        {
            var type = typeof(AssistantFunction);
            var assembly = AssemblyDefinition.ReadAssembly(path);
            return assembly.CustomAttributes.Any(w =>
                w.AttributeType.Name == type.Name && w.AttributeType.Namespace == type.Namespace);
        }
        catch
        {
            return false;
        }
    }
}