using System.Security.Claims;
using DatLichKhamBenh.Web.Data;
using DatLichKhamBenh.Web.Models;
using DatLichKhamBenh.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DatLichKhamBenh.Web.Controllers;

[Authorize(Roles = $"{Roles.Admin},{Roles.Doctor}")]
public class DashboardController(ApplicationDbContext context, IMemoryCache cache) : Controller
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

    public IActionResult Index() => View();

    // JSON stats consumed by the dashboard charts via Fetch. Cached briefly per role/user
    // since recomputing the aggregates on every chart refresh would hit the database hard.
    [HttpGet]
    public async Task<IActionResult> StatsData()
    {
        int? scopedDoctorId = null;
        if (User.IsInRole(Roles.Doctor) && !User.IsInRole(Roles.Admin))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var doctor = await context.Doctors.FirstOrDefaultAsync(d => d.ApplicationUserId == userId);
            if (doctor is null) return Forbid();
            scopedDoctorId = doctor.Id;
        }

        var cacheKey = $"dashboard_stats_{scopedDoctorId?.ToString() ?? "all"}";
        var stats = await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await BuildStatsAsync(scopedDoctorId);
        });

        return Json(stats);
    }

    private async Task<DashboardStatsViewModel> BuildStatsAsync(int? scopedDoctorId)
    {
        var appointmentsQuery = context.Appointments.AsQueryable();
        if (scopedDoctorId.HasValue)
        {
            appointmentsQuery = appointmentsQuery.Where(a => a.DoctorId == scopedDoctorId);
        }

        var stats = new DashboardStatsViewModel
        {
            TotalPatients = await context.Patients.CountAsync(),
            TotalDoctors = await context.Doctors.CountAsync(),
            TotalRooms = await context.Rooms.CountAsync(),
            AppointmentsToday = await appointmentsQuery.CountAsync(a => a.AppointmentDate.Date == DateTime.Today)
        };

        var byStatus = await appointmentsQuery
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();
        stats.ByStatus = byStatus.ToDictionary(x => x.Status.ToString(), x => x.Count);

        var since = DateTime.Today.AddDays(-13);
        var dailyRaw = await appointmentsQuery
            .Where(a => a.AppointmentDate.Date >= since)
            .GroupBy(a => a.AppointmentDate.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();
        var dailyLookup = dailyRaw.ToDictionary(x => x.Date, x => x.Count);
        stats.Last14Days = Enumerable.Range(0, 14)
            .Select(offset => since.AddDays(offset))
            .Select(d => new DashboardStatsViewModel.DailyCount(d.ToString("dd/MM"), dailyLookup.GetValueOrDefault(d)))
            .ToList();

        if (!scopedDoctorId.HasValue)
        {
            var topDoctors = await context.Appointments
                .GroupBy(a => a.Doctor!.FullName)
                .Select(g => new { DoctorName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();
            stats.TopDoctors = topDoctors.Select(x => new DashboardStatsViewModel.DoctorCount(x.DoctorName, x.Count)).ToList();
        }

        return stats;
    }
}
