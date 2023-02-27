using static RhinoPackager.Util;

namespace RhinoPackager.Commands;

public class Test : ICommand
{
    readonly string _testProject;

    public Test(string testProject) => _testProject = testProject;

    public Task<int> RunAsync(bool publish)
    {
        var result = RunDotnet("test", _testProject);
        return Task.FromResult(result);

        //var result = Run("dotnet", $"test -c Release {_testProject}");
    }
}
