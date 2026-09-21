using System.ComponentModel.DataAnnotations;

namespace DatLichKhamBenh.Web.Models.ViewModels;

public class DoctorFormViewModel
{
    public int Id { get; set; }

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

    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress]
    [Display(Name = "Email đăng nhập")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn phòng khám")]
    [Display(Name = "Phòng khám")]
    public int RoomId { get; set; }

    [StringLength(1000)]
    [Display(Name = "Giới thiệu")]
    public string? Bio { get; set; }

    [Display(Name = "Mật khẩu đăng nhập (chỉ khi tạo mới)")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
}
