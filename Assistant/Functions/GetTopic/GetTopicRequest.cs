using System.ComponentModel;

namespace GetTopic
{
    [Description("Get a topic empty request")]
    public class GetTopicRequest
    {
        [Description("Empty request")]
        public string Empty { get; set; }
    }
}
