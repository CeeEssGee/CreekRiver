using System.ComponentModel.DataAnnotations;

namespace CreekRiver.Models;

public class CampsiteType
{
    // EF Core will automatically make a property called Id in C# into the PRIMARY KEY column for the corresponding SQL database table.
    public int Id { get; set; }
    [Required] //attribute
    public string CampsiteTypeName { get; set; }
    // numbers are automatically not nullable
    public int MaxReservationDays { get; set; }
    // numbers are automatically not nullable
    public decimal FeePerNight { get; set; }
}