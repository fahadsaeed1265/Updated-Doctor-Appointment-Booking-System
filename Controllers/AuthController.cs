using DoctorAppBackend.Data;
using DoctorAppBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BC = BCrypt.Net.BCrypt;  // 👈 Add this line — this creates the shortcut "BC"





namespace DoctorAppBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }




        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (existingUser != null)
                return BadRequest(new { message = "Email already registered." });

            var hashedPassword = BC.HashPassword(dto.Password);

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = hashedPassword,
                Role = dto.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful." });
        }


        // Controllers/AuthController.cs
        //[HttpPost("register")]
        //public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        //{
        //    // Check if email already exists
        //    var existingUser = await _context.Users
        //        .FirstOrDefaultAsync(u => u.Email == dto.Email);

        //    if (existingUser != null)
        //        return BadRequest(new { message = "Email already registered." });

        //    // Hash the password
        //    var hashedPassword = BC.HashPassword(dto.Password);

        //    var user = new User
        //    {
        //        Name = dto.Name,
        //        Email = dto.Email,
        //        Password = hashedPassword,
        //        Role = dto.Role
        //    };

        //    _context.Users.Add(user);
        //    await _context.SaveChangesAsync(); // ✅ Save first to get user.UserId

        //    // ✅ If registering as Doctor, auto-create a pending Doctor profile
        //    if (dto.Role == "Doctor")
        //    {
        //        var doctorProfile = new Doctor
        //        {
        //            UserId = user.UserId,
        //            Name = user.Name,
        //            Email = user.Email,
        //            Status = "Pending",
        //            Speciality = "Not specified",
        //            Experience = "0",
        //            Fees = 0,
        //            About = "",
        //            Degree = "",
        //            Address1 = "",
        //            Address2 = ""
        //        };
        //        _context.Doctors.Add(doctorProfile);
        //        await _context.SaveChangesAsync();
        //    }

        //    return Ok(new { message = "Registration successful." });
        //}

        //20th aprill

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // 1. Find the user by email
            var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return Unauthorized("Invalid email or password.");

            // 2. Check if password matches the hash
            bool passwordMatch = BC.Verify(dto.Password, user.Password);  // 👈 user.Password not PasswordHash
            if (!passwordMatch)
                return Unauthorized("Invalid email or password.");


            // 2.5 If user is a Doctor, check approval status
            if (user.Role == "Doctor")
            {
                var doctorProfile = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.UserId == user.UserId);

                if (doctorProfile != null && doctorProfile.Status == "Rejected")
                    return Unauthorized("Your account has been rejected. Please contact support.");

                if (doctorProfile != null && doctorProfile.Status == "Pending")
                    return Unauthorized("Your profile is under review. Please wait for admin approval.");
            }

            // 3. Build the JWT Token
            var token = GenerateJwtToken(user.UserId, user.Email, user.Role, user.Name);
            // 4. Send the token + role back to frontend
            return Ok(new
            {
                token = token,
                role = user.Role,  // 👈 Frontend needs this to redirect
                name = user.Name
            });





        }



     



        //20th aprill


      private string GenerateJwtToken(int id, string email, string role, string name)
{
    var jwtKey = _config["Jwt:Key"];
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, id.ToString()),
        new Claim(ClaimTypes.Email, email),
        new Claim(ClaimTypes.Role, role),
        new Claim(ClaimTypes.Name, name)
    };

    var token = new JwtSecurityToken(
        issuer: _config["Jwt:Issuer"],
        audience: _config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddDays(7),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

    



    


    [HttpPost("admin-login")]
        public IActionResult AdminLogin([FromBody] LoginDto dto)
        {
            var admin = _context.Admins.FirstOrDefault(a => a.Email == dto.Email);

            if (admin == null || !BCrypt.Net.BCrypt.Verify(dto.Password, admin.PasswordHash))
                return Unauthorized("Invalid credentials");

            var token = GenerateJwtToken(admin.Id, admin.Email, "Admin", admin.FullName);
            return Ok(new { token, role = "Admin", name = admin.FullName });
        }
    }
}

