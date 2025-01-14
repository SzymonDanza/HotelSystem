namespace HotelSystem.Models
{
    public class RoomAvailability
    {
        public int RoomId { get; set; }
        public Room Room { get; set; }  

        public DateTime Date { get; set; }  

        public bool Availability { get; set; }  
    }
}
