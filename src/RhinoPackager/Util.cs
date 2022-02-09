using System.Diagnostics;
using System.Text.Json;

namespace RhinoPackager;

static class Util
{
    public static void Log(string? text)
    {
        Console.WriteLine(text);
    }

    public static int RunDotnet(string target, string args)
    {
        var ci = Environment.GetEnvironmentVariable("CI");

        string? ciArg = ci == "true"
            ? "-p:ContinuousIntegrationBuild=\"true\""
            : null;

        return Run("dotnet", $"{target} -c Release {ciArg} {args}");
    }

    public static int Run(string file, string args, string? setCurrentDir = null)
    {
        var currentDir = setCurrentDir ?? Directory.GetCurrentDirectory();

        var startInfo = new ProcessStartInfo
        {
            FileName = file,
            Arguments = args,
            WorkingDirectory = currentDir,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process
        {
            StartInfo = startInfo
        };

        process.OutputDataReceived += (o, e) => Log(e.Data);
        process.ErrorDataReceived += (o, e) => Log(e.Data);
        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        process.WaitForExit();

        return process.ExitCode;
    }

    public static string GetSecret(string key)
    {
        string? value = Environment.GetEnvironmentVariable(key);

        if (value is not null)
            return value;

        var json = File.ReadAllText("build/secrets.json");
        var doc = JsonDocument.Parse(json);
        var prop = doc.RootElement.GetProperty(key);
        return prop.GetString().NotNull($"Secret {key} not found.");
    }

    public static T NotNull<T>(this T? value, string? text = null)
    {
        return value ?? throw new ArgumentNullException(text ?? nameof(value));
    }
}