public class RedmineAttachment
{
    public int Id { get; set; }
    public string Filename { get; set; } = string.Empty;
    public long Filesize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string? Token { get; set; }
}