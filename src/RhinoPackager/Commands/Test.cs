using static RhinoPackager.Util;

namespace RhinoPackager.Commands;

public class Test(string testProject) : ICommand
{
    public Task Run(CommandContext context)
    {
        List<string> arguments = ["test", testProject];
        AddDotnetDefaults(arguments, context);

        _ = ProcessRunner.Run("dotnet", arguments);
        return Task.CompletedTask;
    }
}
