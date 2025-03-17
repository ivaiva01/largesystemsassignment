using System.Text.Json.Serialization;
using infrastructure;
using service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Add services to the container.
// The project can access this from everywhere.
builder.Services.AddSingleton<SearchingService>();
builder.Services.AddSingleton<SearchingRepo>();

builder.Services.AddSingleton<HttpClient>();


builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();


// For allowing cross-site scripting and allowing the API to talk with frontend
var allowedOrigins = new[]
{
    "http://localhost:4200/overview",
    "http://localhost:4200",
    "http://localhost:5052/swagger/index.html",
    "http://localhost:5052/"
};
app.UseCors(options =>
{
    options.SetIsOriginAllowed(origin => allowedOrigins.Contains(origin))
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
});

app.UseHttpsRedirection();
//app.UseAuthorization();
app.MapControllers();
app.Run();