using Api.Hubs;
using Application.Configurations;
using Domain.Entities;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Serilog.Events;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

var hostingSettings = builder.Configuration.GetSection("Hosting");
var ipAddress = hostingSettings.GetValue<string>("IpAddress");
var port = hostingSettings.GetValue<int>("Port");

builder.WebHost.ConfigureKestrel(
    options =>
    {
        options.Listen(IPAddress.Parse(ipAddress!), port);
    });

builder.Services.AddSingleton<Settings>();

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplication();

#region Logging

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .WriteTo.Console()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

#endregion

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            "AllowAll",
            builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
    });

builder.Services.AddSignalR();

var app = builder.Build();

app.MapHub<VolumeHub>("/volumeHub");

app.UseStaticFiles(
    new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "images")),
        RequestPath = "/images"
    });

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

app.UseCors("AllowAll");

app.Run();