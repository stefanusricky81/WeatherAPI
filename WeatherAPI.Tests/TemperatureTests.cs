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
        [InlineData(0, -17.78)]       // negative Celsius from positive-zero Fahrenheit
        [InlineData(-4, -20)]         // negative Fahrenheit
        [InlineData(98.6, 37)]        // body temperature
        [InlineData(-459.67, -273.15)] // absolute zero (boundary, must be allowed)

        public void FahrenheitToCelsius_ReturnsExpectedValue(double fahrenheit, double expectedCelsius)
        {
            var celsius = Temperature.FahrenheitToCelsius(fahrenheit);

            Assert.Equal(expectedCelsius, celsius, precision: 2);
        }

        [Fact]
        public void FahrenheitToCelsius_BelowAbsoluteZero_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Temperature.FahrenheitToCelsius(-460));
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void FahrenheitToCelsius_NonFiniteValue_Throws(double fahrenheit)
        {
            Assert.Throws<ArgumentException>(() => Temperature.FahrenheitToCelsius(fahrenheit));
        }

        [Theory]
        [InlineData(0, 32)]
        [InlineData(100, 212)]
        [InlineData(-40, -40)]
        public void CelsiusToFahrenheit_ReturnsExpectedValue(double celsius, double expectedFahrenheit)
        {
            var fahrenheit = Temperature.CelsiusToFahrenheit(celsius);

            Assert.Equal(expectedFahrenheit, fahrenheit, precision: 2);
        }

        [Fact]
        public void DewPoint_At100PercentHumidity_EqualsAirTemperature()
        {
            var dewPoint = Temperature.DewPointFahrenheit(68, 100);

            Assert.Equal(68, dewPoint, precision: 2);
        }

        [Fact]
        public void DewPoint_KnownValue_IsWithinTolerance()
        {
            // 77 °F (25 °C) at 60 % RH → dew point ≈ 16.7 °C ≈ 62.0 °F
            var dewPoint = Temperature.DewPointFahrenheit(77, 60);

            Assert.InRange(dewPoint, 61.8, 62.3);
        }

        [Fact]
        public void DewPoint_ZeroHumidity_DoesNotThrowAndIsFinite()
        {
            var dewPoint = Temperature.DewPointFahrenheit(77, 0);

            Assert.True(double.IsFinite(dewPoint));
            Assert.True(dewPoint < 77);
        }
    }
}
