using TripService.Models;

namespace TripService.Data
{
    public class TripRepo : ITripRepo
    {
        private readonly AppDbContext _context;

        public TripRepo(AppDbContext context)
        {
            _context = context;
        }

        public void CreateTrip(Trip offer)
        {
            if (offer == null) 
            {
                throw new ArgumentNullException(nameof(offer));
            }
            _context.Trip.Add(offer);
        }

        public IEnumerable<Trip> GetAllTrips()
        {
            return _context.Trip.ToList();
        }

        public IEnumerable<Trip> GetTripsByPreferences(string Destination, DateTime DepartureDate, 
                string DeparturePlace, int NumOfAdults, int NumOfKidsTo18, int NumOfKidsTo10, int NumOfKidsTo3)
        {
            DateTime utcDepartureDate = DepartureDate.ToUniversalTime();

            IQueryable<Trip> query = _context.Trip;
            query = query.Where(t => t.Country == Destination);
            query = query.Where(t => t.DepartureDate.Date == utcDepartureDate.Date);
            query = query.Where(t => t.DeparturePlace == DeparturePlace);
            query = query.Where(t => t.NumOfAdults == NumOfAdults);
            query = query.Where(t => t.NumOfKidsTo18 == NumOfKidsTo18);
            query = query.Where(t => t.NumOfKidsTo10 == NumOfKidsTo10);
            query = query.Where(t => t.NumOfKidsTo3 == NumOfKidsTo3);

            return query.ToList();
        }

        public List<Trip> GetTripById(string id)
        {
            Guid guid = Guid.Parse(id);
            return _context.Trip.Where(p => p.ClientId == guid).ToList();
        }

        public Trip GetTripByGuid(Guid guid)
        {
            var trip = _context.Trip.FirstOrDefault(t => t.Id == guid);
            if (trip == null)
            {
                throw new KeyNotFoundException($"Trip with ID {guid} not found.");
            }

            return trip;
        }

        public void SaveTrip(Trip Trip)
        {
            _context.Trip.Add(Trip);
            _context.SaveChanges();
        }

        public bool CheckAvailability(Guid TripId)
        {
            var trip = _context.Trip.FirstOrDefault(p => p.Id == TripId);
            if (trip == null)
            {
                throw new KeyNotFoundException($"Trip with ID {TripId} not found.");
            }

            bool result;
            string status = trip.Status;
            if (status == "Dostępna")
            {
                result = true;
            }
            else
            {
                result = false;
            }

            return result;
        }

        public void ChangeReservationStatus(Guid TripId, string newReservationStatus, Guid? UserId)
        {
            var trip = _context.Trip.FirstOrDefault(t => t.Id == TripId);
            if (trip != null)
            {
                trip.ClientId = UserId;
                trip.Status = newReservationStatus;
            }
            else
            {
                Console.WriteLine($"Trip with ID {TripId} not found.");
            }
            _context.SaveChanges();
        }

        public string GetMostPopularReservedDestination()
        {
            // Step 1: Retrieve all offers with the tag "reserved"
            var reservedOffers = _context.Trip
                .Where(o => o.Status == "Zarezerwowana")
                .ToList();

            // Step 2: Group by destination and count occurrences
            var destinationCount = reservedOffers
                .GroupBy(o => o.Country)
                .Select(g => new
                {
                    Destination = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Step 3: Determine the most popular destination
            var mostPopularDestination = destinationCount.FirstOrDefault();

            return mostPopularDestination?.Destination;
        }

        public IEnumerable<Trip> GetTripsBySpecificRoomConfiguration(Guid HotelId, int NumOfAdults, 
                int NumOfKidsTo18, int NumOfKidsTo10, int NumOfKidsTo3, DateTime ArrivalDate,
                DateTime ReturnDate, string RoomType)
        {
            IQueryable<Trip> query = _context.Trip.Where(t => 
                t.HotelId == HotelId 
                && t.NumOfAdults == NumOfAdults
                && t.NumOfKidsTo18 == NumOfKidsTo18
                && t.NumOfKidsTo10 == NumOfKidsTo10
                && t.NumOfKidsTo3 == NumOfKidsTo3
                && t.DepartureDate == ArrivalDate.ToUniversalTime()
                && t.ReturnDate == ReturnDate.ToUniversalTime()
                && t.RoomType == RoomType
            );

            return query.ToList();
        }

        public IEnumerable<Trip> GetTripsByFlightId(Guid? flightId)
        {
            IQueryable<Trip> query = _context.Trip.Where(p => 
                p.FlightId == flightId
            );
            
            return query.ToList();
        }
    }
}