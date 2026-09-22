using System.Security.Claims;
using DatLichKhamBenh.Web.Data;
using DatLichKhamBenh.Web.Models;
using DatLichKhamBenh.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DatLichKhamBenh.Web.Controllers;

[Authorize]
public class AppointmentsController(ApplicationDbContext context) : Controller
{
    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // Role-aware list: patients see their own appointments, doctors see appointments assigned
    // to them, admins see everything with optional filters.
    public async Task<IActionResult> Index(AppointmentStatus? status, int? doctorId, DateTime? date)
    {
        IQueryable<Appointment> query = context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Room);

        if (User.IsInRole(Roles.Patient))
        {
            var patient = await context.Patients.FirstOrDefaultAsync(p => p.ApplicationUserId == CurrentUserId);
            if (patient is null) return Forbid();
            query = query.Where(a => a.PatientId == patient.Id);
        }
        else if (User.IsInRole(Roles.Doctor))
        {
            var doctor = await context.Doctors.FirstOrDefaultAsync(d => d.ApplicationUserId == CurrentUserId);
            if (doctor is null) return Forbid();
            query = query.Where(a => a.DoctorId == doctor.Id);
        }
        // Admin: no extra filter, sees all appointments.

        if (status.HasValue) query = query.Where(a => a.Status == status);
        if (doctorId.HasValue) query = query.Where(a => a.DoctorId == doctorId);
        if (date.HasValue) query = query.Where(a => a.AppointmentDate.Date == date.Value.Date);

        if (User.IsInRole(Roles.Admin))
        {
            ViewBag.Doctors = new SelectList(await context.Doctors.OrderBy(d => d.FullName).ToListAsync(), "Id", "FullName", doctorId);
        }
        ViewBag.Status = status;
        ViewBag.Date = date;

        var appointments = await query.OrderByDescending(a => a.AppointmentDate).ThenBy(a => a.TimeSlot).ToListAsync();
        return View(appointments);
    }

    [Authorize(Roles = Roles.Patient)]
    public async Task<IActionResult> Create(int? doctorId, int? patientId)
    {
        if (!patientId.HasValue)
        {
            var selfProfile = await context.Patients.FirstOrDefaultAsync(p =>
                p.ApplicationUserId == CurrentUserId && p.Relationship == PatientRelationships.Self);
            patientId = selfProfile?.Id;
        }

        await PopulateBookingViewBagsAsync(doctorId, patientId);
        return View(new BookAppointmentViewModel { DoctorId = doctorId ?? 0, PatientId = patientId ?? 0 });
    }

    [Authorize(Roles = Roles.Patient)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookAppointmentViewModel model)
    {
        var ownedPatientIds = await context.Patients
            .Where(p => p.ApplicationUserId == CurrentUserId)
            .Select(p => p.Id)
            .ToListAsync();
        if (!ownedPatientIds.Contains(model.PatientId))
        {
            ModelState.AddModelError(nameof(model.PatientId), "Người tới khám không hợp lệ.");
        }

        var doctor = await context.Doctors.FindAsync(model.DoctorId);
        if (doctor is null)
        {
            ModelState.AddModelError(nameof(model.DoctorId), "Bác sĩ không hợp lệ.");
        }
        else if (!AppointmentSlots.All.Contains(model.TimeSlot))
        {
            ModelState.AddModelError(nameof(model.TimeSlot), "Giờ khám không hợp lệ.");
        }
        else if (model.AppointmentDate.Date < DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.AppointmentDate), "Không thể đặt lịch cho ngày đã qua.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateBookingViewBagsAsync(model.DoctorId, model.PatientId);
            return View(model);
        }

        var conflict = await context.Appointments.AnyAsync(a =>
            a.DoctorId == model.DoctorId &&
            a.AppointmentDate.Date == model.AppointmentDate.Date &&
            a.TimeSlot == model.TimeSlot &&
            a.Status != AppointmentStatus.Cancelled);
        if (conflict)
        {
            ModelState.AddModelError(string.Empty, "Khung giờ này vừa có người đặt, vui lòng chọn giờ khác.");
            await PopulateBookingViewBagsAsync(model.DoctorId, model.PatientId);
            return View(model);
        }

        var appointment = new Appointment
        {
            PatientId = model.PatientId,
            DoctorId = model.DoctorId,
            RoomId = doctor!.RoomId,
            AppointmentDate = model.AppointmentDate.Date,
            TimeSlot = model.TimeSlot,
            Reason = model.Reason,
            HasInsurance = model.HasInsurance == true,
            Status = AppointmentStatus.Pending
        };

        context.Appointments.Add(appointment);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Khung giờ này vừa có người đặt, vui lòng chọn giờ khác.");
            await PopulateBookingViewBagsAsync(model.DoctorId, model.PatientId);
            return View(model);
        }

        TempData["Success"] = "Đặt lịch khám thành công! Vui lòng chờ xác nhận từ phòng khám.";
        return RedirectToAction(nameof(Details), new { id = appointment.Id });
    }

    private async Task PopulateBookingViewBagsAsync(int? selectedDoctorId, int? selectedPatientId)
    {
        var doctors = await context.Doctors.Include(d => d.Room).OrderBy(d => d.FullName).ToListAsync();
        ViewBag.Doctors = new SelectList(doctors, "Id", "FullName", selectedDoctorId);
        ViewBag.DoctorsData = doctors.Select(d => new { id = d.Id, fullName = d.FullName, specialty = d.Specialty, room = d.Room?.Name });
        ViewBag.Specialties = doctors.Select(d => d.Specialty).Distinct().OrderBy(s => s).ToList();

        var profiles = await context.Patients
            .Where(p => p.ApplicationUserId == CurrentUserId)
            .OrderByDescending(p => p.Relationship == PatientRelationships.Self)
            .ThenBy(p => p.FullName)
            .ToListAsync();
        ViewBag.PatientProfiles = new SelectList(
            profiles.Select(p => new { p.Id, Label = $"{p.FullName} ({p.Relationship ?? "Người thân"})" }),
            "Id", "Label", selectedPatientId);
        ViewBag.RelationshipOptions = PatientRelationships.RelativeOptions;
    }

    // Fetch endpoint used by the booking form: returns which slots are still free for a doctor+date.
    [Authorize(Roles = Roles.Patient)]
    [HttpGet]
    public async Task<IActionResult> AvailableSlots(int doctorId, DateTime date)
    {
        var booked = await context.Appointments
            .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date && a.Status != AppointmentStatus.Cancelled)
            .Select(a => a.TimeSlot)
            .ToListAsync();

        var isPastDate = date.Date < DateTime.Today;
        var result = AppointmentSlots.All.Select(slot => new
        {
            value = slot.ToString(@"hh\:mm"),
            available = !isPastDate && !booked.Contains(slot) && !(date.Date == DateTime.Today && slot <= DateTime.Now.TimeOfDay)
        });

        return Json(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var appointment = await context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Room)
            .Include(a => a.MedicalInfo)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (appointment is null) return NotFound();

        if (!await CanAccessAsync(appointment)) return Forbid();

        return View(appointment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var appointment = await context.Appointments.FindAsync(id);
        if (appointment is null) return NotFound();
        if (!await CanAccessAsync(appointment)) return Forbid();

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
        {
            TempData["Error"] = "Không thể hủy lịch khám ở trạng thái này.";
        }
        else
        {
            appointment.Status = AppointmentStatus.Cancelled;
            await context.SaveChangesAsync();
            TempData["Success"] = "Đã hủy lịch khám.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = $"{Roles.Doctor},{Roles.Admin}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, AppointmentStatus status)
    {
        var appointment = await context.Appointments.FindAsync(id);
        if (appointment is null) return NotFound();
        if (!await CanAccessAsync(appointment)) return Forbid();

        appointment.Status = status;
        await context.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật trạng thái lịch khám.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = Roles.Patient)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMedicalInfo(int id, string symptoms)
    {
        var appointment = await context.Appointments.Include(a => a.MedicalInfo).FirstOrDefaultAsync(a => a.Id == id);
        if (appointment is null) return NotFound();
        if (!await CanAccessAsync(appointment)) return Forbid();

        if (string.IsNullOrWhiteSpace(symptoms))
        {
            TempData["Error"] = "Vui lòng nhập thông tin bệnh.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (appointment.MedicalInfo is null)
        {
            context.MedicalInfos.Add(new MedicalInfo { AppointmentId = id, Symptoms = symptoms });
        }
        else
        {
            appointment.MedicalInfo.Symptoms = symptoms;
            appointment.MedicalInfo.SentAt = DateTime.Now;
        }

        await context.SaveChangesAsync();
        TempData["Success"] = "Đã gửi thông tin bệnh đến bác sĩ.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = Roles.Doctor)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddDiagnosis(int id, string diagnosis, string? doctorNotes)
    {
        var appointment = await context.Appointments.Include(a => a.MedicalInfo).FirstOrDefaultAsync(a => a.Id == id);
        if (appointment is null) return NotFound();
        if (!await CanAccessAsync(appointment)) return Forbid();

        if (appointment.MedicalInfo is null)
        {
            context.MedicalInfos.Add(new MedicalInfo { AppointmentId = id, Symptoms = "(Bác sĩ nhập trực tiếp)", Diagnosis = diagnosis, DoctorNotes = doctorNotes });
        }
        else
        {
            appointment.MedicalInfo.Diagnosis = diagnosis;
            appointment.MedicalInfo.DoctorNotes = doctorNotes;
        }

        await context.SaveChangesAsync();
        TempData["Success"] = "Đã lưu chẩn đoán.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<bool> CanAccessAsync(Appointment appointment)
    {
        if (User.IsInRole(Roles.Admin)) return true;

        if (User.IsInRole(Roles.Patient))
        {
            var patient = await context.Patients.FirstOrDefaultAsync(p => p.ApplicationUserId == CurrentUserId);
            return patient is not null && appointment.PatientId == patient.Id;
        }

        if (User.IsInRole(Roles.Doctor))
        {
            var doctor = await context.Doctors.FirstOrDefaultAsync(d => d.ApplicationUserId == CurrentUserId);
            return doctor is not null && appointment.DoctorId == doctor.Id;
        }

        return false;
    }
}
