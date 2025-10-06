namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a validation result
/// </summary>
public class ValidationResult
{
    public bool IsValid => !Errors.Any();
    public List<string> Errors { get; set; } = new List<string>();
    public List<string> Warnings { get; set; } = new List<string>();
}
