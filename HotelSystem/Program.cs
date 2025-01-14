using Microsoft.EntityFrameworkCore;
using HotelSystem.Data;

var builder = WebApplication.CreateBuilder(args);

// Dodaj ApplicationDbContext do kontenera us³ug DI
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dodaj kontrolery i widoki
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Inicjalizacja danych w bazie
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    SeedData.Initialize(scope.ServiceProvider, dbContext);  // Wywo³anie metody inicjalizuj¹cej
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
