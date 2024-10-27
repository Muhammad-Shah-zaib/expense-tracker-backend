namespace expense_tracker.Dtos;

public class ResponseDto
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public IList<string> Errors { get; set; } = [];
}