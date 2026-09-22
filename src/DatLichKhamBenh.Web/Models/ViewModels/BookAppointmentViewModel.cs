using System.ComponentModel.DataAnnotations;

namespace DatLichKhamBenh.Web.Models.ViewModels;

public class BookAppointmentViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn người tới khám")]
    [Display(Name = "Người tới khám")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn bác sĩ")]
    [Display(Name = "Bác sĩ")]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày khám")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày khám")]
    public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

    [Required(ErrorMessage = "Vui lòng chọn giờ khám")]
    [Display(Name = "Giờ khám")]
    public TimeSpan TimeSlot { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập lý do khám")]
    [StringLength(1000)]
    [Display(Name = "Lý do khám / triệu chứng")]
    public string Reason { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn có sử dụng bảo hiểm y tế hay không")]
    [Display(Name = "Sử dụng bảo hiểm y tế")]
    public bool? HasInsurance { get; set; }
}
