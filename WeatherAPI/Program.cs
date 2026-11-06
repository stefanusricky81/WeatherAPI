using WeatherAPI;
using WeatherAPI.App;
using WeatherAPI.Infra;

var builder = WebApplication.CreateBuilder(args);

// Layers
builder.Services.AddApp();
builder.Services.AddInfra(builder.Configuration);

// API
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Error handling
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

await app.Services.InitializeDatabaseAsync();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
// Front-end (wwwroot/index.html)
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();

public partial class Program { }