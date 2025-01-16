using HotelSystem.Data;
using Microsoft.AspNetCore.Mvc;

public class RoomController : Controller
{
    private readonly ApplicationDbContext DbContext;

    public RoomController(ApplicationDbContext context)
    {
        DbContext = context;
    }

    public IActionResult Index()
    {
        
        var rooms = DbContext.Rooms
                             .Select(r => new RoomViewModel
                             {
                                 Name = r.Name,
                                 Capacity = r.Capacity,
                                 PricePerNight = r.PricePerNight
                             })
                             .ToList();

        return View(rooms);
    }
}
