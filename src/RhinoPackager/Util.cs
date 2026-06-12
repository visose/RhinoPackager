using System.Text.Json;

namespace RhinoPackager;

public static class Util
{
    public static void Log(string? text)
    {
        if (text is not null)
            Console.WriteLine(text);
    }

    public static void AddDotnetDefaults(List<string> arguments, CommandContext context)
    {
        arguments.Add("--configuration");
        arguments.Add(context.Configuration);
        arguments.Add("--nologo");

        if (Environment.GetEnvironmentVariable("CI") == "true")
            arguments.Add("-p:ContinuousIntegrationBuild=true");
    }

    public static string GetSecret(string key)
    {
        string? value = Environment.GetEnvironmentVariable(key);

        if (!string.IsNullOrWhiteSpace(value))
            return value;

        var localSecrets = "secrets.json";

        if (File.Exists(localSecrets))
        {
            var json = File.ReadAllText(localSecrets);
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty(key, out var property))
            {
                value = property.GetString();

                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
        }

        throw new InvalidOperationException($"Secret '{key}' was not found in the environment or secrets.json.");
    }
}
