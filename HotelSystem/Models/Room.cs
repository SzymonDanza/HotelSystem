using System.ComponentModel.DataAnnotations;

namespace HotelSystem.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa pokoju jest wymagana.")]
        [StringLength(100, ErrorMessage = "Nazwa pokoju nie może przekraczać 100 znaków.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Pojemność jest wymagana.")]
        [Range(1, 10, ErrorMessage = "Pojemność musi być w zakresie od 1 do 10.")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Cena za noc jest wymagana.")]
        [Range(0.01, 10000, ErrorMessage = "Cena musi być między 0.01 a 10,000.")]
        public decimal PricePerNight { get; set; }

        public ICollection<Reservation>? Reservations { get; set; }
    }
}
