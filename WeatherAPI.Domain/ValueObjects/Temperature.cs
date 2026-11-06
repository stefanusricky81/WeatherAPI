namespace WeatherAPI.Domain.ValueObjects
{
    public static class Temperature
    {
        public const double AbsoluteZeroFahrenheit = -459.67;
        private const double Tolerance = 1e-9;

        private const double MagnusB = 17.62;
        private const double MagnusC = 243.12;

        public static double FahrenheitToCelsius(double fahrenheit)
        {
            CheckValidFahrenheit(fahrenheit);
            return (fahrenheit - 32d) * 5d / 9d;
        }

        public static double CelsiusToFahrenheit(double celsius) => celsius * 9d / 5d + 32d;

        public static double DewPointFahrenheit(double temperatureFahrenheit, double humidityPercent)
        {
            var tempC = FahrenheitToCelsius(temperatureFahrenheit);
            var humidity = Math.Clamp(humidityPercent, 1d, 100d);

            var gamma = Math.Log(humidity / 100d) + MagnusB * tempC / (MagnusC + tempC);
            var dewPointC = MagnusC * gamma / (MagnusB - gamma);

            return CelsiusToFahrenheit(dewPointC);
        }

        private static void CheckValidFahrenheit(double fahrenheit)
        {
            if (double.IsNaN(fahrenheit) || double.IsInfinity(fahrenheit))
                throw new ArgumentException("Temperature must be a finite number.", nameof(fahrenheit));

            if (fahrenheit < AbsoluteZeroFahrenheit - Tolerance)
                throw new ArgumentOutOfRangeException(nameof(fahrenheit),fahrenheit, $"Temperature cannot be below absolute zero ({AbsoluteZeroFahrenheit} °F).");
        }
    }
}
