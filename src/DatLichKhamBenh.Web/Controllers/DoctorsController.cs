using DatLichKhamBenh.Web.Data;
using DatLichKhamBenh.Web.Models;
using DatLichKhamBenh.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DatLichKhamBenh.Web.Controllers;

public class DoctorsController(ApplicationDbContext context, IMemoryCache cache, UserManager<ApplicationUser> userManager) : Controller
{
    public const string CacheKey = "doctors_list";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    // Public list: any visitor (including anonymous) can browse doctors to decide who to book with.
    // Cached in memory since the doctor roster changes rarely but is read on almost every page.
    [AllowAnonymous]
    public async Task<IActionResult> Index(string? specialty)
    {
        var doctors = await cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await context.Doctors.Include(d => d.Room).OrderBy(d => d.FullName).ToListAsync();
        }) ?? new List<Doctor>();

        ViewBag.Specialties = doctors.Select(d => d.Specialty).Distinct().OrderBy(s => s).ToList();
        ViewBag.SelectedSpecialty = specialty;

        if (!string.IsNullOrWhiteSpace(specialty))
        {
            doctors = doctors.Where(d => d.Specialty == specialty).ToList();
        }

        return View(doctors);
    }

    // Called via Fetch from the specialty dropdown to refresh the doctor grid without a full page reload.
    [AllowAnonymous]
    public async Task<IActionResult> Filter(string? specialty)
    {
        var doctors = await cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await context.Doctors.Include(d => d.Room).OrderBy(d => d.FullName).ToListAsync();
        }) ?? new List<Doctor>();

        if (!string.IsNullOrWhiteSpace(specialty))
        {
            doctors = doctors.Where(d => d.Specialty == specialty).ToList();
        }

        return PartialView("_DoctorCards", doctors);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var doctor = await context.Doctors.Include(d => d.Room).FirstOrDefaultAsync(d => d.Id == id);
        if (doctor is null) return NotFound();
        return View(doctor);
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> ManageIndex()
    {
        var doctors = await context.Doctors.Include(d => d.Room).OrderBy(d => d.FullName).ToListAsync();
        return View(doctors);
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create()
    {
        await PopulateRoomsAsync();
        return View(new DoctorFormViewModel());
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DoctorFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "Vui lòng đặt mật khẩu đăng nhập cho bác sĩ.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateRoomsAsync();
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.Phone,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, model.Password!);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            await PopulateRoomsAsync();
            return View(model);
        }
        await userManager.AddToRoleAsync(user, Roles.Doctor);

        var doctor = new Doctor
        {
            ApplicationUserId = user.Id,
            FullName = model.FullName,
            Specialty = model.Specialty,
            Phone = model.Phone,
            Email = model.Email,
            RoomId = model.RoomId,
            Bio = model.Bio
        };
        context.Doctors.Add(doctor);
        await context.SaveChangesAsync();
        cache.Remove(CacheKey);

        TempData["Success"] = "Đã thêm bác sĩ và tạo tài khoản đăng nhập.";
        return RedirectToAction(nameof(ManageIndex));
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var doctor = await context.Doctors.FindAsync(id);
        if (doctor is null) return NotFound();

        await PopulateRoomsAsync();
        return View(new DoctorFormViewModel
        {
            Id = doctor.Id,
            FullName = doctor.FullName,
            Specialty = doctor.Specialty,
            Phone = doctor.Phone ?? string.Empty,
            Email = doctor.Email ?? string.Empty,
            RoomId = doctor.RoomId,
            Bio = doctor.Bio
        });
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DoctorFormViewModel model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid)
        {
            await PopulateRoomsAsync();
            return View(model);
        }

        var doctor = await context.Doctors.FindAsync(id);
        if (doctor is null) return NotFound();

        doctor.FullName = model.FullName;
        doctor.Specialty = model.Specialty;
        doctor.Phone = model.Phone;
        doctor.Email = model.Email;
        doctor.RoomId = model.RoomId;
        doctor.Bio = model.Bio;

        await context.SaveChangesAsync();
        cache.Remove(CacheKey);
        TempData["Success"] = "Đã cập nhật thông tin bác sĩ.";
        return RedirectToAction(nameof(ManageIndex));
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var doctor = await context.Doctors.Include(d => d.Room).FirstOrDefaultAsync(d => d.Id == id);
        if (doctor is null) return NotFound();
        return View(doctor);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var doctor = await context.Doctors.Include(d => d.Appointments).FirstOrDefaultAsync(d => d.Id == id);
        if (doctor is null) return NotFound();

        if (doctor.Appointments.Count > 0)
        {
            TempData["Error"] = "Không thể xóa bác sĩ đang có lịch khám.";
            return RedirectToAction(nameof(ManageIndex));
        }

        context.Doctors.Remove(doctor);
        await context.SaveChangesAsync();
        cache.Remove(CacheKey);
        TempData["Success"] = "Đã xóa bác sĩ.";
        return RedirectToAction(nameof(ManageIndex));
    }

    private async Task PopulateRoomsAsync()
    {
        ViewBag.Rooms = new SelectList(await context.Rooms.OrderBy(r => r.Name).ToListAsync(), "Id", "Name");
    }
}
