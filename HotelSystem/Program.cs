using Microsoft.EntityFrameworkCore;
using HotelSystem.Data;
using HotelSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// Dodanie ApplicationDbContext do kontenera us�ug
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dodanie kontroler�w z widokami
builder.Services.AddControllersWithViews();

// Rejestracja HttpContextAccessor (wa�ne: przed builder.Build())
builder.Services.AddHttpContextAccessor();

// Rejestracja sesji (je�li jest potrzebna w logowaniu)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Budowa aplikacji
var app = builder.Build();

// Inicjalizacja bazy danych
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

// Obs�uga b��d�w w �rodowisku produkcyjnym
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware dla aplikacji
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession(); // Middleware dla sesji
app.UseAuthorization();

// Konfiguracja routingu
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();