using HotelSystem.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Konfiguracja DbContext z u�yciem connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dodanie us�ug kontroler�w i widok�w
builder.Services.AddControllersWithViews();

// Dodanie wsparcia dla pami�ci podr�cznej i sesji
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".HotelSystem.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Czas trwania sesji
    options.Cookie.HttpOnly = true; // Bezpiecze�stwo plik�w cookie
    options.Cookie.IsEssential = true; // Wymagane dla sesji
});

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Dodanie middleware dla sesji
app.UseSession();

// Dodanie w�asnej logiki autoryzacji
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    if (path.StartsWithSegments("/admin") && !(context.User.Identity?.IsAuthenticated ?? false))
    {
        context.Response.Redirect("/Login");
    }
    else
    {
        await next();
    }
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
