using DoctorAppBackend.Data;
using DoctorAppBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    // ── GET Profile ───────────────────────────────────
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int userId = int.Parse(userIdClaim);

        var user = _context.Users.Find(userId);
        if (user == null)
            return NotFound(new { message = "User not found" });

        return Ok(new
        {
            user.Name,
            user.Email,
            user.Phone,
            user.Gender,
            user.Dob,
            user.Address1,
            user.Address2,
            // 👇 Send image as base64
            Image = user.ImageData != null
                ? Convert.ToBase64String(user.ImageData)
                : null
        });
    }

    // ── UPDATE Profile WITH Image ─────────────────────
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int userId = int.Parse(userIdClaim);

        var user = _context.Users.Find(userId);
        if (user == null)
            return NotFound(new { message = "User not found" });

        // Update text fields
        user.Name = dto.Name;
        user.Phone = dto.Phone;
        user.Gender = dto.Gender;
        user.Dob = dto.Dob;
        user.Address1 = dto.Address1;
        user.Address2 = dto.Address2;

        // 👇 Update image if provided
        if (dto.ImageFile != null)
        {
            using var ms = new MemoryStream();
            await dto.ImageFile.CopyToAsync(ms);
            user.ImageData = ms.ToArray();
        }

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Profile updated!" });
    }
}

// DTO
public class UpdateProfileDto
{
    public string Name { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public string? Dob { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public IFormFile? ImageFile { get; set; }  // 👈 Optional image
}