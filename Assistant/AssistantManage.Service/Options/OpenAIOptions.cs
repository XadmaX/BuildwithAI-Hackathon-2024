namespace Assistant.Service.Options
{
    public class OpenAIOptions
    {
        public string Host { get; set; }
        public string Key { get; set; }
        public int MaxFunctionResponseLength { get; set; } = 2000;
    }
}
