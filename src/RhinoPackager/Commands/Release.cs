using System.Text;
using static RhinoPackager.Util;

namespace RhinoPackager.Commands;

public class Release(Props props, Github github, string? notesFile = null, string? message = null, string? assetsFolder = null, string[]? assetsFiles = null) : ICommand
{
    readonly string[] _assetsFiles = assetsFiles is not null
            ? [.. assetsFiles.Select(f => Path.Combine(assetsFolder ?? "", f))]
            : [];

    public async Task Run(CommandContext context)
    {
        string version = props.GetVersion();
        StringBuilder body = new();

        if (notesFile is not null)
        {
            var notes = ReleaseNotes.GetReleaseNotes(notesFile, version);

            if (notes is not null)
                _ = body.AppendLine(notes);
        }

        if (message is not null)
            _ = body.AppendLine(message);

        if (!context.Publish)
        {
            Log("Skipping publishing Github release...");
            return;
        }

        var result = await github.AddRelease(version, body.ToString());
        await github.AddReleaseAssets(result, _assetsFiles);

        Log($"Created release id: {result.Id}");
    }
}
