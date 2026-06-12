using static RhinoPackager.Util;

namespace RhinoPackager.Commands;

public class Build(string project, string target = "publish", string[]? arguments = null) : ICommand
{
    readonly string _project = string.IsNullOrWhiteSpace(project)
        ? throw new ArgumentException("Project path cannot be empty.", nameof(project))
        : project;

    readonly string _target = string.IsNullOrWhiteSpace(target)
        ? throw new ArgumentException("Dotnet target cannot be empty.", nameof(target))
        : target;

    public Task Run(CommandContext context)
    {
        List<string> dotnetArguments = [_target, _project];
        AddDotnetDefaults(dotnetArguments, context);

        dotnetArguments.AddRange(arguments ?? []);
        _ = ProcessRunner.Run("dotnet", dotnetArguments);
        return Task.CompletedTask;
    }
}
