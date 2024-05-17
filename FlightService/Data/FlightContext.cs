using Microsoft.EntityFrameworkCore;
using FlightService.Data.Tables;

namespace FlightService.Data
{
    public class FlightContext : DbContext
    {
        public FlightContext(DbContextOptions<FlightContext> options) : base(options) {}

        public FlightContext() {}

        public DbSet<FlightEntity> Flight {get;set;}
        public DbSet<FlightSeatEvent> FlightEvent { get; set; }
    }
}