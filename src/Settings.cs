using System.Text.Json;

namespace RhinoPackager;

class Settings
{
    public static Settings Instance { get; }

    static Settings()
    {
        var json = File.ReadAllText("build/settings.json");
        Instance = JsonSerializer.Deserialize<Settings>(json).NotNull();
    }

    public string TestProject { get; set; } = default!;
    public string BuildProject { get; set; } = default!;
    public string BuildFolder { get; set; } = default!;
    public string PackageFolder { get; set; } = default!;
    public string[] PackageFiles { get; set; } = default!;
    public string GithubOwner { get; set; } = default!;
    public string GithubRepo { get; set; } = default!;
    public string ReleaseFile { get; set; } = default!;
    public string ReleaseMessage { get; set; } = default!;
    public string Tag { get; set; } = default!;
}