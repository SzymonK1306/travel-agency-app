using HotelsService.Models;

namespace HotelsService.Data
{
    public interface IHotelRepo
    {
        bool SaveChanges();

        IEnumerable<Hotel> GetAllHotels();
        Hotel GetHotelById(int id);
        void CreateHotel(Hotel hotel);
    }
}