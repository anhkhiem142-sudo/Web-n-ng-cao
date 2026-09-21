using DatLichKhamBenh.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DatLichKhamBenh.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        const string adminEmail = "admin@gmail.com";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Quản trị viên",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, "admin123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, Roles.Admin);
            }
        }

        if (!await context.Rooms.AnyAsync())
        {
            var rooms = new List<Room>
            {
                new() { Name = "Phòng khám Nội tổng quát", Location = "Tầng 1 - Khu A", Description = "Khám và điều trị các bệnh lý nội khoa thông thường." },
                new() { Name = "Phòng khám Nhi", Location = "Tầng 1 - Khu B", Description = "Khám bệnh cho trẻ em từ 0-16 tuổi." },
                new() { Name = "Phòng khám Tai Mũi Họng", Location = "Tầng 2 - Khu A", Description = "Chuyên khoa Tai - Mũi - Họng." },
                new() { Name = "Phòng khám Da liễu", Location = "Tầng 2 - Khu B", Description = "Chuyên khoa Da liễu, thẩm mỹ da." }
            };
            context.Rooms.AddRange(rooms);
            await context.SaveChangesAsync();

            var doctors = new List<Doctor>
            {
                new() { FullName = "BS. Nguyễn Văn An", Specialty = "Nội tổng quát", Phone = "0901111111", Email = "an.nguyen@dlkb.local", RoomId = rooms[0].Id, Bio = "15 năm kinh nghiệm nội khoa." },
                new() { FullName = "BS. Trần Thị Bình", Specialty = "Nhi khoa", Phone = "0902222222", Email = "binh.tran@dlkb.local", RoomId = rooms[1].Id, Bio = "Chuyên khám và điều trị bệnh lý nhi." },
                new() { FullName = "BS. Lê Văn Cường", Specialty = "Tai Mũi Họng", Phone = "0903333333", Email = "cuong.le@dlkb.local", RoomId = rooms[2].Id, Bio = "Chuyên khoa Tai Mũi Họng." },
                new() { FullName = "BS. Phạm Thị Dung", Specialty = "Da liễu", Phone = "0904444444", Email = "dung.pham@dlkb.local", RoomId = rooms[3].Id, Bio = "Chuyên khoa Da liễu." }
            };
            context.Doctors.AddRange(doctors);
            await context.SaveChangesAsync();
        }
    }
}
