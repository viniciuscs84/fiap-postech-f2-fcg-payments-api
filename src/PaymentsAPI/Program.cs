using PaymentsAPI.Messaging;
using PaymentsAPI.Payments;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var rabbitMqOptions = builder.Configuration
    .GetSection(RabbitMqOptions.SectionName)
    .Get<RabbitMqOptions>() ?? new RabbitMqOptions();
var paymentOptions = builder.Configuration
    .GetSection(PaymentSimulationOptions.SectionName)
    .Get<PaymentSimulationOptions>() ?? new PaymentSimulationOptions();

builder.Services.AddSingleton(rabbitMqOptions);
builder.Services.AddSingleton(paymentOptions);
builder.Services.AddSingleton<IPaymentResultPublisher, RabbitMqPaymentResultPublisher>();
builder.Services.AddSingleton<IPaymentProcessor, PaymentProcessor>();
builder.Services.AddHostedService<OrderPlacedConsumer>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "PaymentsAPI" }));

app.Run();

public partial class Program { }
