using System.Reflection;
using Assistant.Service.Extensions;
using AssistantManage.Data;
using AssistantManage.Options;
using AssistantManage.Options.Azure;
using AssistantManage.Repositories;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

#region Options

builder.Services.AddOptions<BlobStorageOptions>().BindConfiguration("Azure:Storage").ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<RefTextOptions>().BindConfiguration("RefText").ValidateDataAnnotations()
    .ValidateOnStart();

#endregion

#region Endpoints

builder.Services.AddControllers()
    .AddNewtonsoftJson(options => options.SerializerSettings.Converters.Add(new StringEnumConverter()));
JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
{
    Converters = [new StringEnumConverter()]
};

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.EnableAnnotations(); }).AddSwaggerGenNewtonsoftSupport();

#endregion

#region Services

builder.Services.AddScoped<IStoreRepository, EntityFrameworkRepository>();
builder.Services.AddScoped(provider =>
{
    var blobOptions = provider.GetRequiredService<IOptions<BlobStorageOptions>>();

    var blobClient = new BlobServiceClient(blobOptions.Value.ConnectionString);
    var containerClient = blobClient.GetBlobContainerClient(blobOptions.Value.ContainerName);
    containerClient.CreateIfNotExists();

    return containerClient;
});

#endregion

#region Database

builder.AddSqlServerDbContext<AssistantDbContext>("AssistantManagerDb");

#endregion

builder.Host.AddAssistantServices();
builder.Services.AddMediatR(opts => { opts.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()); });

builder.Services.AddSingleton(sp =>
{
    return new ServiceBusClient(builder.Configuration.GetConnectionString("ServiceBus"));
});

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AssistantDbContext>();
    dbContext.Database.Migrate();
}

app.Run();