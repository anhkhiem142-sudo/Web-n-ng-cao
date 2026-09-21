using DatLichKhamBenh.Web.Data;
using DatLichKhamBenh.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DatLichKhamBenh.Web.Controllers;

[Authorize(Roles = Roles.Admin)]
public class RoomsController(ApplicationDbContext context, IMemoryCache cache) : Controller
{
    public const string CacheKey = "rooms_list";

    public async Task<IActionResult> Index()
    {
        var rooms = await context.Rooms.Include(r => r.Doctors).OrderBy(r => r.Name).ToListAsync();
        return View(rooms);
    }

    public async Task<IActionResult> Details(int id)
    {
        var room = await context.Rooms.Include(r => r.Doctors).FirstOrDefaultAsync(r => r.Id == id);
        if (room is null) return NotFound();
        return View(room);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Room room)
    {
        if (!ModelState.IsValid) return View(room);

        context.Rooms.Add(room);
        await context.SaveChangesAsync();
        cache.Remove(CacheKey);
        TempData["Success"] = "Đã thêm phòng khám mới.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var room = await context.Rooms.FindAsync(id);
        if (room is null) return NotFound();
        return View(room);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Room room)
    {
        if (id != room.Id) return NotFound();
        if (!ModelState.IsValid) return View(room);

        context.Rooms.Update(room);
        await context.SaveChangesAsync();
        cache.Remove(CacheKey);
        TempData["Success"] = "Đã cập nhật phòng khám.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var room = await context.Rooms.FirstOrDefaultAsync(r => r.Id == id);
        if (room is null) return NotFound();
        return View(room);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var room = await context.Rooms.Include(r => r.Doctors).FirstOrDefaultAsync(r => r.Id == id);
        if (room is null) return NotFound();

        if (room.Doctors.Count > 0)
        {
            TempData["Error"] = "Không thể xóa phòng khám đang có bác sĩ.";
            return RedirectToAction(nameof(Index));
        }

        context.Rooms.Remove(room);
        await context.SaveChangesAsync();
        cache.Remove(CacheKey);
        TempData["Success"] = "Đã xóa phòng khám.";
        return RedirectToAction(nameof(Index));
    }
}
