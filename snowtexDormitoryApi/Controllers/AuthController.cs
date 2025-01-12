using Microsoft.AspNetCore.Mvc;
using snowtexDormitoryApi.Data;
using snowtexDormitoryApi.DTOs;
using snowtexDormitoryApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace snowtexDormitoryApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AuthDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup(SignupRequestDto request)
    {
        // Check if the email or phone already exists
        if (_context.Users.Any(u => u.email == request.email))
            return BadRequest(new { status = 400, message = "Email already exists." });

        if (_context.Users.Any(u => u.phone == request.phone))
            return BadRequest(new { status = 400, message = "Phone number already exists." });

        // Hash the password before saving
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.password);

        // Create a new user model and populate its properties
        var user = new UserModel
        {
            name = request.name,
            companyName = request.companyName,
            email = request.email,
            phone = request.phone,
            password = hashedPassword,
            accountType = request.accountType,  // Ensure accountType is a string
            createdDate = DateTime.UtcNow
        };

        // Add the new user to the database
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Return success response with status 201
        return CreatedAtAction(nameof(Signup), new
        {
            status = 201,
            message = "Dormitory user created successfully !",
            user = new { user.name, user.email }
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        // Find the user by email
        var user = _context.Users.FirstOrDefault(u => u.email == request.email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.password, user.password))
            return Unauthorized(new { status = 401, message = "Invalid email or password." });

        // Generate the JWT token
        var token = GenerateJwtToken(user);

        // Return success response with token and user details
        return Ok(new
        {
            status = 200,
            message = "Login successful!",
            user = new
            {
                user.name,
                user.email,
                token
            }
        });
    }

    private string GenerateJwtToken(UserModel user)
    {
        // Get current timestamp and expiration timestamp
        var currentTimestamp = DateTime.UtcNow;
        //var expirationTimestamp = currentTimestamp.AddHours(2);
        var expirationTimestamp = currentTimestamp.AddDays(5);
        // Create claims with the specified user details and expiration time
        var claims = new[]
        {
            new Claim("userId", user.userId.ToString()),
            new Claim("name", user.name),
            new Claim("email", user.email),
            new Claim("exp", ((DateTimeOffset)expirationTimestamp).ToUnixTimeSeconds().ToString())
        };

        // Create JWT token with claims
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: expirationTimestamp,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Append("authToken", "", new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(-1),
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        });
        return Ok(new { message = "Logged out successfully" });
    }




}

