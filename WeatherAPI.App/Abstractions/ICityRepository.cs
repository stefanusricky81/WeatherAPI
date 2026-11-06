using WeatherAPI.Domain.Entities;

namespace WeatherAPI.App.Abstractions
{
    public interface ICityRepository
    {
        Task<IReadOnlyList<City>> GetByCountryCodeAsync(string countryCode, CancellationToken cancellationToken);

        Task<City?> GetByIdAsync(int id, CancellationToken cancellationToken);

        Task<City?> FindByNameAsync(string name, string? countryCode, CancellationToken cancellationToken);
    }
}
