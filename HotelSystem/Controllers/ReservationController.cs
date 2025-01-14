using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.Controllers
{
    public class ReservationController : Controller
    {
        // Akcja dla strony głównej
        public IActionResult Index()
        {
            ViewData["Title"] = "Witamy w Rezerwatorze";  // Tytuł strony
            return View("~/Views/Reservation/Index.cshtml");  // Zwróć widok 'Index'
        }

        // Akcja dla strony polityki prywatności
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
