using HotelsService.Models;

namespace HotelsService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app)
        {
            using(var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>());
            }
        }

        private static void SeedData(AppDbContext context)
        {
            if(!context.Hotels.Any())
            {
                Console.WriteLine("--> Seeding data");

                context.Hotels.AddRange(
                    new Hotel() {Name="Holiday Inn", City="New York", PriceRange="$200-$300"},
                    new Hotel() {Name="Marriott", City="Orlando", PriceRange="$150-$250"}
                );

                context.SaveChanges();
            }
            else 
            {
                Console.WriteLine("--> We already have data");
            }
        }
    }
}