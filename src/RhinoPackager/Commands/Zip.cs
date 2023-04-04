using System.IO.Compression;

namespace RhinoPackager.Commands;

public class Zip : ICommand
{
    readonly string _targetPath;
    readonly string _sourceFolder;
    readonly string[] _files;

    public Zip(string targetPath, string sourceFolder, string[] files)
    {
        _targetPath = targetPath;
        _sourceFolder = sourceFolder;
        _files = files;
    }
    public async Task<int> RunAsync(bool publish)
    {
        using var zipStream = CreatePackage();
        zipStream.Position = 0;

        using var fileStream = File.Create(_targetPath);
        await zipStream.CopyToAsync(fileStream);

        return 0;
    }

    Stream CreatePackage()
    {
        MemoryStream memoryStream = new();
        using ZipArchive archive = new(memoryStream, ZipArchiveMode.Create, true);

        foreach (var file in _files)
        {
            var localPath = Path.Combine(_sourceFolder, file);
            archive.CreateEntryFromFile(localPath, file);
        }

        return memoryStream;
    }
}
