using System.Text;
using expense_tracker.Dtos.AuthLogs;
using expense_tracker.Dtos.Registration;
using expense_tracker.Utilities;

namespace expense_tracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController(Argon2HasherService argon2HasherService, ExpenseTrackerContext context)
        : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<RegistrationResponseDto>> Register(RegistrationRequestDto requestDto)
        {
            // Validate the requestDto
            if (string.IsNullOrWhiteSpace(requestDto.Username) || string.IsNullOrWhiteSpace(requestDto.Password))
            {
                return BadRequest(new RegistrationResponseDto
                {
                    StatusCode = 400,
                    Message = "Username and password cannot be empty",
                    Errors = new List<string> { "Username and password cannot be empty." }
                });
            }

            await using var transaction = await context.Database.BeginTransactionAsync(); // Start transaction
            try
            {
                // Check whether the username is taken or not
                var user = await context.AppUsers.FirstOrDefaultAsync(au => au.Username == requestDto.Username);
                if (user != null)
                {
                    return Conflict(new RegistrationResponseDto()
                    {
                        StatusCode = 400,
                        Message = "Username is already taken",
                        Errors = new List<string> { "Username is already taken." }
                    });
                }
                
                var salt = Encoding.UTF8.GetBytes(RandomSaltGenerator.GenerateSalt(512 / 8));
                var password = Encoding.UTF8.GetBytes(requestDto.Password);
                
                // We have the hash now
                var hash = argon2HasherService.HashPassword(password, salt);
                
                // Create a new user entity
                var newUser = new AppUser
                {
                    Username = requestDto.Username,
                    FirstName = requestDto.FirstName,
                    LastName = requestDto.LastName,
                    HashPassword = Convert.ToBase64String(hash),
                    HashKey = Convert.ToBase64String(salt),
                };

                await context.AppUsers.AddAsync(newUser);
                await context.SaveChangesAsync();

                var authLog = new AuthLog
                {
                    UserId = newUser.Id,
                    Type = LogType.REGISTRATION.ToString(),
                    Date = DateTime.UtcNow
                };

                await context.AuthLogs.AddAsync(authLog);
                await context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new RegistrationResponseDto
                {
                    StatusCode = 200,
                    Message = "User registered successfully",
                    Errors = new List<string>()
                }); // Return success message
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Rollback the transaction if an error occurs
                return StatusCode(500, new RegistrationResponseDto
                {
                    StatusCode = 500,
                    Message = "An error occurred while processing your request.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}
