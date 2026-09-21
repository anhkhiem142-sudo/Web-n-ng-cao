using Microsoft.AspNetCore.Identity;

namespace DatLichKhamBenh.Web.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? Address { get; set; }
}
