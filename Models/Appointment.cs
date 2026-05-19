using System.ComponentModel.DataAnnotations;

namespace DoctorAppBackend.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; } // Primary Key (EF Core needs this)

        [Required]
        public string UserId { get; set; }

        [Required]
        public string DocId { get; set; }

        [Required]
        public string SlotDate { get; set; }

        [Required]
        public string SlotTime { get; set; }

        // In Node, he uses "Object". In C#, we usually reference the 
        // User/Doctor ID, but for now, we'll use strings or a specific DTO
        public string UserData { get; set; }
        public string DocData { get; set; }

        [Required]
        public decimal Amount { get; set; } // Use decimal for money, not number

        [Required]
        public long Date { get; set; } // JavaScript timestamps are usually 'long'

        public bool Cancelled { get; set; } = false;


        public string Status { get; set; } = "Pending";




        public bool IsCompleted { get; set; } = false;

        public string PaymentStatus { get; set; } = "Pending";
    }
}
