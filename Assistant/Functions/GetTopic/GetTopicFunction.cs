using Assistant.Functions.Abstractions.Interfaces;
using System.ComponentModel;

namespace GetTopic
{
    [Description("Get a topic of the week")]
    public class GetTopicFunction : IFunction<GetTopicRequest>
    {
        public Task<object> ExecuteFunction(GetTopicRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<object>("чарівний ліс");
        }
    }
}
