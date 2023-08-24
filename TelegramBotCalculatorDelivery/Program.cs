using System.Diagnostics;
using TelegramBotCalculatorDelivery;
using TelegramBotCalculatorDelivery.Service;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(opt => opt.ListenLocalhost(7000));
Console.WriteLine("asdasdasd 1");
Debug.WriteLine("asdasdasd 1");
// Add services to the container.
builder.Services.AddSingleton<TelegramBot>();
builder.Services.AddScoped<ITelegramBotService, TelegramBotService>();
builder.Services.AddControllers();
var app = builder.Build();
Debug.WriteLine("asdasdasd 2");
Console.WriteLine("asdasdasd 2");
// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
Debug.WriteLine("GET service 2");
Console.WriteLine("GET service 2");
app.Services.GetRequiredService<TelegramBot>().GetBot().Wait();

app.UseRouting();
app.UseEndpoints(ends => ends.MapControllers());

app.Run();

