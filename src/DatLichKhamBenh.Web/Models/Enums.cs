namespace DatLichKhamBenh.Web.Models;

public enum Gender
{
    Male,
    Female,
    Other
}

public enum AppointmentStatus
{
    Pending,
    Confirmed,
    Completed,
    Cancelled
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string Doctor = "Doctor";
    public const string Patient = "Patient";

    public static readonly string[] All = { Admin, Doctor, Patient };
}
