namespace HotelSystem.Models
{
    public class ReservationViewModel
    {
        public DateTime SelectedDate { get; set; } // Wybrana data rezerwacji
        public RoomAvailability RoomAvailability { get; set; } // Informacje o dostępności pokoju
        public List<Room> AvailableRooms { get; set; } // Lista dostępnych pokoi

        // Dodatkowe właściwości, jeśli są wymagane
    }
}
