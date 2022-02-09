namespace RhinoPackager;

public interface ICommand
{
    Task<int> RunAsync(bool publish);
}