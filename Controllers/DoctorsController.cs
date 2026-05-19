using DoctorAppBackend.Data;
using DoctorAppBackend.Models;
using DoctorAppBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DoctorAppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;
        public DoctorsController(AppDbContext context, IWebHostEnvironment env, EmailService emailService, IConfiguration config)
        {
            _context = context;
            _env = env;
            _emailService = emailService;
            _config = config;
        }

        // ── GET All Doctors (Patients see this list) ──────────
        //[Authorize]
        [HttpGet]
        public IActionResult GetAllDoctors()
        {
            var doctors = _context.Doctors
                .Select(d => new {
                    d.Id,
                    d.Name,
                    d.Speciality,
                    d.Experience,
                    d.Fees,
                    d.About,
                    d.Address1,
                    d.Address2,
                    d.Degree,
                    d.Available,
                    // Convert image to base64 so React can display it
                    Image = d.ImageData != null
                        ? Convert.ToBase64String(d.ImageData)
                        : null
                })
                .ToList();

            return Ok(doctors);
        }

        // ── POST Add Doctor Profile (Doctor fills their details) ──
        [Authorize(Roles = "Doctor")]
        [HttpPost]
        public async Task<IActionResult> AddDoctor([FromForm] DoctorDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("Token invalid");

            int userId = int.Parse(userIdClaim);

            // Handle image upload
            byte[] imageData = null;
            if (dto.ImageFile != null)
            {
                using var ms = new MemoryStream();
                await dto.ImageFile.CopyToAsync(ms);
                imageData = ms.ToArray();
            }

            // Check if profile already exists
            var existing = _context.Doctors
                .FirstOrDefault(d => d.UserId == userId);

            if (existing != null)
            {
                // ✅ UPDATE existing profile
                existing.Name = dto.Name;
                existing.Email = dto.Email;
                existing.Experience = dto.Experience;
                existing.Fees = dto.Fees;
                existing.About = dto.About;
                existing.Speciality = dto.Speciality;
                existing.Degree = dto.Degree;
                existing.Address1 = dto.Address1;
                existing.Address2 = dto.Address2;
                if (imageData != null)
                    existing.ImageData = imageData;

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Profile updated!" });
            }
            else
            {
                // ✅ CREATE new profile with Pending status
                var doctor = new Doctor
                {
                    UserId = userId,
                    Name = dto.Name,
                    Email = dto.Email,
                    Experience = dto.Experience,
                    Fees = dto.Fees,
                    About = dto.About,
                    Speciality = dto.Speciality,
                    Degree = dto.Degree,
                    Address1 = dto.Address1,
                    Address2 = dto.Address2,
                    ImageData = imageData,
                    Available = true,
                    Status = "Pending"
                };

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                // Notify admin about new doctor profile
                try
                {
                    var adminEmail = _config["EmailSettings:AdminEmail"];
                    await _emailService.SendNewDoctorProfileNotification(
                        adminEmail,
                        dto.Name,
                        dto.Email,
                        dto.Speciality,
                        dto.Degree,
                        dto.Experience
                    );
                }
                catch { }

                return Ok(new { success = true, message = "Profile submitted! Please wait for admin approval." });
            }
        }


        // ── GET Single Doctor by ID ───────────────────────
        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetDoctorById(int id)
        {
            var doctor = _context.Doctors
                .Where(d => d.Id == id)
                .Select(d => new
                {
                    d.Id,
                    d.Name,
                    d.Speciality,
                    d.Experience,
                    d.Fees,
                    d.About,
                    d.Address1,
                    d.Address2,
                    d.Degree,
                    d.Available,
                    Image = d.ImageData != null
                        ? Convert.ToBase64String(d.ImageData)
                        : null
                })
                .FirstOrDefault();


    if (doctor == null)
                return NotFound();


    return Ok(doctor);
        }



        // ── GET Doctor's own profile ──────────────────────────
        [Authorize(Roles = "Doctor")]
        [HttpGet("my-profile")]
        public IActionResult GetMyProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId = int.Parse(userIdClaim);

            var doctor = _context.Doctors
                .Where(d => d.UserId == userId)
                .Select(d => new {
                    d.Id,
                    d.Name,
                    d.Speciality,
                    d.Experience,
                    d.Fees,
                    d.About,
                    d.Address1,
                    d.Address2,
                    d.Degree,
                    d.Available,
                    d.Status,  // ✅ add this

                    Image = d.ImageData != null
                        ? Convert.ToBase64String(d.ImageData)
                        : null
                })
                .FirstOrDefault();

            if (doctor == null)
                return NotFound("Profile not found. Please add your profile.");

            return Ok(doctor);
        }
    }

    // DTO for receiving form data
    public class DoctorDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Experience { get; set; }
        public decimal Fees { get; set; }
        public string About { get; set; }
        public string Speciality { get; set; }
        public string Degree { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public IFormFile? ImageFile { get; set; } // ✅ ? makes it optional

    }
}