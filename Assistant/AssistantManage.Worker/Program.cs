using Assistant.Service.Extensions;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var host = new HostBuilder()
    .AddAssistantServices()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((builder, services) => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddSingleton(new ServiceBusClient(builder.Configuration.GetConnectionString("ServiceBus")));
    })
    .Build();

host.Run();
