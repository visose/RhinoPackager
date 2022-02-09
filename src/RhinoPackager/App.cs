using static RhinoPackager.Util;

namespace RhinoPackager;

public class App
{
    readonly bool _publish = false;
    readonly List<ICommand> _commands = new();

    public static App Create(string[] args)
    {
        var publish = !args.Any(a => a.Equals("debug", StringComparison.OrdinalIgnoreCase));
        return new App(publish);
    }

    private App(bool publish) => _publish = publish;

    public void Add(IEnumerable<ICommand> commands)
    {
        _commands.AddRange(commands);
    }

    public async Task<int> RunAsync()
    {
        foreach (var command in _commands)
        {
            string name = command.GetType().Name;
            Log($"Starting {name}...");

            var result = await command.RunAsync(_publish);

            if (result != 0)
            {
                Log($"Stopped at step: {name}");
                return Math.Max(result, 0);
            }
        }

        Log("Finished with no errors.");
        return 0;
    }
}