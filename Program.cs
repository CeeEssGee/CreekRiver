using CreekRiver.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// First of 3 added statements: allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Second of 3 added statements: allows our api endpoints to access the database through Entity Framework Core
// The middle of these three statements is the most important. It makes an instance of the CreekRiverDbContext class available to our endpoints, so that they can query the database. builder.Configuration["CreekRiverDbConnectionString"] retrieves the connection string that we stored in the secrets manager so that EF Core can use it to connect to the database. Don't worry about what the others are doing for now.
builder.Services.AddNpgsql<CreekRiverDbContext>(builder.Configuration["CreekRiverDbConnectionString"]);

// Third of 3 added statements: Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// REMOVE ALL THE WEATHER FORECAST RELATED CODE
// var summaries = new[]
// {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };

// app.MapGet("/weatherforecast", () =>
// {
//     var forecast = Enumerable.Range(1, 5).Select(index =>
//         new WeatherForecast
//         (
//             DateTime.Now.AddDays(index),
//             Random.Shared.Next(-20, 55),
//             summaries[Random.Shared.Next(summaries.Length)]
//         ))
//         .ToArray();
//     return forecast;
// })
// .WithName("GetWeatherForecast");

// ENDPOINTS
// We provide the endpoint access to the DbContext for our database by adding another param to the handler. This is a rudimentary form of dependency injection, where the framework sees a dependency that the handler requires, and passes in an instance of it as an arg so that the handler can use it.
app.MapGet("/api/campsites", (CreekRiverDbContext db) =>
{
    // Linq methods can be chained to db.Campsites, like ToList. Underneath this seemingly simple line of code, EF Core is turning this method chain into a SQL query: SELECT Id, Nickname, ImageUrl, CampsiteTypeId FROM "Campsites";, and then turning the tabular data that comes back from the database into .NET objects! ASP.NET is serializing those .NET objects into JSON to send back to the client.
    return db.Campsites.ToList();
});

app.MapGet("/api/campsites/{id}", (CreekRiverDbContext db, int id) =>
{
    // Include is a method that will add related data to an entity. Because our campsite has a CampsiteType property where we can store that data, Include will add a JOIN in the underlying SQL query to the CampsiteTypes table. This is the same functionality that JSON Server provided with the _expand query string param.
    // Single is like First in that it looks for one matching item based on the expression, but unlike First will throw an Exception if it finds more than one that matches. For something like a primary key, where there is definitely only one row that should match the query, Single is a good way to express that.
    try
    {
        return Results.Ok(db.Campsites.Include(c => c.CampsiteType).Single(c => c.Id == id));
    }
    catch
    {
        return Results.NotFound();
    }
});

app.MapPost("/api/campsites", (CreekRiverDbContext db, Campsite campsite) =>
{
    db.Campsites.Add(campsite);
    // The SaveChanges method (inherited in CreekRiverDbContext from the DbContext class), actually sends the change to the database. Don't forget to call this method when you are making data changes! Note that it is possible to make multiple changes before calling SaveChanges, and the database will get all of those updates at once.
    db.SaveChanges();
    // The Results.Created methods creates a 201 response, indicating that a new campsite was created. The first argument in that method call is the URL where the new campsite can be accessed. Created will add a "location" header to the HTTP response that the client can use if it needs it. The second argument will be the body of the HTTP response (the new campsite). EF Core automatically adds the new Id to the campsite object.
    return Results.Created($"/api/campsites/{campsite.Id}", campsite);
});

app.MapDelete("/api/campsites/{id}", (CreekRiverDbContext db, int id) =>
{
    // There isn't much to add here, except that SingleOrDefault works, as you might expect, similarly to FirstOrDefault. The Remove method, followed by SaveChanges executes a SQL query to delete the campsite by Id.
    // You might be wondering what happens when you delete a campsite that has reservations associated with it. By default, EF Core will configure the related reservations rows to be deleted as well (this is the behavior that you are used to from JSON Server). This is called a cascade delete. If you want the database to do something other than cascade, you have to specify that in the configuration.
    Campsite campsite = db.Campsites.SingleOrDefault(campsite => campsite.Id == id);
    if (campsite == null)
    {
        return Results.NotFound();
    }
    db.Campsites.Remove(campsite);
    db.SaveChanges();
    return Results.NoContent();
});

app.MapPut("/api/campsites/{id}", (CreekRiverDbContext db, int id, Campsite campsite) =>
{
    // This endpoint handler finds the matching campsite, updates each of its properties with the data from the incoming campsite, saves that change to the database, and returns NoContent because there isn't any data other than a success message that the endpoint needs to return.
    Campsite campsiteToUpdate = db.Campsites.SingleOrDefault(campsite => campsite.Id == id);
    if (campsiteToUpdate == null)
    {
        return Results.NotFound();
    }
    campsiteToUpdate.Nickname = campsite.Nickname;
    campsiteToUpdate.CampsiteTypeId = campsite.CampsiteTypeId;
    campsiteToUpdate.ImageUrl = campsite.ImageUrl;

    db.SaveChanges();
    return Results.NoContent();
});

app.MapGet("/api/reservations", (CreekRiverDbContext db) =>
{
    // Here you can see that we need ThenInclude for CampsiteType because the JOIN for it is not on a Reservations column but a Campsites column, and ThenInclude must be chained right after the Include that JOINs Campsites, so that ThenInclude can determine which table it is joining from.
    // Try the endpoint out. You will notice that if a campsite has reservations associated with it, EF Core will also populate the reservation data for those campsites nested in the reservations property. This is ok, and is called "navigation property fix-up". For the most part, you can ignore this extra data, but occasionally you might find it useful.
    return db.Reservations
        .Include(r => r.UserProfile)
        .Include(r => r.Campsite)
        .ThenInclude(c => c.CampsiteType)
        .OrderBy(res => res.CheckinDate)
        .ToList();
});

app.MapPost("/api/reservations", (CreekRiverDbContext db, Reservation newRes) =>
{
    try
    {
        db.Reservations.Add(newRes);
        db.SaveChanges();
        // Check if reservation checkout is before or the same day as checkin
        if (newRes.CheckoutDate <= newRes.CheckinDate)
        {
            return Results.BadRequest("Reservation checkout must be at least one day after checkin");
        }
        // check if reservation is too long
        // Campsite campsite = db.Campsites.Include(c => c.CampsiteType).Single(c => c.Id == newRes.CampsiteId);
        // if (campsite != null && newRes.TotalNights > campsite.CampsiteType.MaxReservationDays)
        // {
        //     return Results.BadRequest("Reservation exceeds maximum reservation days for this campsite type");
        // }
        return Results.Created($"/api/reservations/{newRes.Id}", newRes);
    }
    catch
    {
        return Results.BadRequest("Invalid data submitted");
    }

});

app.Run();

// REMOVE ALL THE WEATHER FORECAST RELATED CODE
// record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }