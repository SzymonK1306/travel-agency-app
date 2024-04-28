using OffersServise.Models; 

namespace OffersService.Messaging {
    public interface IMessageBusClient {
        void SendOffer(Offer offer);
    }
}