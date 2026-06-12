using static RhinoPackager.Util;

namespace RhinoPackager.Commands;

public class Nuget(Props props, string project, string? targets = null, string? certPath = null) : ICommand
{
    public Task Run(CommandContext context)
    {
        Pack(context);
        Sign();
        Publish(context);

        return Task.CompletedTask;
    }

    void Pack(CommandContext context)
    {
        var folder = GetFolder();
        List<string> arguments = ["pack", project];
        AddDotnetDefaults(arguments, context);
        arguments.Add("--output");
        arguments.Add(folder);

        if (targets is not null)
        {
            var targetProperty = targets.Contains(';', StringComparison.Ordinal)
                ? "TargetFrameworks"
                : "TargetFramework";

            arguments.Add($"-p:{targetProperty}={targets}");
        }

        _ = ProcessRunner.Run("dotnet", arguments);
    }

    void Publish(CommandContext context)
    {
        if (!context.Publish)
        {
            Log("Skipping publishing Nuget package...");
            return;
        }

        var packageFile = GetPackageFileName();
        var key = GetSecret("NUGET_KEY");
        var folder = GetFolder();

        _ = ProcessRunner.Run(
            "dotnet",
            ["nuget", "push", packageFile, "--api-key", key, "--source", "https://api.nuget.org/v3/index.json"],
            folder,
            [key]);
    }

    void Sign()
    {
        if (certPath is null)
        {
            Log("Skipping signing package...");
            return;
        }

        string packageFile = GetPackageFileName();
        var certPass = GetSecret("CERTPASS");
        var timeStamper = "http://timestamp.digicert.com";
        var folder = GetFolder();

        _ = ProcessRunner.Run(
            "nuget",
            [
                "sign",
                packageFile,
                "-CertificatePath",
                certPath,
                "-CertificatePassword",
                certPass,
                "-Timestamper",
                timeStamper,
                "-NonInteractive"
            ],
            folder,
            [certPass]);
    }

    string GetPackageFileName()
    {
        Props projectProps = new(project);
        var name = projectProps.Get("PackageId");

        var version = props.GetVersion();
        return $"{name}.{version}.nupkg";
    }

    static string GetFolder() => Path.Combine("artifacts", "Nuget");
}
