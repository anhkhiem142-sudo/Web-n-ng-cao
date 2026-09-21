using System.ComponentModel.DataAnnotations;

namespace DatLichKhamBenh.Web.Models;

public class Appointment
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Bệnh nhân")]
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn bác sĩ")]
    [Display(Name = "Bác sĩ")]
    public int DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    [Required]
    [Display(Name = "Phòng khám")]
    public int RoomId { get; set; }
    public Room? Room { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày khám")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày khám")]
    public DateTime AppointmentDate { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn giờ khám")]
    [Display(Name = "Giờ khám")]
    public TimeSpan TimeSlot { get; set; }

    [Display(Name = "Trạng thái")]
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    [StringLength(1000)]
    [Display(Name = "Lý do khám")]
    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public MedicalInfo? MedicalInfo { get; set; }
}
