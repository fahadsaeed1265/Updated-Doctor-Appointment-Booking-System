namespace DoctorAppBackend.Models
{
    public class AppointmentDto
    {
        public string UserId { get; set; }
        public string DocId { get; set; }
        public string SlotDate { get; set; }
        public string SlotTime { get; set; }
    }
}
