namespace RhinoPackager;

public interface ICommand
{
    Task Run(CommandContext context);
}
