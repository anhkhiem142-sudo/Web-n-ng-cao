using System.Security.Claims;
using DatLichKhamBenh.Web.Data;
using DatLichKhamBenh.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatLichKhamBenh.Web.Controllers;

[Authorize]
public class PatientsController(ApplicationDbContext context) : Controller
{
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Index(string? search)
    {
        var query = context.Patients.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.FullName.Contains(search) || (p.Phone != null && p.Phone.Contains(search)));
        }
        ViewBag.Search = search;
        var patients = await query.OrderBy(p => p.FullName).ToListAsync();
        return View(patients);
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Details(int id)
    {
        var patient = await context.Patients
            .Include(p => p.Appointments).ThenInclude(a => a.Doctor)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (patient is null) return NotFound();
        return View(patient);
    }

    [Authorize(Roles = Roles.Admin)]
    public IActionResult Create() => View(new Patient { DateOfBirth = DateTime.Today.AddYears(-30) });

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Patient patient)
    {
        if (!ModelState.IsValid) return View(patient);

        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        TempData["Success"] = "Đã thêm bệnh nhân mới.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var patient = await context.Patients.FindAsync(id);
        if (patient is null) return NotFound();
        return View(patient);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Patient patient)
    {
        if (id != patient.Id) return NotFound();
        if (!ModelState.IsValid) return View(patient);

        var existing = await context.Patients.FindAsync(id);
        if (existing is null) return NotFound();

        existing.FullName = patient.FullName;
        existing.DateOfBirth = patient.DateOfBirth;
        existing.Gender = patient.Gender;
        existing.Phone = patient.Phone;
        existing.Address = patient.Address;

        await context.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật thông tin bệnh nhân.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var patient = await context.Patients.FirstOrDefaultAsync(p => p.Id == id);
        if (patient is null) return NotFound();
        return View(patient);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var patient = await context.Patients.Include(p => p.Appointments).FirstOrDefaultAsync(p => p.Id == id);
        if (patient is null) return NotFound();

        if (patient.Appointments.Count > 0)
        {
            TempData["Error"] = "Không thể xóa bệnh nhân đang có lịch khám.";
            return RedirectToAction(nameof(Index));
        }

        context.Patients.Remove(patient);
        await context.SaveChangesAsync();
        TempData["Success"] = "Đã xóa bệnh nhân.";
        return RedirectToAction(nameof(Index));
    }

    // Self-service profile for the logged-in patient.
    [Authorize(Roles = Roles.Patient)]
    public async Task<IActionResult> Profile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var patient = await context.Patients.FirstOrDefaultAsync(p => p.ApplicationUserId == userId);
        if (patient is null) return NotFound();
        return View(patient);
    }

    [Authorize(Roles = Roles.Patient)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(Patient patient)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var existing = await context.Patients.FirstOrDefaultAsync(p => p.ApplicationUserId == userId);
        if (existing is null) return NotFound();

        if (!ModelState.IsValid) return View(patient);

        existing.FullName = patient.FullName;
        existing.DateOfBirth = patient.DateOfBirth;
        existing.Gender = patient.Gender;
        existing.Phone = patient.Phone;
        existing.Address = patient.Address;

        await context.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật hồ sơ của bạn.";
        return RedirectToAction(nameof(Profile));
    }
}
