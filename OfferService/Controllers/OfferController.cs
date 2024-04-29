using Microsoft.AspNetCore.Mvc;
using OfferService.Models;
using OfferService.Services;
using System;

namespace OfferService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OfferController : ControllerBase
    {
        private readonly IOfferService _offerService;

        public OfferController(IOfferService offerService) => _offerService = offerService;

        [HttpPost]
        public ActionResult CreateOffer([FromBody] Offer offer)
        {
            _offerService.CreateOffer(offer);
            return Ok();
        }
    }
}