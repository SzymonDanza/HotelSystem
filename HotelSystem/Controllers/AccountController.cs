using HotelSystem.Data;
using HotelSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public AccountController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user != null)
            {
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Nieprawidłowa nazwa użytkownika lub hasło.";
            return View();
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (_dbContext.Users.Any(u => u.Username == user.Username))
            {
                ViewBag.Error = "Użytkownik o tej nazwie już istnieje.";
                return View();
            }

            user.IsAdmin = false; // Domyślnie nowy użytkownik nie jest adminem
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}