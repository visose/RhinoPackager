using static RhinoPackager.Util;

namespace RhinoPackager.Commands;

public class Build : ICommand
{
    readonly string _buildProject;

    public Build(string buildProject)
    {
        _buildProject = buildProject;
    }

    public Task<int> RunAsync(bool publish)
    {
        var result = RunDotnet("build", _buildProject);
        return Task.FromResult(result);
        //var result = Run("dotnet", $"build -c Release {ciArg} {_buildProject}");
    }
}