using System.Text;
using expense_tracker.Dtos.Login;

namespace expense_tracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController (ExpenseTrackerContext context, Argon2HasherService argon2HasherService): ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<LoginResponseDto>> LoginAsync([FromBody] LoginRequestDto requestDto)
    {
        try
        {
            // fetching user
            var user = await context.AppUsers.FirstOrDefaultAsync(au => au.Username.ToLower() == requestDto.Username.ToLower());
            if (user == null)
            {
                return NotFound(new LoginResponseDto()
                {
                    StatusCode = 404,
                    Message = "User not found",
                    Errors = new List<string> { "Provided Username is incorrect" }
                });
            }
            
            // checking password
            var password = Encoding.UTF8.GetBytes(requestDto.Password);
            var result = argon2HasherService.VerifyHash(Convert.FromBase64String(user.HashPassword),  password, Convert.FromBase64String(user.HashKey));
            if (result)
            
                return Ok(new LoginResponseDto()
                {
                    StatusCode = 200,
                    Message = "Login successful",
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName ?? "",
                });
            else
                return BadRequest(new LoginResponseDto()
                {
                    StatusCode = 400,
                    Message = "Password is incorrect",
                    Errors = new List<string> { $"Provided Password is incorrect for user {requestDto.Username}" }
                });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}