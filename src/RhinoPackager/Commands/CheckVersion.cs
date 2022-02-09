using static RhinoPackager.Util;

namespace RhinoPackager.Commands;

public class CheckVersion : ICommand
{
    readonly Github _github;

    public CheckVersion(Github github)
    {
        _github = github;
    }

    public async Task<int> RunAsync(bool publish)
    {
        if (!publish)
        {
            Log($"Skipping version check ...");
            return 0;
        }

        var publishedVersion = await _github.GetLatestVersionAsync();
        string version = Props.GetVersion();

        if (publishedVersion == version)
        {
            Log($"Version number {version} not updated, nothing else to do.");
            return -1;
        }

        return 0;
    }
}