using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherAPI.Domain.Entities
{
    public class City
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string CountryCode { get; private set; }
        public City(int id, string name, string countryCode)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), id, "City id must be positive.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("City name is required.", nameof(name));

            if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Trim().Length != 2)
                throw new ArgumentException("Country code must be a 2-letter ISO code.", nameof(countryCode));

            Id = id;
            Name = name.Trim();
            CountryCode = countryCode.Trim().ToUpperInvariant();
        }
    }
}
