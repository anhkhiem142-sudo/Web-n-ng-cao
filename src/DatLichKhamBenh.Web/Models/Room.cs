using System.ComponentModel.DataAnnotations;

namespace DatLichKhamBenh.Web.Models;

public class Room
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên phòng khám")]
    [StringLength(100)]
    [Display(Name = "Tên phòng khám")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    [Display(Name = "Vị trí")]
    public string? Location { get; set; }

    [StringLength(500)]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
