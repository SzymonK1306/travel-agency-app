using System.ComponentModel.DataAnnotations;
// using Shared.PaymentService.Events;

namespace Shared.Payment.Events
{
    public class ProcessPaymentFromCustomerEvent
    {
        public double Price {get; set;}
        public Guid Id {get; set;}
        public Guid CorrelationId {get; set;}

    }

}