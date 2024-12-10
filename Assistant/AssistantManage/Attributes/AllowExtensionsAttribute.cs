using System.ComponentModel.DataAnnotations;

namespace AssistantManage.Attributes;

public class AllowExtensionsAttribute(params string[] extensions) : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var files = value as IEnumerable<IFormFile>;

        if (files == null || !files.Any())
            return ValidationResult.Success;

        foreach (var file in files)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!extensions.Contains(extension))
            {
                return new ValidationResult($"Недопустиме розширення файлу в колекції: {extension}.");
            }
        }

        return ValidationResult.Success;
    }
}