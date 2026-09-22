using System.Security.Claims;
using DatLichKhamBenh.Web.Data;
using DatLichKhamBenh.Web.Models;
using DatLichKhamBenh.Web.Models.ViewModels;
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

    // Self-service: lists every patient profile the logged-in account manages
    // (their own profile created at registration, plus any relatives they added).
    [Authorize(Roles = Roles.Patient)]
    public async Task<IActionResult> Profile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var profiles = await context.Patients
            .Where(p => p.ApplicationUserId == userId)
            .OrderByDescending(p => p.Relationship == PatientRelationships.Self)
            .ThenBy(p => p.FullName)
            .ToListAsync();
        return View(profiles);
    }

    [Authorize(Roles = Roles.Patient)]
    public async Task<IActionResult> EditProfile(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var patient = await context.Patients.FirstOrDefaultAsync(p => p.Id == id && p.ApplicationUserId == userId);
        if (patient is null) return NotFound();
        return View(patient);
    }

    [Authorize(Roles = Roles.Patient)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(int id, Patient patient)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var existing = await context.Patients.FirstOrDefaultAsync(p => p.Id == id && p.ApplicationUserId == userId);
        if (existing is null) return NotFound();

        if (!ModelState.IsValid) return View(patient);

        existing.FullName = patient.FullName;
        existing.DateOfBirth = patient.DateOfBirth;
        existing.Gender = patient.Gender;
        existing.Phone = patient.Phone;
        existing.Address = patient.Address;

        await context.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật hồ sơ.";
        return RedirectToAction(nameof(Profile));
    }

    [Authorize(Roles = Roles.Patient)]
    public IActionResult AddRelative()
    {
        ViewBag.RelationshipOptions = PatientRelationships.RelativeOptions;
        return View(new RelativeFormViewModel());
    }

    [Authorize(Roles = Roles.Patient)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRelative(RelativeFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.RelationshipOptions = PatientRelationships.RelativeOptions;
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        context.Patients.Add(new Patient
        {
            ApplicationUserId = userId,
            FullName = model.FullName,
            DateOfBirth = model.DateOfBirth,
            Gender = model.Gender,
            Phone = model.Phone,
            Address = model.Address,
            Relationship = model.Relationship
        });
        await context.SaveChangesAsync();
        TempData["Success"] = "Đã thêm hồ sơ người thân.";
        return RedirectToAction(nameof(Profile));
    }

    // Fetch endpoint used by the booking form's "+ Thêm người thân" modal so a relative
    // can be added and selected without leaving the appointment booking page.
    [Authorize(Roles = Roles.Patient)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRelativeAjax([FromForm] RelativeFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Where(kv => kv.Value?.Errors.Count > 0)
                .ToDictionary(kv => kv.Key, kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
            return BadRequest(new { errors });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var patient = new Patient
        {
            ApplicationUserId = userId,
            FullName = model.FullName,
            DateOfBirth = model.DateOfBirth,
            Gender = model.Gender,
            Phone = model.Phone,
            Address = model.Address,
            Relationship = model.Relationship
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        return Json(new { id = patient.Id, label = $"{patient.FullName} ({patient.Relationship})" });
    }
}
