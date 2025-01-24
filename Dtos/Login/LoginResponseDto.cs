namespace expense_tracker.Dtos.Login;

public class LoginResponseDto: ResponseDto
{
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}