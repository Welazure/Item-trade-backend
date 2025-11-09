namespace Trading.Dto;

public class UploadMediaResponse
{
    public bool Success { get; set; }
    public List<MediaResponse> UploadedFiles { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
