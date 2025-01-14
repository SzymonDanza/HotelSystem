using System;
using System.Collections.Generic;
namespace HotelSystem.Models;


public class RoomAvailabilityModel
{
    public int RoomId { get; set; }
    public string RoomName { get; set; }

    public Dictionary<DateTime, bool> Availability { get; set; } 
}
