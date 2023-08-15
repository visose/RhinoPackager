using static RhinoPackager.Util;

namespace RhinoPackager.Commands;

public class CheckVersion : ICommand
{
    readonly Props _props;
    readonly Github _github;

    public CheckVersion(Props props, Github github)
    {
        _github = github;
        _props = props;
    }

    public async Task<int> RunAsync(bool publish)
    {
        if (!publish)
        {
            Log($"Skipping version check ...");
            return 0;
        }

        string version = _props.GetVersion();
        var versionExists = await _github.TagExistsAsync(version);

        if (versionExists)
        {
            Log($"Version number {version} not updated, nothing else to do.");
            return -1;
        }

        return 0;
    }
}
