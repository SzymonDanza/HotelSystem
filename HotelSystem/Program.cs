using HotelSystem.Data;
using HotelSystem.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Dodanie ApplicationDbContext do kontenera us�ug DI
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dodanie us�ugi CurrencyService z HttpClient
builder.Services.AddHttpClient<CurrencyService>(); // Dodanie HttpClient do CurrencyService
builder.Services.AddScoped<CurrencyService>();

// Dodanie kontroler�w z widokami
builder.Services.AddControllersWithViews();

// Rejestracja HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Rejestracja sesji
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Konfiguracja uwierzytelniania i autoryzacji
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "HotelSystemAuth";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Inicjalizacja danych (je�li wymagane)
    SeedData.Initialize(scope.ServiceProvider, dbContext);

    // Aktualizacja dost�pno�ci pokoi (je�li wymagane)
    var availabilityService = new AvailabilityService(dbContext);
    availabilityService.UpdateRoomAvailability(DateTime.Now.Year);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Rejestracja sesji, uwierzytelniania i autoryzacji
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Konfiguracja routingu
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
