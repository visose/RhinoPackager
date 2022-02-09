using System.Text;
using static RhinoPackager.Util;

namespace RhinoPackager.Commands;

public class Release : ICommand
{
    readonly Github _github;
    readonly string? _file;
    readonly string? _message;

    public Release(Github github, string? file = null, string? message = null)
    {
        _github = github;
        _file = file;
        _message = message;
    }

    public async Task<int> RunAsync(bool publish)
    {
        string version = Props.GetVersion();
        var body = new StringBuilder();

        if (_file is not null)
        {
            var notes = ReleaseNotes.GetReleaseNotes(_file, version);

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
        Log($"Created release id: {result.Id}");
        return 0;
    }
}