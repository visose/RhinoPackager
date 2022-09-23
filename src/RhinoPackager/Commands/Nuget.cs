using static RhinoPackager.Util;

namespace RhinoPackager.Commands;

public class Nuget : ICommand
{
    readonly Props _props;
    readonly string _project;
    readonly string? _targets;
    readonly string? _certPath;

    public Nuget(Props props, string project, string? targets = null, string? certPath = null)
    {
        _props = props;
        _project = project;
        _targets = targets;
        _certPath = certPath;
    }

    public Task<int> RunAsync(bool publish)
    {
        var result = Pack();

        if (result != 0)
            return Task.FromResult(result);

        result = Sign();

        if (result != 0)
            return Task.FromResult(result);

        result = Publish(publish);

        return Task.FromResult(result);
    }

    int Pack()
    {
        string? targetsArg = _targets is not null 
            ? $"-p:TargetFrameworks={_targets}" 
            : null;

        var folder = GetFolder();
        return RunDotnet("pack", $"{targetsArg} {_project} -o {folder}");
    }

    int Publish(bool publish)
    {
        string packageFile = GetPackageFileName();
        var key = GetSecret("NUGET_KEY");
        var folder = GetFolder();

        if (!publish)
        {
            Log("Skipping publishing Nuget package...");
            return 0;
        }

        return Run("dotnet", $"nuget push {packageFile} -k {key} -s https://api.nuget.org/v3/index.json", folder);
    }

    int Sign()
    {
        if (_certPath is null)
        {
            Log("Skipping signing package...");
            return 0;
        }

        string packageFile = GetPackageFileName();
        var certPass = GetSecret("CERTPASS");
        var timeStamper = "http://timestamp.digicert.com";
        var folder = GetFolder();

        return Run("nuget", $"sign {packageFile} -CertificatePath {_certPath} -CertificatePassword {certPass} -Timestamper {timeStamper} -NonInteractive", folder);
    }

    string GetPackageFileName()
    {
        var projectProps = new Props(_project);
        var name = projectProps.Get("PackageId");

        var version = _props.GetVersion();
        return $"{name}.{version}.nupkg";
    }

    static string GetFolder() => Path.Combine("artifacts", "Nuget");
}