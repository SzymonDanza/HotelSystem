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
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true; 
    options.Cookie.IsEssential = true; 
});

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

    
    SeedData.Initialize(scope.ServiceProvider, dbContext);

    
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

app.UseSession(); 
app.UseAuthentication(); 
app.UseAuthorization(); 


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
