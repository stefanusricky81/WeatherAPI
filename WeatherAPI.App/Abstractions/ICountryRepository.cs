using System;
using System.Collections.Generic;
using System.Text;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.App.Abstractions
{
    public interface ICountryRepository
    {
        Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken cancellationToken);
        Task<bool> ExistsAsync(string countryCode, CancellationToken cancellationToken);
    }
}
