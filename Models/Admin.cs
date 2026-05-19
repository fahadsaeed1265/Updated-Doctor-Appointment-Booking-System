namespace DoctorAppBackend.Models
{
    public class Admin
    {



        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }

    }
}
