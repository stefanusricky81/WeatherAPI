using System;
using System.Collections.Generic;
using System.Text;
using WeatherAPI.Domain.ValueObjects;

namespace WeatherAPI.Tests
{
    public class TemperatureTests
    {
        [Theory]
        [InlineData(32, 0)]           // freezing point of water
        [InlineData(212, 100)]        // boiling point of water
        [InlineData(-40, -40)]        // scales intersect
        [InlineData(-459.67, -273.15)] // absolute zero (boundary, must be allowed)
        public void ConvertsFahrenheitToCelsius(double fahrenheit, double expectedCelsius)
        {
            var celsius = Temperature.FahrenheitToCelsius(fahrenheit);

            Assert.Equal(expectedCelsius, celsius, precision: 2);
        }

        [Fact]
        public void BelowAbsoluteZeroThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Temperature.FahrenheitToCelsius(-460));
        }

        [Fact]
        public void DewPointAtFullHumidity()
        {
            var dewPoint = Temperature.DewPointFahrenheit(68, 100);

            Assert.Equal(68, dewPoint, precision: 2);
        }
    }
}
