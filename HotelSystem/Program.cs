using Microsoft.EntityFrameworkCore;
using HotelSystem.Data;
using HotelSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// Dodaj ApplicationDbContext do kontenera us�ug DI
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dodaj kontrolery i widoki
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Inicjalizacja danych w bazie
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Dodaj kod inicjalizuj�cy baz� danych, np. wype�nienie tabeli RoomAvailabilities
    SeedData.Initialize(scope.ServiceProvider, dbContext); // Wywo�anie metody inicjalizuj�cej

    // Wywo�anie metody aktualizuj�cej dost�pno�� pokoi na podstawie aktualnego roku
    var availabilityService = new AvailabilityService(dbContext);
    var currentYear = DateTime.Now.Year;

    // Aktualizacja dost�pno�ci dla bie��cego roku
    availabilityService.UpdateRoomAvailability(currentYear);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
