using DatLichKhamBenh.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DatLichKhamBenh.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MedicalInfo> MedicalInfos => Set<MedicalInfo>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Doctor>()
            .HasOne(d => d.Room)
            .WithMany(r => r.Doctors)
            .HasForeignKey(d => d.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Appointment>()
            .HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Appointment>()
            .HasOne(a => a.Room)
            .WithMany()
            .HasForeignKey(a => a.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prevent double-booking the same doctor at the same date/time,
        // except for cancelled appointments which free up the slot again.
        builder.Entity<Appointment>()
            .HasIndex(a => new { a.DoctorId, a.AppointmentDate, a.TimeSlot })
            .HasFilter("[Status] <> 3")
            .IsUnique();

        builder.Entity<Appointment>()
            .HasOne(a => a.MedicalInfo)
            .WithOne(m => m.Appointment)
            .HasForeignKey<MedicalInfo>(m => m.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Patient>()
            .HasOne(p => p.ApplicationUser)
            .WithMany()
            .HasForeignKey(p => p.ApplicationUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Doctor>()
            .HasOne(d => d.ApplicationUser)
            .WithMany()
            .HasForeignKey(d => d.ApplicationUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
