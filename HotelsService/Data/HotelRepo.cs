using HotelsService.Models;

namespace HotelsService.Data
{
    public class HotelRepo : IHotelRepo
    {
        private readonly AppDbContext _context;

        public HotelRepo(AppDbContext context)
        {
            _context = context;
        }

        public void CreateHotel(Hotel hotel)
        {
            if (hotel == null)
            {
                throw new ArgumentNullException(nameof(hotel));
            }
            _context.Hotels.Add(hotel);
        }

        public IEnumerable<Hotel> GetAllHotels()
        {
            return _context.Hotels.ToList();
        }

        public Hotel GetHotelById(int id)
        {
            return _context.Hotels.FirstOrDefault(p => p.Id == id);
        }

        public bool SaveChanges()
        {
            return (_context.SaveChanges() >= 0);
        }
    }
}