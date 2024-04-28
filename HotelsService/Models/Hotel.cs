using System.ComponentModel.DataAnnotations;

namespace HotelsService.Models
{
    public class Hotel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string PriceRange { get; set; }
    }
}