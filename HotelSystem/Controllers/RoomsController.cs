using Microsoft.AspNetCore.Mvc;
using HotelSystem.Data;
using HotelSystem.Models;

public class RoomsController : Controller
{
    private readonly ApplicationDbContext _context;

    public RoomsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var rooms = _context.Rooms.ToList();
        return View(rooms);
    }
}
