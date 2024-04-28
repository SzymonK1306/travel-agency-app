using Microsoft.EntityFrameworkCore;
using HotelsService.Models;

namespace HotelsService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }

        public DbSet<Hotel> Hotels { get; set; }
    }
}