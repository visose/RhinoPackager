using static RhinoPackager.Util;

namespace RhinoPackager.Commands;

public class Build : ICommand
{
    readonly string _target;
    readonly List<string> _args = new();

    public Build(string buildProject, string target = "build", params string[] args)
    {
        _target = target;

        _args.Add(buildProject);
        _args.AddRange(args);
    }

    public Task<int> RunAsync(bool publish)
    {
        var result = RunDotnet(_target, _args.ToArray());
        return Task.FromResult(result);
        //var result = Run("dotnet", $"build -c Release {ciArg} {_buildProject}");
    }
}
