using Microsoft.EntityFrameworkCore;
using CreekRiver.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

// DbContext is a class that comes from EF Core that represents our database as .NET objects that we can access. Up until now, you have mostly encountered classes that represent database entities. However, in most .NET projects classes also provide an important way to organize code into abstracted objects that represent different parts of the application, each taking on a specific role. DbContext is just such a class. But this class is actually called CreekRiverDbContext, and after its name you see : DbContext. This is how inheritance is indicated in C#. Inheritance means that a class inherits all of the properties, fields, and methods of another class. In this case, we want our CreekRiverDbContext class to be a DbContext as well. Inheritance indicates an "is-a" relationship between two types. All of the properties of DbContext allow this class to connect to the database with no other code that you have to write.

// The properties on the CreekRiverDbContext class are, obviously, the collections corresponding to the tables in our database. By adding them to this class, we are telling EF Core which classes represent our database entities. DbSet is another class from EF Core, which is like other collections such as List and Array, in that we can write Linq queries to get data from them. What is special about DbSet is that our Linq queries will be transformed into a SQL query, which will be run against the database to get the data for which we are querying.

// Finally, there is something that looks like a method called CreekRiverDbContext. This is a constructor, which is a method-like member of a class that allows us to write extra logic to configure the class, so that it is ready for use when it is created. You can always tell that something is a constructor in a class when: 1. It is public, 2. has the same name as the class itself, and 3. has no return type. In this case, our CreekRiverDbContext class actually doesn't need any special setup, but the DbContext class does. DbContext is our class's base class, and it requires an options object to set itself up properly. We use the base keyword to pass that object down to DbContext when ASP.NET creates the CreekRiverDbContext class.

public class CreekRiverDbContext : DbContext // This is how inheritance is indicated in C# (: DbContext)  Inheritance means that a class inherits all of the properties, fields, and methods of another class. In this case, we want our CreekRiverDbContext class to be a DbContext as well. Inheritance indicates an "is-a" relationship between two types. All of the properties of DbContext allow this class to connect to the database with no other code that you have to write.
{

    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Campsite> Campsites { get; set; }
    public DbSet<CampsiteType> CampsiteTypes { get; set; }

    // The rest of the code in this method will check - every time we create or update the database schema - whether this data is in the database or not, and will attempt to add it if it doesn't find it all. This is very useful for seeding the database when it is created for the first time with test data.
    protected override void OnModelCreating(ModelBuilder modelBuilder) // protected is an access modifier, method can only be called from within the class itself, and is a form of encapsulation, override means this is replacing a method of the same name that is inherited from the DbContext class
    {
        // seed data with campsite types
        modelBuilder.Entity<CampsiteType>().HasData(new CampsiteType[]
        {
            new CampsiteType {Id = 1, CampsiteTypeName = "Tent", FeePerNight = 15.99M, MaxReservationDays = 7},
            new CampsiteType {Id = 2, CampsiteTypeName = "RV", FeePerNight = 26.50M, MaxReservationDays = 14},
            new CampsiteType {Id = 3, CampsiteTypeName = "Primitive", FeePerNight = 10.00M, MaxReservationDays = 3},
            new CampsiteType {Id = 4, CampsiteTypeName = "Hammock", FeePerNight = 12M, MaxReservationDays = 7}
        });
        // seed data with campsites
        modelBuilder.Entity<Campsite>().HasData(new Campsite[]
        {
            new Campsite {Id = 1, CampsiteTypeId = 1, Nickname = "Barred Owl", ImageUrl="https://tnstateparks.com/assets/images/content-images/campgrounds/249/colsp-area2-site73.jpg"},
            new Campsite {Id = 2, CampsiteTypeId = 2, Nickname = "RV Land", ImageUrl="https://tnstateparks.com/assets/images/content-images/campgrounds/249/colsp-area2-site73.jpg"},
            new Campsite {Id = 3, CampsiteTypeId = 3, Nickname = "Bedrock", ImageUrl="https://tnstateparks.com/assets/images/content-images/campgrounds/249/colsp-area2-site73.jpg"},
            new Campsite {Id = 4, CampsiteTypeId = 4, Nickname = "Hammock Bay", ImageUrl="https://tnstateparks.com/assets/images/content-images/campgrounds/249/colsp-area2-site73.jpg"},
            new Campsite {Id = 5, CampsiteTypeId = 1, Nickname = "Pitched", ImageUrl="https://tnstateparks.com/assets/images/content-images/campgrounds/249/colsp-area2-site73.jpg"},
            new Campsite {Id = 6, CampsiteTypeId = 4, Nickname = "Hang 10", ImageUrl="https://tnstateparks.com/assets/images/content-images/campgrounds/249/colsp-area2-site73.jpg"}
        });
        modelBuilder.Entity<UserProfile>().HasData(new UserProfile[]
        {
            new UserProfile {Id = 1, FirstName = "John", LastName = "Doe", Email = "John.Doe@gmail.comx"}
        });
        modelBuilder.Entity<Reservation>().HasData(new Reservation[]
        {
            new Reservation {Id = 1, CampsiteId = 1, UserProfileId = 1, CheckinDate = new DateTime(2023, 09, 01, 16, 0, 0), CheckoutDate = new DateTime(2023, 09, 08, 11, 0, 0)}
        });
    }

    public CreekRiverDbContext(DbContextOptions<CreekRiverDbContext> context) : base(context)
    {

    }
}