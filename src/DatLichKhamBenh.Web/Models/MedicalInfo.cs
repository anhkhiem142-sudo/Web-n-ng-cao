using System.ComponentModel.DataAnnotations;

namespace DatLichKhamBenh.Web.Models;

public class MedicalInfo
{
    public int Id { get; set; }

    [Required]
    public int AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    [Required(ErrorMessage = "Vui lòng mô tả triệu chứng")]
    [StringLength(2000)]
    [Display(Name = "Triệu chứng / thông tin bệnh")]
    public string Symptoms { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Chẩn đoán")]
    public string? Diagnosis { get; set; }

    [StringLength(2000)]
    [Display(Name = "Ghi chú của bác sĩ")]
    public string? DoctorNotes { get; set; }

    public DateTime SentAt { get; set; } = DateTime.Now;
}
