using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text;
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
                e.Property(c => c.Code).HasMaxLength(2);
                e.Property(c => c.Name).HasMaxLength(100).IsRequired();
                e.HasData(SeedCountries);
            });

            modelBuilder.Entity<City>(e =>
            {
                e.HasKey(c => c.Id);
                e.Property(c => c.Id).ValueGeneratedNever();
                e.Property(c => c.Name).HasMaxLength(100).IsRequired();
                e.Property(c => c.CountryCode).HasMaxLength(2).IsRequired();
                e.HasOne<Country>().WithMany().HasForeignKey(c => c.CountryCode);
                e.HasIndex(c => new { c.CountryCode, c.Name }).IsUnique();
                e.HasData(SeedCities);
            });

            modelBuilder.Entity<WeatherNote>(e =>
            {
                e.HasKey(n => n.Id);
                e.Property(n => n.Text).HasMaxLength(WeatherNote.MaxTextLength).IsRequired();
                e.Property(n => n.SkyCondition).HasMaxLength(50).IsRequired();
                e.HasOne<City>().WithMany().HasForeignKey(n => n.CityId);
            });
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // SQLite does not store DateTimeKind; force every DateTime to be read back as UTC.
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
