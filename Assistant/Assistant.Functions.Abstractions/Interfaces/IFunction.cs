using System.Threading;

namespace Assistant.Functions.Abstractions.Interfaces;

public interface IFunction<in T> where T : class
{
    Task<object> ExecuteFunction(T request, CancellationToken cancellationToken = default);
}