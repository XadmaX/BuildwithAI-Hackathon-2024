namespace SimpleLlmAgents.Services.SmpleLlmAgent.Models
{
    public class Agent
    {
        public Agent(Template.Models.Template template)
        {
            Name = template.Name;
            Description = template.Description;
            Template = template;
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public Template.Models.Template Template { get; set; }
    }
}
