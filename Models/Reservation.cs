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
    // same as:
    // public int TotalNight
    // {
    //     get
    //     {
    //         return (CheckoutDate - CheckinDate).Days;
    //     }
    // }

    // This class member is called a field. Fields do not have get or set, which is how you know it is not a property.
    // It is marked as private. This is another access modifier, like (public and protected). It prevents this field from being accessed outside of code that is contained in the class (most fields are either private or protected). One of the results of this is that it will be ignored in the JSON, so it won't appear in HTTP responses.
    // It is also marked as static. This means that the value for this field will be shared across all instances of Reservation
    // It is also marked as readonly, so that once set, its value cannot change.
    // This is not exactly new, but just as a reminder, to indicate that a numeric value is a decimal and not an int or double, there is an M at the end of the number.
    // By convention, private fields have a prepended _.
    // The purpose of this field is to store the base reservation cost, which is charged no matter how many nights are reserved. It is the same for every reservation, so we want all reservation instances to share it, but we don't need to send it back with every reservation in the JSON.
    private static readonly decimal _baseReservationFee = 10M;

    // This property multiplies the fee per night for this campsite by the total nights, and then adds the base reservation fee to calculate the total cost. Because the Campsite and Campsite.CampsiteType properties may not always be set on this model, the get first has to check to make sure that they have values.
    // Fortunately, the endpoint to get reservations already includes CampsiteType, so test it again to see the totalCost values appear in the JSON response for each reservation.
    public decimal? TotalCost
    {
        get
        {
            if (Campsite?.CampsiteType != null)
            {
                return Campsite.CampsiteType.FeePerNight * TotalNights + _baseReservationFee;
            }
            return null;
        }
    }
}