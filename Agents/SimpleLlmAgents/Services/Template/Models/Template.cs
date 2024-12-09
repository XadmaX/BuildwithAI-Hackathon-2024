namespace SimpleLlmAgents.Services.Template.Models
{
    public class Template
    {
        public Template(string name, string description, List<Parameter> parameters, string fileName)
        {
            Name = name;
            Description = description;
            Parameters = parameters;
            FileName = fileName;
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Parameter> Parameters { get; set; }
        public string FileName { get; set; }
    }
}
