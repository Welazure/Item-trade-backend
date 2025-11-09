namespace Trading.Dto;

public class MediaResponse
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public string MediaType { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime UploadedAt { get; set; }
    public string Url { get; set; }
}
