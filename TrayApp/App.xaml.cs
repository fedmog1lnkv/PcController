using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Forms;
using TrayApp.Hubs;

namespace TrayApp
{
    public partial class App
    {
        private NotifyIcon _trayIcon;
        private IHost _webHost;
        private MainWindow _mainWindow;
        private IConfiguration _configuration;
        private BackgroundWorker _backgroundWorker;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _mainWindow = new MainWindow();

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

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
                var ipAddress = hostingSettings.GetValue<string>("IpAddress") ?? "0.0.0.0";
                var port = hostingSettings.GetValue<int>("Port");

                builder.WebHost.ConfigureKestrel(
                    options =>
                    {
                        options.Listen(IPAddress.Parse(ipAddress), port);
                    });

                var startup = new Startup();
                startup.ConfigureServices(builder.Services);

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

                webHost.UseStaticFiles(
                    new StaticFileOptions
                    {
                        FileProvider =
                            new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "images")),
                        RequestPath = "/images"
                    });

                /*if (webHost.Environment.IsDevelopment())
                {*/
                    webHost.UseSwagger();
                    webHost.UseSwaggerUI();
                // }

                webHost.UseHttpsRedirection();
                webHost.UseAuthorization();
                webHost.UseRouting();

                webHost.MapControllers();
                webHost.MapHub<MainHub>("/hub");

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

        private new async void Exit(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;

            await _webHost.StopAsync(TimeSpan.FromSeconds(5));

            Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayIcon?.Dispose();
            base.OnExit(e);
        }
    }
}