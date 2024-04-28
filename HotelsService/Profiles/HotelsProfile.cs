using AutoMapper;
using HotelsService.Dtos;
using HotelsService.Models;

namespace HotelsService.Profiles
{
    public class HotelsProfile : Profile
    {
        public HotelsProfile()
        {
            CreateMap<Hotel, HotelReadDto>();
            CreateMap<HotelCreateDto, Hotel>();
        }
    }
}