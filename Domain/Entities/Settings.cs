namespace Domain.Entities;

public class Settings
{
    public IReadOnlyList<ApplicationInfo> ApplicationInfos { get; private set; } = new List<ApplicationInfo>();

    public void AddApplication(ApplicationInfo applicationInfo)
    {
        if (applicationInfo == null)
            throw new ArgumentNullException(nameof(applicationInfo));

        if (ApplicationInfos.Any(app => app.Name == applicationInfo.Name))
            throw new InvalidOperationException($"Приложение с именем {applicationInfo.Name} уже существует.");

        var updatedList = ApplicationInfos.ToList();
        updatedList.Add(applicationInfo);
        ApplicationInfos = updatedList.AsReadOnly();
    }

    public void SetApplications(List<ApplicationInfo> apps)
    {
        ApplicationInfos = apps.AsReadOnly();
    }
}