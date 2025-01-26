using HotelSystem.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using HotelSystem.Models;  // Dodajemy przestrzeń nazw dla modelu Room
using System.Threading.Tasks;

public class RoomController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CurrencyService _currencyService;

    public RoomController(ApplicationDbContext context, CurrencyService currencyService)
    {
        _dbContext = context;
        _currencyService = currencyService;
    }

    // Metoda dla wyświetlenia pokoi
    public IActionResult Index()
    {
        var rooms = _dbContext.Rooms
                             .Select(r => new RoomViewModel
                             {
                                 Name = r.Name,
                                 Capacity = r.Capacity,
                                 PricePerNight = r.PricePerNight
                             })
                             .ToList();

        return View(rooms);
    }

    // Metoda przeliczająca cenę na USD
    [HttpGet]
    public async Task<IActionResult> GetPriceInUsd(decimal priceInPLN)
    {
        // Pobranie kursów walut z API
        var rates = await _currencyService.GetExchangeRatesAsync("PLN");

        if (rates != null && rates.Rates.ContainsKey("USD"))
        {
            // Obliczenie ceny w USD
            var priceInUsd = priceInPLN * rates.Rates["USD"];
            return Json(new { success = true, priceInUsd });
        }
        else
        {
            // Obliczenie ceny w USD
            var priceInUsd = priceInPLN * rates.Rates["USD"];
            return Json(new { success = true, priceInUsd });
        }


        return Json(new { success = false, message = "Nie znaleziono kursu USD." });
    }
}