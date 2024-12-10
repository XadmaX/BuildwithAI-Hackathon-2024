using Assistant.Functions.Abstractions.Attributes;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;


[assembly: AssistantFunction]
namespace ImageGenerator;

public class LoadModule : Module
{
    /// <inheritdoc />
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);
        var services = new ServiceCollection();
        builder.Populate(services);
    }
}