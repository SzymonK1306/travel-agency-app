using AutoMapper;
using OffersServise.Dtos;
using OffersServise.Models;

namespace OffersServise.Profiles {
    public class OffersProfile : Profile {
        public OffersProfile() {
            CreateMap<Offer, OfferReadDto>();
            CreateMap<OfferCreateDto, Offer>();
        }
    }
}