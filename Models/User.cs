namespace DoctorAppBackend.Models
{
    public class User
    {
        public int UserId { get; set; }   // Primary Key for the User (Patient)
        public string Name { get; set; }   // Name of the patient
        public string Email { get; set; }  // Email for the patient
        public string Password { get; set; }
        public string? Phone { get; set; }  // Phone number (optional)
//added 20thaprill
        public string Role { get; set; } // 👈 "Patient" or "Doctor"
        public string? Gender { get; set; }  // 👈 Add this
        public string? Dob { get; set; }  // 👈 Add this
        public string? Address1 { get; set; }  // 👈 Add this
        public string? Address2 { get; set; }  // 👈 Add this

        public byte[]? ImageData { get; set; }  // 👈 Add this



        // You can add other properties as required, e.g., Address, Date of Birth, etc.
    }
}
