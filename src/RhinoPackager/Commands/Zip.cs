using System.IO.Compression;

namespace RhinoPackager.Commands;

public class Zip(string targetPath, string sourceFolder, string[] files) : ICommand
{
    public Task Run(CommandContext context)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(targetPath));

        if (!string.IsNullOrWhiteSpace(directory))
            _ = Directory.CreateDirectory(directory);

        using var fileStream = File.Create(targetPath);
        using ZipArchive archive = new(fileStream, ZipArchiveMode.Create);

        foreach (var file in files)
        {
            var localPath = Path.Combine(sourceFolder, file);
            _ = archive.CreateEntryFromFile(localPath, file);
        }

        return Task.CompletedTask;
    }
}
