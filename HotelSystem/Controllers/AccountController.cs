﻿using System.Security.Claims;
using HotelSystem.Data;
using HotelSystem.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());

                // Logowanie uzytkownika
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
        };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

               
                if (user.IsAdmin)
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Nieprawidłowa nazwa użytkownika lub hasło.");
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

            user.IsAdmin = false; 
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            
            HttpContext.Session.Clear();

            
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            
            return RedirectToAction("Login");
        }
    }
}
