using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using HotelService.Data;
using HotelService.Models;
using HotelService.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddMassTransit(x => {
    x.AddConsumer<AvailableRoomsConsumer>();
    x.UsingRabbitMq((context, cfg) => {
        cfg.Host("localhost", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ReceiveEndpoint("hotel-queue", e => {
            e.ConfigureConsumer<AvailableRoomsConsumer>(context);
        });
    });
});

builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("HotelDbConnection")));
builder.Services.AddScoped<IHotelRepo, HotelRepo>();

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    LoadData(app);
}

app.UseRouting();

app.MapControllers();

app.Run();

PrintAllData(app.Services);

void LoadData(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
        dbContext.Database.EnsureCreated();

        var jsonString = File.ReadAllText("./Data/Db/hotels.json");
        using var jsonDoc = JsonDocument.Parse(jsonString);

        var hotelsArray = jsonDoc.RootElement.EnumerateArray();
        List<Hotel> hotels = new List<Hotel>();

        while (hotelsArray.MoveNext())
        {
            var hotelElement = hotelsArray.Current;
            var hotel = new Hotel
            {
                Id = Guid.NewGuid(),
                Name = hotelElement.GetProperty("name").GetString() ?? "Unknown",
                Country = hotelElement.GetProperty("Country").GetString() ?? "Unknown",
                City = hotelElement.GetProperty("city").GetString() ?? "Unknown",
                AirportName = hotelElement.GetProperty("AirportName").GetString() ?? "Unknown",
                Rooms = new List<Room>()
            };
            
            var roomsArray = hotelElement.GetProperty("Rooms").EnumerateArray();
            while (roomsArray.MoveNext())
            {
                var roomElement = roomsArray.Current;
                var room = new Room
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotel.Id,
                    NumOfPeople = roomElement.GetProperty("capacity").GetInt32(),
                    RoomType = roomElement.GetProperty("name").GetString() ?? "Unknown",
                    Features = roomElement.GetProperty("Features").GetString() ?? "Unknown",
                    RoomEvents = new List<RoomEvent>()
                };

                // Example room event if necessary
                // var roomEvent = new RoomEvent
                // {
                //     Id = Guid.NewGuid(),
                //     RoomId = room.Id,
                //     Status = "Available",
                //     StartDate = DateTime.Now, 
                //     EndDate = DateTime.Now.AddDays(1) 
                // };
                // room.RoomEvents.Add(roomEvent);

                hotel.Rooms.Add(room);
            }

            hotels.Add(hotel);
        }

        dbContext.Hotels.AddRange(hotels);
        dbContext.SaveChanges();
    }
}

void PrintAllData(IServiceProvider serviceProvider)
{
    using (var scope = serviceProvider.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<HotelDbContext>();

        var hotels = dbContext.Hotels.Include(h => h.Rooms).ThenInclude(r => r.RoomEvents).ToList();

        foreach (var hotel in hotels)
        {
            Console.WriteLine($"Hotel ID: {hotel.Id}, Name: {hotel.Name}, Country: {hotel.Country}, City: {hotel.City}, AirportName: {hotel.AirportName}");

            foreach (var room in hotel.Rooms)
            {
                Console.WriteLine($"\tRoom ID: {room.Id}, NumOfPeople: {room.NumOfPeople}, RoomType: {room.RoomType}, Features: {room.Features}");

                foreach (var roomEvent in room.RoomEvents)
                {
                    Console.WriteLine($"\t\tRoomEvent ID: {roomEvent.Id}, Status: {roomEvent.Status}, StartDate: {roomEvent.StartDate}, EndDate: {roomEvent.EndDate}");
                }
            }
        }
    }
}