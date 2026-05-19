using DoctorAppBackend.Models;
using Microsoft.EntityFrameworkCore;


namespace DoctorAppBackend.Data
{


    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Doctor> Doctors { get; set; }

        public DbSet<Appointment> Appointments { get; set; }
   
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }

    }
}

