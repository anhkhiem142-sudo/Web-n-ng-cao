using System.ComponentModel.DataAnnotations;

namespace DatLichKhamBenh.Web.Models.ViewModels;

public class RelativeFormViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(100)]
    [Display(Name = "Họ tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập ngày sinh")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày sinh")]
    public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-10);

    [Display(Name = "Giới tính")]
    public Gender Gender { get; set; }

    [Phone]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    [StringLength(300)]
    [Display(Name = "Địa chỉ")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn quan hệ với bạn")]
    [Display(Name = "Quan hệ với bạn")]
    public string Relationship { get; set; } = string.Empty;
}
