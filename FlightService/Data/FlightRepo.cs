using FlightService.Data;
using FlightService.Data.Tables;

namespace FlightService.Repo
{
    public class FlightRepo : IFlightRepo
    {
        private readonly FlightContext _context;

        public FlightRepo(FlightContext context)
        {
            _context = context;
        }

        // public IEnumerable<FlightEntity> GetAvailableFlights(string departurePlace, string arrivalPlace, DateTime departureTime, DateTime arrivalTime, int freeSeats)
        // {
        //         return _context.Flights
        //         .Where(r => r.DeparturePlace == departurePlace && r.ArrivalPlace == arrivalPlace && r.DepartureTime >= departureTime && r.NumOfFreeSeats >= freeSeats) //event sourcing in here? 
        //         .Select(r => r)
        //         .ToList();
        // }

 public IEnumerable<FlightEntity> GetAvailableFlights(string departurePlace, string arrivalPlace, DateTime departureTime, DateTime arrivalTime, int freeSeats)
        {
            return _context.Flight
                .Where(r => r.DeparturePlace == departurePlace && r.ArrivalPlace == arrivalPlace && r.DepartureTime.Date == departureTime.Date)
                .Where(r => r.NumOfSeats - _context.FlightEvent.Where(e => e.FlightId == r.Id).Sum(e => e.ReservedSeats) >= freeSeats)  
                .ToList();
        }


       public void ReserveSeats(Guid Id, int seats)
        {
            var flight = _context.Flight.Find(Id);
            if (flight == null)
            {
                throw new Exception("Flight not found");
            }

            var currentFreeSeats = flight.NumOfSeats - _context.FlightEvent.Where(e => e.FlightId == Id).Sum(e => e.ReservedSeats); 
            if (currentFreeSeats < seats)
            {
                throw new Exception("Not enough free seats");
            }

            _context.FlightEvent.Add(new FlightSeatEvent
            {
                FlightId = Id, 
                ReservedSeats = seats
            });

            _context.SaveChanges();
        }

        public int GetNumOfFreeSeatsOfSpecificFlight(Guid flightId)
        {
            var flight = _context.Flight.Find(flightId);
            if (flight == null)
            {
                return 0;
            }

            var numOfFreeSeats = flight.NumOfSeats 
                - _context.FlightEvent.Where(e => e.FlightId == flightId).Sum(e => e.ReservedSeats); 
    
            return numOfFreeSeats;
        }

        public IEnumerable<FlightEntity> GetAllFlights()
        {
             return _context.Flight
                .Where(r => r.DeparturePlace!=null)
                .Select(r => r)
                .ToList();
            
        }

    }




}