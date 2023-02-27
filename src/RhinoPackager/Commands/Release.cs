using System.Text;
using static RhinoPackager.Util;

namespace RhinoPackager.Commands;

public class Release : ICommand
{
    readonly Props _props;
    readonly Github _github;
    readonly string? _notesFile;
    readonly string? _message;
    readonly string[] _assetsFiles;

    public Release(Props props, Github github, string? notesFile = null, string? message = null, string? assetsFolder = null, string[]? assetsFiles = null)
    {
        _props = props;
        _github = github;
        _notesFile = notesFile;
        _message = message;

        _assetsFiles = assetsFiles is not null
            ? assetsFiles
                .Select(f => Path.Combine(assetsFolder ?? "", f))
                .ToArray()
            : Array.Empty<string>();
    }

    public async Task<int> RunAsync(bool publish)
    {
        string version = _props.GetVersion();
        StringBuilder body = new();

        if (_notesFile is not null)
        {
            var notes = ReleaseNotes.GetReleaseNotes(_notesFile, version);

            if (notes is not null)
                body.AppendLine(notes);
        }

        if (_message is not null)
            body.AppendLine(_message);

        if (!publish)
        {
            Log("Skipping publishing Github release...");
            return 0;
        }

        var result = await _github.AddReleaseAsync(version, body.ToString());
        await _github.AddReleaseAssetsAsync(result, _assetsFiles);

        Log($"Created release id: {result.Id}");
        return 0;
    }
}
