using System.Runtime.InteropServices;
using static RhinoPackager.Util;

namespace RhinoPackager.Commands;

public class Yak : ICommand
{
    readonly Manifest _manifest;
    readonly string _sourceFolder;
    readonly string[] _files;
    readonly string[] _tags;

    public Yak(string propsFile, string sourceFolder, string[] files, string[] tags)
    {
        _manifest = new Manifest(propsFile);
        _sourceFolder = sourceFolder;
        _files = files;
        _tags = tags;
    }

    public async Task<int> RunAsync(bool publish)
    {
        int result = await PackageAsync();

        if (result != 0)
            return result;

        result = await PublishAsync(publish);
        return result;
    }

    async Task<int> PackageAsync()
    {
        var folder = GetFolder();

        // Package folder
        if (Directory.Exists(folder))
            Directory.Delete(folder, true);

        Directory.CreateDirectory(folder);

        // Copy bin files

        foreach (var file in _files)
        {
            var source = Path.Combine(_sourceFolder, file);
            var destination = Path.Combine(folder, file);
            File.Copy(source, destination, true);
        }

        // Save manifest
        _manifest.Save(folder);

        // Build package
        string yak = await GetYakPathAsync();
        int result = Run(yak, "build", folder);

        if (result != 0)
            return result;

        // Multiple versions
        var packagePath = Directory.EnumerateFiles(folder, "*.yak").Single();

        foreach (var tag in _tags)
        {
            var newPackagePath = Path.Combine(folder, GetPackageFileName(tag));

            if (packagePath.Equals(newPackagePath, StringComparison.OrdinalIgnoreCase))
                continue;

            File.Copy(packagePath, newPackagePath);
            Log($"File copied to: {Path.GetFileName(newPackagePath)}");
        }

        return result;
    }

    async Task<int> PublishAsync(bool publish)
    {
        string yak = await GetYakPathAsync();
        var folder = GetFolder();

        if (!publish)
        {
            Log("Skipping publishing Yak packages...");
            return 0;
        }

        foreach(var tag in _tags)
        {
            var packageFile = GetPackageFileName(tag);
            var result = Run(yak, $"push {packageFile}", folder);

            if (result != 0)
                return result;
        }

        return 0;
    }

    static async Task<string> GetYakPathAsync()
    {
        const string yak = "Yak.exe";
        const string rhino = "C:/Program Files/Rhino 7/System/Yak.exe";

        if (File.Exists(rhino))
            return rhino;

        string yakPath = Path.GetFullPath(yak);

        if (File.Exists(yakPath))
            return yakPath;

        var http = new HttpClient();
        var bytes = await http.GetByteArrayAsync($"http://files.mcneel.com/yak/tools/latest/yak.exe");
        await File.WriteAllBytesAsync(yakPath, bytes);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            Run("chmod", $"+x {yakPath}");

        return yakPath;
    }

    string GetPackageFileName(string tag) => 
        $"{_manifest.Name}-{_manifest.Version}-{tag}.yak".ToLowerInvariant();

    string GetFolder() => Path.Combine("artifacts", "Yak", _manifest.Name);
}