using DoctorAppBackend.Data;
using DoctorAppBackend.Models;
using DoctorAppBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/appointments")]
public class AppointmentController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly EmailService _emailService;

    public AppointmentController(AppDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    // ── 1. Book Appointment (Patient only) ────────────
    [Authorize(Roles = "Patient")]
    [HttpPost("book")]
    public async Task<IActionResult> BookAppointment([FromBody] AppointmentDto data)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("Invalid token");

            int userId = int.Parse(userIdClaim);
            int doctorId = int.Parse(data.DocId);

            // Check doctor exists and is available
            var docData = await _context.Doctors.FindAsync(doctorId);
            if (docData == null || !docData.Available)
                return Ok(new { success = false, message = "Doctor not available" });

            // Check if slot already booked
            var slotTaken = _context.Appointments.Any(a =>
                a.DocId == data.DocId &&
                a.SlotDate == data.SlotDate &&
                a.SlotTime == data.SlotTime
            );
            if (slotTaken)
                return Ok(new { success = false, message = "Slot already booked!" });

            // Get user info
            var userData = await _context.Users.FindAsync(userId);
            if (userData == null)
                return Ok(new { success = false, message = "User not found" });

            // Save appointment
            var appointment = new Appointment
            {
                UserId = userId.ToString(),
                DocId = doctorId.ToString(),
                UserData = userData.Name,
                DocData = docData.Name,
                Amount = docData.Fees,
                SlotTime = data.SlotTime,
                SlotDate = data.SlotDate,

                Date = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                PaymentStatus = data.PaymentStatus ?? "Pending"  // ✅ add this

            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // 👇 Send emails to BOTH patient and doctor
            try
            {
                // ── Email to Patient ──────────────────
                await _emailService.SendAppointmentConfirmation(
                    toEmail: userData.Email,
                    patientName: userData.Name,
                    doctorName: docData.Name,
                    slotDate: data.SlotDate,
                    slotTime: data.SlotTime,
                    fees: docData.Fees
                );

                // ── Email to Doctor ───────────────────
                await _emailService.SendDoctorNotification(
                    doctorEmail: docData.Email,
                    doctorName: docData.Name,
                    patientName: userData.Name,
                    slotDate: data.SlotDate,
                    slotTime: data.SlotTime,
                    fees: docData.Fees
                );
            }
            catch (Exception emailEx)
            {
                // Don't fail booking if email fails
                Console.WriteLine("Email sending failed: " + emailEx.Message);
            }

            return Ok(new { success = true, message = "Appointment Booked! Emails sent to patient and doctor." });
        }
        catch (Exception ex)
        {
            return Ok(new { success = false, message = "Error: " + ex.Message });
        }
    }

    // ── 2. Get Booked Slots for a Doctor ──────────────
    [HttpGet("booked-slots/{docId}")]
    public IActionResult GetBookedSlots(string docId)
    {
        var bookedSlots = _context.Appointments
            .Where(a => a.DocId == docId)
            .Select(a => $"{a.SlotDate}_{a.SlotTime}")
            .ToList();

        return Ok(bookedSlots);
    }

    // ── 3. Get Doctor's own Appointments ──────────────
    [Authorize(Roles = "Doctor")]
    [HttpGet("my-appointments")]
    public IActionResult GetMyAppointments()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int userId = int.Parse(userIdClaim);

        var doctor = _context.Doctors
            .FirstOrDefault(d => d.UserId == userId);

        if (doctor == null)
            return NotFound(new { message = "Please add your profile first" });

        var appointments = _context.Appointments
            .Where(a => a.DocId == doctor.Id.ToString())
            .Select(a => new {
                a.Id,
                PatientName = a.UserData,
                a.SlotDate,
                a.SlotTime,
                a.Amount,
                a.Cancelled,
                a.IsCompleted
            })
            .ToList();

        return Ok(appointments);
    }

    // ── 4. Get Patient's own Appointments ─────────────
    [Authorize(Roles = "Patient")]
    [HttpGet("patient-appointments")]
    public IActionResult GetPatientAppointments()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int userId = int.Parse(userIdClaim);

        var appointments = _context.Appointments
            .Where(a => a.UserId == userId.ToString())
            .Select(a => new {
                a.Id,
                DoctorName = a.DocData,
                a.SlotDate,
                a.SlotTime,
                a.Amount,
                a.Cancelled,
                a.IsCompleted,
                a.PaymentStatus,  // ✅ add this

                DoctorImage = _context.Doctors
                    .Where(d => d.Id.ToString() == a.DocId)
                    .Select(d => d.ImageData != null
                        ? Convert.ToBase64String(d.ImageData)
                        : null)
                    .FirstOrDefault(),
                DoctorSpeciality = _context.Doctors
                    .Where(d => d.Id.ToString() == a.DocId)
                    .Select(d => d.Speciality)
                    .FirstOrDefault()
            })
            .ToList();

        return Ok(appointments);
    }

    // ── 5. Mark as Completed (Doctor only) ────────────
    [Authorize(Roles = "Doctor")]
    [HttpPut("complete/{id}")]
    public async Task<IActionResult> CompleteAppointment(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment == null)
            return NotFound(new { message = "Appointment not found" });

        appointment.IsCompleted = true;
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Marked as completed!" });
    }

    // ── 6. Cancel Appointment (Doctor OR Patient) ─────
    [HttpPut("cancel/{id}")]
    public async Task<IActionResult> CancelAppointment(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment == null)
            return NotFound(new { message = "Appointment not found" });

        appointment.Cancelled = true;
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Appointment cancelled!" });
    }



    // ── 7. Pay for Appointment (Patient only) ─────────
    [Authorize(Roles = "Patient")]
    [HttpPut("pay/{id}")]
    public async Task<IActionResult> PayAppointment(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int userId = int.Parse(userIdClaim);

        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment == null)
            return NotFound(new { message = "Appointment not found" });

        // Make sure this appointment belongs to the logged-in patient
        if (appointment.UserId != userId.ToString())
            return Unauthorized(new { message = "Not your appointment" });

        if (appointment.Cancelled)
            return Ok(new { success = false, message = "Cannot pay for a cancelled appointment" });

        appointment.PaymentStatus = "Paid";
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Payment successful!" });
    }
}

// DTO
public class AppointmentDto
{
    public string DocId { get; set; }
    public string SlotDate { get; set; }
    public string SlotTime { get; set; }
    public string PaymentStatus { get; set; } = "Pending"; // ✅ add this

}