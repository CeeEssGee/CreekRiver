using System.ComponentModel.DataAnnotations;

namespace CreekRiver.Models;

public class Campsite
{
    public int Id { get; set; }
    [Required]
    public string Nickname { get; set; }
    public string ImageUrl { get; set; }
    // foreign key to CampsiteType and will have a FK constraint on the column
    public int CampsiteTypeId { get; set; }
    public CampsiteType CampsiteType { get; set; }
    public List<Reservation> Reservations { get; set; }
}