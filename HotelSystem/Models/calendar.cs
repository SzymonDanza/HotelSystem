namespace HotelSystem.Models
{
    public class CalendarViewModel
    {
        public int RoomId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public List<dynamic> CalendarDays { get; set; }
    }
}
