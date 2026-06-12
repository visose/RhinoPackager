using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using static RhinoPackager.Util;

namespace RhinoPackager.Commands;

public class Yak(Props props, string sourceFolder, string[] tags, string[]? exclude = null, bool publish = true) : ICommand
{
    const string LinuxYakUrl = "https://files.mcneel.com/yak/tools/0.13.0/linux-x64/yak";
    const string WindowsRhinoYakPath = "C:/Program Files/Rhino 8/System/Yak.exe";

    readonly Props _props = props;
    readonly string _sourceFolder = sourceFolder;
    readonly string[] _tags = tags;
    readonly string[] _excludePatterns = exclude ?? [];
    readonly bool _publish = publish;

    public async Task Run(CommandContext context)
    {
        await Package();

        if (!_publish)
            return;

        await Publish(context);
    }

    async Task Package()
    {
        var folder = GetFolder();

        if (Directory.Exists(folder))
            Directory.Delete(folder, true);

        _ = Directory.CreateDirectory(folder);

        var sourceFolder = Path.GetFullPath(_sourceFolder);

        if (!Directory.Exists(sourceFolder))
            throw new DirectoryNotFoundException($"Yak source folder not found: {sourceFolder}");

        foreach (var source in Directory.EnumerateFiles(sourceFolder, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceFolder, source);
            var relativePackagePath = NormalizePackagePath(relativePath);

            if (IsExcluded(relativePackagePath))
                continue;

            var destination = Path.Combine(folder, relativePath);
            var dir = Path.GetDirectoryName(destination);

            if (!string.IsNullOrWhiteSpace(dir))
                _ = Directory.CreateDirectory(dir);

            File.Copy(source, destination, true);
        }

        Manifest manifest = new(_props);
        manifest.Save(folder);

        string yak = await GetYakPath();
        _ = ProcessRunner.Run(yak, ["build"], folder);

        var packagePaths = Directory.GetFiles(folder, "*.yak");

        if (packagePaths.Length != 1)
            throw new InvalidOperationException($"Yak build should produce exactly one .yak file, but produced {packagePaths.Length}.");

        var packagePath = packagePaths[0];

        foreach (var tag in _tags)
        {
            var newPackagePath = Path.Combine(folder, GetPackageFileName(_props, tag));

            if (packagePath.Equals(newPackagePath, StringComparison.OrdinalIgnoreCase))
                continue;

            File.Copy(packagePath, newPackagePath);
            Log($"File copied to: {Path.GetFileName(newPackagePath)}");
        }
    }

    async Task Publish(CommandContext context)
    {
        string yak = await GetYakPath();
        var folder = GetFolder();

        if (!context.Publish)
        {
            Log("Skipping publishing Yak packages...");
            return;
        }

        foreach (var tag in _tags)
        {
            var packageFile = GetPackageFileName(_props, tag);
            _ = ProcessRunner.Run(yak, ["push", packageFile], folder);
        }
    }

    static async Task<string> GetYakPath()
    {
        var localPath = Environment.GetEnvironmentVariable("YAK_PATH");

        if (!string.IsNullOrWhiteSpace(localPath))
        {
            var fullPath = Path.GetFullPath(localPath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"YAK_PATH points to a file that does not exist: {fullPath}", fullPath);

            return fullPath;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && File.Exists(WindowsRhinoYakPath))
            return WindowsRhinoYakPath;

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            throw new PlatformNotSupportedException($"Set YAK_PATH or install Rhino 8 at {WindowsRhinoYakPath} to use Yak on this platform.");

        var yakPath = Path.GetFullPath(Path.Combine("artifacts", "tools", "yak"));

        if (File.Exists(yakPath))
            return yakPath;

        var directory = Path.GetDirectoryName(yakPath)
            ?? throw new InvalidOperationException("Invalid Yak path.");

        _ = Directory.CreateDirectory(directory);

        using HttpClient http = new();
        var bytes = await http.GetByteArrayAsync(LinuxYakUrl);
        await File.WriteAllBytesAsync(yakPath, bytes);

        _ = ProcessRunner.Run("chmod", ["+x", yakPath]);

        return yakPath;
    }

    public static string GetPackageFileName(Props props, string tag) =>
        $"{props.GetName()}-{props.GetVersion()}-{tag}.yak".ToLowerInvariant();

    bool IsExcluded(string packagePath)
    {
        foreach (var pattern in _excludePatterns)
        {
            if (MatchesPattern(packagePath, pattern))
                return true;
        }

        return false;
    }

    static bool MatchesPattern(string packagePath, string pattern)
    {
        pattern = NormalizePackagePath(pattern).Trim();

        if (pattern.Length == 0)
            return false;

        if (pattern.StartsWith("regex:", StringComparison.OrdinalIgnoreCase))
            return Regex.IsMatch(packagePath, pattern[6..], RegexOptions.IgnoreCase);

        var target = pattern.Contains('/') ? packagePath : Path.GetFileName(packagePath);
        var regex = WildcardToRegex(pattern);
        return Regex.IsMatch(target, regex, RegexOptions.IgnoreCase);
    }

    static string NormalizePackagePath(string path) =>
        path.Replace('\\', '/');

    static string WildcardToRegex(string pattern)
    {
        StringBuilder builder = new("^");

        for (int i = 0; i < pattern.Length; i++)
        {
            var c = pattern[i];

            if (c == '*')
            {
                if (i + 1 < pattern.Length && pattern[i + 1] == '*')
                {
                    _ = builder.Append(".*");
                    i++;
                    continue;
                }

                _ = builder.Append("[^/]*");
                continue;
            }

            if (c == '?')
            {
                _ = builder.Append("[^/]");
                continue;
            }

            _ = builder.Append(Regex.Escape(c.ToString()));
        }

        _ = builder.Append('$');
        return builder.ToString();
    }

    static string GetFolder() => Path.Combine("artifacts", "yak");
}
