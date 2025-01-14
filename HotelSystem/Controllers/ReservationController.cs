using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.Controllers
{
    public class ReservationController : Controller
    {
       
        public IActionResult Index()
        {
            ViewData["Title"] = "Witamy w Rezerwatorze";  
            return View("~/Views/Reservation/Index.cshtml");  
        }

        
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
