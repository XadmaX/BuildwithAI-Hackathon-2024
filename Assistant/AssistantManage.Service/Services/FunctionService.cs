using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using Assistant.Functions.Abstractions.Interfaces;
using Assistant.Service.Interfaces;
using Assistant.Service.JsonSchema;
using Assistant.Service.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.Generation;
using OpenAI.Assistants;

namespace Assistant.Service.Services
{
    internal class FunctionService : IFunctionService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<FunctionService> _logger;
        private readonly OpenAIOptions _options;

        public FunctionService(
            IServiceProvider provider,
            ILogger<FunctionService> logger,
            IOptions<OpenAIOptions> options)
        {
            _provider = provider;
            _logger = logger;
            _options = options.Value;
        }

        public static Dictionary<string, ToolDefinition> GetFunctionsForRegistration()
        {
            var schemaGenOptions = new SystemTextJsonSchemaGeneratorSettings()
            {
                SchemaProcessors = { new FunctionRequestSchemaProcessor() },
                SchemaType = SchemaType.JsonSchema
            };
            var generator = new JsonSchemaGenerator(schemaGenOptions);
            
            var method = typeof(IFunction<>).GetMethods().Single();

            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes().Where(w =>
                w.GetInterfaces().Any(a => a.IsGenericType && a.GetGenericTypeDefinition() == typeof(IFunction<>))));

            var functions = new ConcurrentDictionary<string, ToolDefinition>();

            Parallel.ForEach(types, type =>
            {
                var descriptionAttribute = type.GetCustomAttribute<DescriptionAttribute>();
                var parameterType = type.GetMethod(method.Name)?.GetParameters().Where(w => w.ParameterType != typeof(CancellationToken)).Single().ParameterType;

                if (parameterType != null)
                {
                    var functionDefinition = new FunctionToolDefinition
                    {
                        Description = descriptionAttribute == null ? string.Empty : descriptionAttribute.Description,
                        FunctionName = type.Name,
                        Parameters =
                            BinaryData.FromString(generator.Generate(parameterType).ToJson())
                    };

                    functions.AddOrUpdate(type.Name,
                        _ => functionDefinition,
                        (_, _) => functionDefinition);
                }
            });

            return functions.ToDictionary();
        }

        public async Task<ToolOutput> ExecuteFunctionAsync(RequiredAction action, CancellationToken cancellationToken)
        {
            var funcInterfaceType = typeof(IFunction<>);
            var funcType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).FirstOrDefault(w =>
                w.Name == action.FunctionName && w.GetInterfaces()
                    .Any(a => a.IsGenericType && a.GetGenericTypeDefinition() == funcInterfaceType));

            if (funcType == null)
                throw new ArgumentNullException(nameof(funcType));

            var ctor = funcType.GetConstructors().Single();
            var ctorParameters = ctor.GetParameters();

            var func = ctorParameters.Any()
                ? ctor.Invoke(ctorParameters.Select(s => _provider.GetService(s.ParameterType)).ToArray())
                : Activator.CreateInstance(funcType);

            MethodInfo method = funcType.GetMethod(funcInterfaceType.GetMethods().Single().Name)!;
            var parameters = method.GetParameters();

            var funcParameterType = parameters.Where(w => w.ParameterType != typeof(CancellationToken)).Single().ParameterType;

            object? output = null;

            try
            {
                output = method.Invoke(func,
                    [JsonConvert.DeserializeObject(action.FunctionArguments, funcParameterType), cancellationToken]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ToolOutput(action.ToolCallId,
                    "Виникла помилка виконання функції, ще раз згенеруй запит та спробуй викликати функцію ще раз");
            }

            var serializedData =
                JsonConvert.SerializeObject(await (Task<object>)output!, Formatting.None);

            if (serializedData.Length > _options.MaxFunctionResponseLength)
                return new ToolOutput(action.ToolCallId,
                    JsonConvert.SerializeObject(new { result = "Response is to large" }, Formatting.None));

            return new ToolOutput(action.ToolCallId, serializedData);
        }
    }
}