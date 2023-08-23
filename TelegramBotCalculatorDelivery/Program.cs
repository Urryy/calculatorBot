using TelegramBotCalculatorDelivery;
using TelegramBotCalculatorDelivery.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<TelegramBot>();
builder.Services.AddScoped<ITelegramBotService, TelegramBotService>();
builder.Services.AddControllers();
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.Services.GetRequiredService<TelegramBot>().GetBot().Wait();

app.UseRouting();
app.UseEndpoints(ends => ends.MapControllers());

app.Run();

