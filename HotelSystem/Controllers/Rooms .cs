using HotelSystem.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using HotelSystem.Models;  
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

    
    
    [HttpGet]
    public async Task<IActionResult> GetPriceInUsd(decimal priceInPLN, string currency)
    {   
        if (string.IsNullOrEmpty(currency))
        {
            return Json(new { success = false, message = "Nie podano waluty." });
        }

       
        var rates = await _currencyService.GetExchangeRatesAsync("PLN");

        if (rates != null && rates.Rates.ContainsKey(currency))
        {
            
            var convertedPrice = priceInPLN * rates.Rates[currency];
            return Json(new { success = true, convertedPrice });
        }

        return Json(new { success = false, message = $"Nie znaleziono kursu dla waluty {currency}." });
    }
}