using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Infra
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Country> Countries => Set<Country>();
        public DbSet<City> Cities => Set<City>();
        public DbSet<WeatherNote> WeatherNotes => Set<WeatherNote>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Country>(e =>
            {
                e.HasKey(c => c.Code);
                e.HasData(SeedCountries);
            });

            modelBuilder.Entity<City>(e =>
            {
                e.HasOne<Country>().WithMany().HasForeignKey(c => c.CountryCode);
                e.HasData(SeedCities);
            });

            modelBuilder.Entity<WeatherNote>(e =>
            {
                e.HasOne<City>().WithMany().HasForeignKey(n => n.CityId);
            });
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        }

        private static readonly Country[] SeedCountries =
        [
            new("AU", "Australia"),
            new("GB", "United Kingdom"),
            new("ID", "Indonesia"),
            new("JP", "Japan"),
            new("NZ", "New Zealand"),
            new("SG", "Singapore"),
            new("US", "United States")
        ];

        private static readonly City[] SeedCities =
        [
            new(1, "Jakarta", "ID"),
            new(2, "Surabaya", "ID"),
            new(3, "Bandung", "ID"),
            new(4, "Denpasar", "ID"),
            new(5, "Sydney", "AU"),
            new(6, "Melbourne", "AU"),
            new(7, "Brisbane", "AU"),
            new(8, "Perth", "AU"),
            new(9, "Auckland", "NZ"),
            new(10, "Wellington", "NZ"),
            new(11, "Christchurch", "NZ"),
            new(12, "Singapore", "SG"),
            new(13, "London", "GB"),
            new(14, "Manchester", "GB"),
            new(15, "Perth", "GB"),
            new(16, "New York", "US"),
            new(17, "Los Angeles", "US"),
            new(18, "Chicago", "US"),
            new(19, "Tokyo", "JP"),
            new(20, "Osaka", "JP")
        ];
    }

    internal class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter() : base( v => v.Kind == DateTimeKind.Local ? v.ToUniversalTime() : DateTime.SpecifyKind(v, DateTimeKind.Utc),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
        {
        }
    }
}
