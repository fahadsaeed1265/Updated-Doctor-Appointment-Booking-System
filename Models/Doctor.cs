using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppBackend.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        // 👇 Link to Users table — this connects Doctor profile to login
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }   // Navigation property

        public string Name { get; set; }
        public string Email { get; set; }
        public string Experience { get; set; }
        public decimal Fees { get; set; }
        public string About { get; set; }
        public string Speciality { get; set; }
        public string Degree { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public byte[] ImageData { get; set; }
        public bool Available { get; set; } = true;
        public string Status { get; set; } = "Pending";
        // ❌ Removed Password — Doctor uses User table password for login
    }
}