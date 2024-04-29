using MassTransit;
using CommonMessages.Events;
using OfferService.Models;
using System;

namespace OfferService.Services
{
    public class OfferService : IOfferService
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public OfferService(IPublishEndpoint publishEndpoint) => _publishEndpoint = publishEndpoint;

        public void CreateOffer(Offer offer)
        {
            // Symulacja zapisu do bazy
            Console.WriteLine($"Saving offer: {offer.Title}");

            // Publikacja zdarzenia
            _publishEndpoint.Publish(new OfferCreated { OfferId = offer.Id, Title = offer.Title });
        }
    }
}