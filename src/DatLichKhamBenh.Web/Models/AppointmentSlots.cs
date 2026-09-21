namespace DatLichKhamBenh.Web.Models;

public static class AppointmentSlots
{
    // Clinic hours: 08:00-11:30 (morning) and 13:30-17:00 (afternoon), 30-minute slots.
    public static readonly IReadOnlyList<TimeSpan> All = BuildSlots();

    private static List<TimeSpan> BuildSlots()
    {
        var slots = new List<TimeSpan>();
        for (var t = new TimeSpan(8, 0, 0); t <= new TimeSpan(11, 30, 0); t = t.Add(TimeSpan.FromMinutes(30)))
        {
            slots.Add(t);
        }
        for (var t = new TimeSpan(13, 30, 0); t <= new TimeSpan(17, 0, 0); t = t.Add(TimeSpan.FromMinutes(30)))
        {
            slots.Add(t);
        }
        return slots;
    }
}
