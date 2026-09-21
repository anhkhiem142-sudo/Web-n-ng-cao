namespace DatLichKhamBenh.Web.Models.ViewModels;

public class DashboardStatsViewModel
{
    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public int TotalRooms { get; set; }
    public int AppointmentsToday { get; set; }

    public Dictionary<string, int> ByStatus { get; set; } = new();
    public List<DailyCount> Last14Days { get; set; } = new();
    public List<DoctorCount> TopDoctors { get; set; } = new();

    public record DailyCount(string Date, int Count);
    public record DoctorCount(string DoctorName, int Count);
}
