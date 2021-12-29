using Octokit;
using System.Text;
using static RhinoPackager.Util;

namespace RhinoPackager;

class Commands
{
    public static int Test()
    {        
        var project = Settings.Instance.TestProject;

        if (string.IsNullOrEmpty(project))
        {
            Log("No test project.");
            return 0;
        }

        return Run("dotnet", $"test {project}");
    }

    public static async Task<int> CheckVersionAsync()
    {
        var settings = Settings.Instance;
        string version = Manifest.GetVersion();

        var client = new GitHubClient(new ProductHeaderValue(settings.GithubOwner))
        {
            Credentials = new Credentials(GetSecret("GITHUB_TOKEN"))
        };

        var latest = await client.Repository.Release.GetLatest(settings.GithubOwner, settings.GithubRepo);

        if (latest.TagName == version)
        {
            Log($"Version number {version} not updated, nothing else to do.");
            return -1;
        }

        return 0;
    }

    public static int Build()
    {
        var project = Settings.Instance.BuildProject;
        return Run("dotnet", $"build {project} -c Release");
    }

    public static async Task<int> PackageAsync()
    {
        var settings = Settings.Instance;
        var packageFolder = settings.PackageFolder;

        // Package folder
        if (Directory.Exists(packageFolder))
            Directory.Delete(packageFolder, true);

        Directory.CreateDirectory(packageFolder);

        // Copy bin files
        var buildFolder = settings.BuildFolder;
        var files = settings.PackageFiles;

        foreach (var file in files)
        {
            var source = Path.Combine(buildFolder, file);
            var destination = Path.Combine(packageFolder, file);
            File.Copy(source, destination, true);
        }

        // Create manifest
        var path = Path.Combine(packageFolder, "manifest.yml");
        Manifest.CreateAndSave(path);

        // Build package
        string yak = await GetYakPathAsync();
        return Run(yak, "build", packageFolder);
    }

    public static async Task<int> PublishAsync()
    {
        var packageFolder = Settings.Instance.PackageFolder;

        string packagePath = Directory.EnumerateFiles(packageFolder)
            .Single(f => Path.GetExtension(f) == ".yak");

        string packageFile = Path.GetFileName(packagePath);
        string yak = await GetYakPathAsync();
        return Run(yak, $"push {packageFile}", packageFolder);
    }

    public static async Task<int> ReleaseAsync()
    {
        var settings = Settings.Instance;
        string version = Manifest.GetVersion();

        var client = new GitHubClient(new ProductHeaderValue(settings.GithubOwner))
        {
            Credentials = new Credentials(GetSecret("GITHUB_TOKEN"))
        };

        var body = new StringBuilder();

        var notes = Release.GetReleaseNotes()
            .FirstOrDefault(n => n.Version == version);

        if (notes is not null)
        {
            foreach (var change in notes.Changes)
                body.AppendLine($"- {change}");

            body.AppendLine();
        }

        body.AppendLine(settings.ReleaseMessage);

        var release = new NewRelease(version)
        {
            Name = version,
            Body = body.ToString(),
            Prerelease = false
        };

        var result = await client.Repository.Release.Create(settings.GithubOwner, settings.GithubRepo, release);
        Log($"Created release id: {result.Id}");
        return 0;
    }
}