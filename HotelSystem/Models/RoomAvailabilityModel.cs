using HotelSystem.Models;

public class RoomAvailability
{
    public int Id { get; set; } // Klucz główny
    public DateTime Date { get; set; }
    public bool Availability { get; set; }

    // Relacja do pokoju
    public int RoomId { get; set; }
    public Room Room { get; set; }
}
