using static RhinoPackager.Util;

namespace RhinoPackager.Commands;

public class CheckVersion(Props props, Github github) : ICommand
{
    public async Task Run(CommandContext context)
    {
        if (!context.Publish)
        {
            Log("Skipping version check...");
            return;
        }

        string version = props.GetVersion();
        var versionExists = await github.TagExists(version);

        if (versionExists)
            throw new InvalidOperationException($"Version number {version} already exists. Update Version before publishing.");

    }
}
