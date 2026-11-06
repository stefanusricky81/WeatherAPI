using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using WeatherAPI.App.Abstractions;

namespace WeatherAPI.Infra
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("WeatherDb") ?? "Data Source=weather.db";
            //var connectionString = configuration.GetConnectionString("WeatherDb");
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<ICityRepository, CityRepository>();
            services.AddScoped<IWeatherNoteRepository, WeatherNoteRepository>();

            services.AddOptions<OpenWeatherMapOptions>().Bind(configuration.GetSection(OpenWeatherMapOptions.SectionName));

            services.AddHttpClient<IWeatherService, OpenWeatherMapService>((serviceProvider, client) =>
            {
                var owm = serviceProvider.GetRequiredService<IOptions<OpenWeatherMapOptions>>().Value;
                client.BaseAddress = new Uri(owm.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(10);
            });

            return services;
        }

        public static async Task InitializeDatabaseAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();
        }
    }
}
