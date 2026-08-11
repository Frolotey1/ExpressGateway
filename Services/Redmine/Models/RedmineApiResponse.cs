public class RedmineApiResponse<T>
{
    public T? User { get; set; }
    public T? Issue { get; set; }
    public T? Project { get; set; }
    public T? Group { get; set; }
}
