using System.ComponentModel.DataAnnotations;

namespace DatLichKhamBenh.Web.Models;

public class Patient
{
    public int Id { get; set; }

    public string? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(100)]
    [Display(Name = "Họ tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập ngày sinh")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày sinh")]
    public DateTime DateOfBirth { get; set; }

    [Display(Name = "Giới tính")]
    public Gender Gender { get; set; }

    [Phone]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    [StringLength(300)]
    [Display(Name = "Địa chỉ")]
    public string? Address { get; set; }

    /// <summary>
    /// Quan hệ với chủ tài khoản đã tạo hồ sơ này: "Bản thân" cho hồ sơ tự động tạo khi đăng ký,
    /// hoặc "Con", "Vợ/Chồng", "Bố/Mẹ", "Khác" cho hồ sơ người thân được thêm sau.
    /// Null đối với bệnh nhân do Admin tạo trực tiếp (không gắn tài khoản đăng nhập).
    /// </summary>
    [StringLength(50)]
    [Display(Name = "Quan hệ với tài khoản")]
    public string? Relationship { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
