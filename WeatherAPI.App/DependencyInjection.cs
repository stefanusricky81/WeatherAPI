using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace WeatherAPI.App
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApp(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection).Assembly;

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            services.AddValidatorsFromAssembly(assembly);

            services.AddSingleton(TimeProvider.System);

            return services;
        }
    }
}
