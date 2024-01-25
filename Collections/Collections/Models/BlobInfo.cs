namespace Collections.Models;

public class BlobInfo
{
    private Stream content;

    public BlobInfo(string content, string contentType)
    {
        Content = content;
        ContentType = contentType;
    }

    public BlobInfo(Stream content, string contentType)
    {
        this.content = content;
        ContentType = contentType;
    }

    public string Content { get; set; }

    public string ContentType { get; set; }
}