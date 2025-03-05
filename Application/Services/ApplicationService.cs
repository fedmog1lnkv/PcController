using Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Application.Services
{
    public class ApplicationService
    {
        private readonly string _fileDirectory = Path.Combine(Directory.GetCurrentDirectory(), "images");
        private readonly Settings _settings;
        private readonly IConfiguration _configuration;

        public ApplicationService(Settings settings, IConfiguration configuration)
        {
            _settings = settings;
            _configuration = configuration;

            var applicationsSection = configuration.GetSection("Applications");
            var applicationInfos = new List<ApplicationInfo>();

            foreach (var appSection in applicationsSection.GetChildren())
            {
                var appInfo = appSection.Get<ApplicationInfo>();

                appInfo.Name = appSection.Key;

                applicationInfos.Add(appInfo);
            }

            _settings.SetApplications(applicationInfos);

            ClearDirectory();
            ProcessFilesAsync().Wait();
        }

        // Получаем приложения
        public List<ApplicationInfo> GetApplications()
        {
            return _settings.ApplicationInfos.ToList();
        }

        public void ClearDirectory()
        {
            if (Directory.Exists(_fileDirectory))
            {
                foreach (var file in Directory.GetFiles(_fileDirectory))
                {
                    File.Delete(file);
                }
            }
            else
            {
                Directory.CreateDirectory(_fileDirectory);
            }
        }

        public async Task ProcessFilesAsync()
        {
            var applications = GetApplications();

            foreach (var app in applications)
            {
                var fileName = $"{app.Name}{Path.GetExtension(app.Photo)}";

                if (Uri.IsWellFormedUriString(app.Photo, UriKind.Absolute))
                {
                    await DownloadFileAsync(app.Photo, fileName);
                }
                else
                {
                    CopyFile(app.Photo, fileName);
                }

                app.Photo = $"/images/{fileName}";
            }
        }

        private async Task DownloadFileAsync(string url, string fileName)
        {
            var client = new HttpClient();
            var destinationPath = Path.Combine(_fileDirectory, fileName);

            using (var response = await client.GetAsync(url))
            {
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(destinationPath, FileMode.Create))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
        }

        private void CopyFile(string filePath, string fileName)
        {
            var destinationPath = Path.Combine(_fileDirectory, fileName);

            if (File.Exists(filePath))
            {
                File.Copy(filePath, destinationPath, true); // Копируем файл в целевую директорию
            }
        }
    }
}