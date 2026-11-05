using System;
using System.Collections.Generic;
using System.Text;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.App.Abstractions
{
    public interface IWeatherNoteRepository
    {
        Task AddAsync(WeatherNote note, CancellationToken cancellationToken);
    }
}
