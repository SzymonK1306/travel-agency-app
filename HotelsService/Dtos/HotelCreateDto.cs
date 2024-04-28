using System.ComponentModel.DataAnnotations;

namespace HotelsService.Dtos
{
    public class HotelCreateDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string PriceRange { get; set; }
    }
}