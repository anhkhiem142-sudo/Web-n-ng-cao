using System.ComponentModel.DataAnnotations;

namespace DatLichKhamBenh.Web.Models;

public class Doctor
{
    public int Id { get; set; }

    public string? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập họ tên bác sĩ")]
    [StringLength(100)]
    [Display(Name = "Họ tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập chuyên khoa")]
    [StringLength(100)]
    [Display(Name = "Chuyên khoa")]
    public string Specialty { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    [EmailAddress]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn phòng khám")]
    [Display(Name = "Phòng khám")]
    public int RoomId { get; set; }
    public Room? Room { get; set; }

    [StringLength(1000)]
    [Display(Name = "Giới thiệu")]
    public string? Bio { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
