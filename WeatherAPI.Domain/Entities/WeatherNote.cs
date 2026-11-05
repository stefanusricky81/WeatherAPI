using System;
using System.Collections.Generic;
using System.Text;
using WeatherAPI.Domain.ValueObjects;

namespace WeatherAPI.Domain.Entities
{
    public class WeatherNote
    {
        public const int MaxTextLength = 500;
        public int Id { get; private set; }
        public int CityId { get; private set; }
        public string Text { get; private set; }
        public double Fahrenheit { get; private set; }
        public string SkyCondition { get; private set; }
        public DateTime ObservedAtUtc { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }

        public WeatherNote(int cityId, string text,double fahrenheit, string skyCondition,DateTime observedAtUtc,DateTime createdAtUtc)
        {
            if (cityId <= 0)
                throw new ArgumentOutOfRangeException(nameof(cityId), cityId, "City id must be positive.");

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Note text is required.", nameof(text));

            if (text.Trim().Length > MaxTextLength)
                throw new ArgumentException($"Note text cannot exceed {MaxTextLength} characters.", nameof(text));

            if (string.IsNullOrWhiteSpace(skyCondition))
                throw new ArgumentException("Sky condition is required.", nameof(skyCondition));

            CityId = cityId;
            Text = text.Trim();
            Fahrenheit = fahrenheit;
            SkyCondition = skyCondition.Trim();
            ObservedAtUtc = observedAtUtc;
            CreatedAtUtc = createdAtUtc;
        }
    }
}
