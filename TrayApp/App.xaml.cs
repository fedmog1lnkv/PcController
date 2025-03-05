using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Forms;

namespace TrayApp
{
    public partial class App
    {
        private NotifyIcon _trayIcon;
        private IHost _webHost;
        private MainWindow _mainWindow;
        private IConfiguration _configuration;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _mainWindow = new MainWindow();

            // Настройка конфигурации
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())                           // Путь к текущему каталогу
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Чтение из appsettings.json
                .Build();

            // Настройка логирования
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .WriteTo.Console()
                .CreateLogger();

            _webHost = await StartApiServer();

            CreateTrayIcon();
        }

        private void CreateTrayIcon()
        {
            _trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Information,
                Visible = true,
                Text = "Media Controller"
            };

            _trayIcon.DoubleClick += (sender, e) => ShowMainWindow();

            _trayIcon.ContextMenuStrip = new ContextMenuStrip();
            _trayIcon.ContextMenuStrip.Items.Add("Exit", null, Exit);
        }

        private void ShowMainWindow()
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
        }

        private async Task<IHost> StartApiServer()
        {
            try
            {
                var builder = WebApplication.CreateBuilder();

                var hostingSettings = _configuration.GetSection("Hosting");
                var ipAddress = hostingSettings.GetValue<string>("IpAddress");

                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = "0.0.0.0";
                }

                var port = hostingSettings.GetValue<int>("Port");

                builder.WebHost.ConfigureKestrel(
                    options =>
                    {
                        options.Listen(IPAddress.Parse(ipAddress), port);
                    });

                // Регистрация сервисов
                builder.Services.AddSingleton<Settings>();
                builder.Services.AddSingleton<ApplicationService>();
                builder.Services.AddScoped<MediaService>();
                builder.Services.AddScoped<VolumeService>();
                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                // Добавление Cors
                builder.Services.AddCors(
                    options =>
                    {
                        options.AddPolicy(
                            "AllowAll",
                            policy =>
                            {
                                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                            });
                    });

                var webHost = builder.Build();

                var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "images");

                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                    Log.Information($"Папка '{imagesPath}' была успешно создана.");
                }
                else
                {
                    Log.Information($"Папка '{imagesPath}' уже существует.");
                }

                webHost.UseStaticFiles(
                    new StaticFileOptions
                    {
                        FileProvider =
                            new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "images")),
                        RequestPath = "/images"
                    });

                // Использование Swagger в режиме разработки
                // if (webHost.Environment.IsDevelopment())
                // {
                webHost.UseSwagger();
                webHost.UseSwaggerUI();
                // }

                webHost.UseHttpsRedirection();
                webHost.UseAuthorization();

                // Добавление маршрутов для API
                webHost.MapControllers();

                // Добавление Cors
                webHost.UseCors("AllowAll");

                // Запуск приложения
                await webHost.StartAsync();

                return webHost;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "API server failed to start.");
                return null;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private async void Exit(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;

            if (_webHost != null)
            {
                await _webHost.StopAsync(TimeSpan.FromSeconds(5));
            }

            Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayIcon?.Dispose();
            base.OnExit(e);
        }
    }
}