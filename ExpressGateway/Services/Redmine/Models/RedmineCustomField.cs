public class RedmineCustomField
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Value { get; set; }
    public List<RedminePossibleValue>? PossibleValues { get; set; }
    public bool IsRequired { get; set; }
    public string? Description { get; set; }
    public string FieldFormat { get; set; } = string.Empty;
}