namespace RhinoPackager;

public sealed record AppOptions(bool Publish, string Configuration)
{
    public const string DefaultConfiguration = "Release";

    public static AppOptions Parse(string[] args)
    {
        var publish = false;
        var configuration = DefaultConfiguration;

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            switch (arg)
            {
                case "--publish":
                    publish = true;
                    break;

                case "--no-publish":
                case "--dry-run":
                    publish = false;
                    break;

                case "-c":
                case "--configuration":
                    configuration = ReadValue(args, ref i, arg);
                    break;

                case "debug":
                    throw new ArgumentException("The debug argument was removed. Use --no-publish instead.");

                default:
                    throw new ArgumentException($"Unknown argument '{arg}'. Use --publish, --no-publish, or --configuration <name>.");
            }
        }

        if (string.IsNullOrWhiteSpace(configuration))
            throw new ArgumentException("Configuration cannot be empty.");

        return new(publish, configuration);
    }

    static string ReadValue(string[] args, ref int index, string option)
    {
        if (index + 1 >= args.Length)
            throw new ArgumentException($"Missing value for {option}.");

        index++;
        return args[index];
    }
}

public sealed record CommandContext(AppOptions Options)
{
    public bool Publish => Options.Publish;
    public string Configuration => Options.Configuration;
}
