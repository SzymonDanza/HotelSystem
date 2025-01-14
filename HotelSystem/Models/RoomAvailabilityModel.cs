using System;
using System.Collections.Generic;

public class RoomAvailabilityModel
{
    public int RoomId { get; set; }
    public string RoomName { get; set; }
    public Dictionary<DateTime, bool> Availability { get; set; } // Dla każdego dnia informacja, czy pokój jest dostępny
}
