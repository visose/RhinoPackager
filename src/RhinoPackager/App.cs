using static RhinoPackager.Util;

namespace RhinoPackager;

public class App
{
    readonly AppOptions _options;
    readonly List<ICommand> _commands = [];

    public static App Create(string[] args)
    {
        var options = AppOptions.Parse(args);
        return new(options);
    }

    private App(AppOptions options) => _options = options;

    public void Add(params ICommand[] commands) =>
        _commands.AddRange(commands);

    public async Task<int> Run()
    {
        CommandContext context = new(_options);

        if (!_options.Publish)
            Log("Publishing disabled. Use --publish to push packages and create releases.");

        foreach (var command in _commands)
        {
            string name = command.GetType().Name;
            Log($"Starting {name}...");

            await command.Run(context);
        }

        Log("Finished with no errors.");
        return 0;
    }
}
