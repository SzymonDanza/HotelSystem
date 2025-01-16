namespace HotelSystem.Models
{
    public class ReservationViewModel
    {
        public DateTime SelectedDate { get; set; }
        public List<Room> AvailableRooms { get; set; }

        public DateTime EndDate { get; set; }
    }
}
