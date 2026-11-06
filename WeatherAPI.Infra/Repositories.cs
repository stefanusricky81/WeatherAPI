using Microsoft.EntityFrameworkCore;
using WeatherAPI.App.Abstractions;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Infra
{
    public class CountryRepository(AppDbContext db) : ICountryRepository
    {
        public async Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await db.Countries.AsNoTracking().OrderBy(c => c.Name).ToListAsync(cancellationToken);
        }

        public Task<bool> ExistsAsync(string countryCode, CancellationToken cancellationToken)
        {
            return db.Countries.AsNoTracking().AnyAsync(c => c.Code == countryCode, cancellationToken);
        }
    }

    public class CityRepository(AppDbContext db) : ICityRepository
    {
        public async Task<IReadOnlyList<City>> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken)
        {
            return await db.Cities.AsNoTracking().Where(c => c.CountryCode == countryCode).OrderBy(c => c.Name).ToListAsync(cancellationToken);
        }

        public Task<City?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return db.Cities.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public Task<City?> FindByNameAsync(string name, string? countryCode, CancellationToken cancellationToken)
        {
            var normalizedName = name.ToLower();

            var query = db.Cities.AsNoTracking().Where(c => c.Name.ToLower() == normalizedName);

            if (countryCode is not null)
                query = query.Where(c => c.CountryCode == countryCode);

            return query.OrderBy(c => c.Id).FirstOrDefaultAsync(cancellationToken);
        }
    }

    public class WeatherNoteRepository(AppDbContext db) : IWeatherNoteRepository
    {
        public async Task AddAsync(WeatherNote note, CancellationToken cancellationToken)
        {
            db.WeatherNotes.Add(note);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
