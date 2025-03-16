using Application;
using indexer.dto;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Configuration.AddJsonFile("appsettings.json");
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddSingleton<IMessageConsumer, RabbitMqConsumer>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

builder.Services.AddSingleton<IMessageProcessor, MessageProcessor>();
builder.Services.AddHostedService<RabbitMqBackgroundService>();

var app = builder.Build();
app.Run();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

var publisher = services.GetRequiredService<IMessagePublisher>();
var processor = services.GetRequiredService<IMessageProcessor>();
var consumer = services.GetRequiredService<IMessageConsumer>();

var testMessage = new MessageDto<EmailDto>
{
    Id = Guid.NewGuid(),
    Timestamp = DateTime.UtcNow,
    Payload = new EmailDto { Subject = "Test Email", Body = "Hello, this is a test email!" }
};