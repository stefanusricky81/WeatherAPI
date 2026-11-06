using WeatherAPI.Domain.Entities;

namespace WeatherAPI.App.Abstractions
{
    public interface ICountryRepository
    {
        Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken cancellationToken);
        Task<bool> ExistsAsync(string countryCode, CancellationToken cancellationToken);
    }
}
