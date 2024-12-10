using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using NJsonSchema.Generation;

namespace Assistant.Service.JsonSchema;

internal class FunctionRequestSchemaProcessor : ISchemaProcessor
{
    /// <inheritdoc />
    public void Process(SchemaProcessorContext context)
    {
        foreach (var property in context.Schema.Properties)
        {
            var propertyInfo = context.ContextualType.Type.GetProperty(property.Key)!;

            var description = propertyInfo.GetCustomAttribute<DescriptionAttribute>();
            if (description != null)
            {
                property.Value.Description = description.Description;
            }

            property.Value.IsRequired = Attribute.IsDefined(propertyInfo, typeof(RequiredAttribute));
        }
    }
}