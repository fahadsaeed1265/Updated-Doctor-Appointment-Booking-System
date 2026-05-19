namespace DoctorAppBackend.Models
{
    public class AppointmentStatusDto
    {
        public int AppointmentId { get; set; }
        public string Status { get; set; }  // Example values: "pending", "confirmed", "cancelled"
    }
}
