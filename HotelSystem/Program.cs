using Microsoft.EntityFrameworkCore;  
using HotelSystem.Data;

var builder = WebApplication.CreateBuilder(args);

// Dodaj ApplicationDbContext do kontenera us³ug DI
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));  // Po³¹czenie z SQL Server

// Dodaj kontrolery i widoki
builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews();

var app = builder.Build();


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
