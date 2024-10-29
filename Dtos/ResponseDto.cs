namespace expense_tracker.Dtos;

public class ResponseDto
{
    public bool Success { get; set; } = false;
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public IList<string> Errors { get; set; } = [];
}