namespace CreekRiver.Models;

public class Reservation
{
    public int Id { get; set; }
    // foreign key
    public int CampsiteId { get; set; }
    public Campsite Campsite { get; set; }
    // foreign key
    public int UserProfileId { get; set; }
    // Will be ignored by EF Core when creating the UserProfile tablle but we can use it to store the UserProfile data
    public UserProfile UserProfile { get; set; }
    public DateTime CheckinDate { get; set; }
    public DateTime CheckoutDate { get; set; }
    // calculate total nights
    public int TotalNights => (CheckoutDate - CheckinDate).Days;
}