namespace WeatherAPI.Domain.Entities
{
    public class Country
    {
        public string Code { get; private set; }
        public string Name { get; private set; }

        public Country(string code, string name)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Trim().Length != 2)
                throw new ArgumentException("Country code must be a 2 letter.", nameof(code));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Country name is required.", nameof(name));

            Code = code.Trim().ToUpperInvariant();
            Name = name.Trim();
        }
    }
}
