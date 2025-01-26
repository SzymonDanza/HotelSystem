using HotelSystem.Data;
using HotelSystem.Services;
using Microsoft.AspNetCore.Mvc;

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
        var rates = await _currencyService.GetExchangeRatesAsync("USD");

        if (rates != null && rates.Rates.ContainsKey("USD"))
        {
            // Obliczenie ceny w USD
            var priceInUsd = priceInPLN / rates.Rates["USD"];
            return Json(new { success = true, priceInUsd });
        }

        return Json(new { success = false });
    }
}
