namespace SimpleLlmAgents.Services.Template.Models
{
    public class Parameter
    {
        public Parameter(string name, string description, bool isRequired)
        {
            Name = name;
            Description = description;
            IsRequired = isRequired;
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }
    }
}
