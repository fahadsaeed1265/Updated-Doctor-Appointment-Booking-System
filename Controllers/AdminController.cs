using DoctorAppBackend.Data;
using DoctorAppBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public AdminController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalPatients = await _context.Users.CountAsync(u => u.Role == "Patient");
            var totalDoctors = await _context.Users.CountAsync(u => u.Role == "Doctor");
            var totalAppointments = await _context.Appointments.CountAsync();
            return Ok(new { totalPatients, totalDoctors, totalAppointments });
        }

        [HttpGet("patients")]
        public async Task<IActionResult> GetPatients()
        {
            var patients = await _context.Users
                .Where(u => u.Role == "Patient")
                .Select(u => new { u.UserId, u.Name, u.Email })
                .ToListAsync();
            return Ok(patients);
        }

        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Select(d => new
                {
                    d.UserId,
                    d.Speciality,
                    d.Status,
                    Name = d.Name,
                    Email = d.Email
                }).ToListAsync();
            return Ok(doctors);
        }

        [HttpGet("appointments")]
        public async Task<IActionResult> GetAppointments()
        {
            var appointments = await _context.Appointments
                .Select(a => new
                {
                    a.Id,
                    a.SlotDate,
                    a.SlotTime,
                    a.Status,
                    a.UserId,
                    a.DocId,
                    a.Amount
                }).ToListAsync();
            return Ok(appointments);
        }

        [HttpPut("approve-doctor/{userId}")]
        public async Task<IActionResult> ApproveDoctor(int userId)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor not found.");

            doctor.Status = "Approved";
            await _context.SaveChangesAsync();

            try
            {
                await _emailService.SendDoctorApprovalEmail(doctor.Email, doctor.Name);
            }
            catch (Exception ex)
            {
                // Status was saved, just email failed
                return Ok(new { message = "Doctor approved but email failed: " + ex.Message });
            }

            return Ok(new { message = "Doctor approved and email sent." });
        }

        [HttpPut("reject-doctor/{userId}")]
        public async Task<IActionResult> RejectDoctor(int userId)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor not found.");

            doctor.Status = "Rejected";
            await _context.SaveChangesAsync();

            try
            {
                await _emailService.SendDoctorRejectionEmail(doctor.Email, doctor.Name);
            }
            catch (Exception ex)
            {
                return Ok(new { message = "Doctor rejected but email failed: " + ex.Message });
            }

            return Ok(new { message = "Doctor rejected and email sent." });
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue()
        {
            var totalRevenue = await _context.Appointments
                .Where(a => a.Cancelled == false)
                .SumAsync(a => a.Amount);

            var revenuePerDoctor = await _context.Appointments
                .Where(a => a.Cancelled == false)
                .GroupBy(a => a.DocId)
                .Select(g => new
                {
                    DocId = g.Key,
                    TotalEarned = g.Sum(a => a.Amount)
                }).ToListAsync();

            return Ok(new
            {
                totalRevenue,
                revenuePerDoctor
            });
        }
    }
}