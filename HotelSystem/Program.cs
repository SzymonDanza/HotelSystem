using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using HotelSystem.Data;
using HotelSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// Dodanie ApplicationDbContext do kontenera us³ug DI
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dodanie kontrolerów z widokami
builder.Services.AddControllersWithViews();

// Rejestracja HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Rejestracja sesji
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Sesja wygasa po 30 minutach
    options.Cookie.HttpOnly = true; // Zwiêkszone bezpieczeñstwo ciasteczek
    options.Cookie.IsEssential = true; // Ciasteczka s¹ wymagane dla dzia³ania sesji
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Œcie¿ka logowania
        options.AccessDeniedPath = "/Account/AccessDenied"; // Œcie¿ka odmowy dostêpu
        options.Cookie.Name = "HotelSystemAuth"; // Nazwa ciasteczka
    });

builder.Services.AddAuthorization();


// Budowa aplikacji
var app = builder.Build();

// Inicjalizacja bazy danych
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Inicjalizacja danych w bazie danych
    SeedData.Initialize(scope.ServiceProvider, dbContext);

    // Aktualizacja dostêpnoœci pokoi
    var availabilityService = new AvailabilityService(dbContext);
    availabilityService.UpdateRoomAvailability(DateTime.Now.Year);
}

// Obs³uga b³êdów w œrodowisku produkcyjnym
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware dla aplikacji
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Middleware sesji
app.UseAuthentication(); // Middleware uwierzytelniania
app.UseAuthorization(); // Middleware autoryzacji

// Konfiguracja routingu
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// Uruchomienie aplikacji
app.Run();
