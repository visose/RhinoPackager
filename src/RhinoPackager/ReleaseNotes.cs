using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RhinoPackager;

record ReleaseItem
{
    public string Version { get; init; } = default!;
    public List<string> Changes { get; init; } = default!;
}

static class ReleaseNotes
{
    public static string? GetReleaseNotes(string releaseFile, string version)
    {
        var notes = GetAllReleaseNotes(releaseFile)
            .FirstOrDefault(n => n.Version == version);

        if (notes is null)
            return null;

        var text = new System.Text.StringBuilder();
        text.AppendLine($"Changes in {version}:");

        foreach (var change in notes.Changes)
            text.AppendLine($" - {change}");

        return text.ToString();
    }


    static List<ReleaseItem> GetAllReleaseNotes(string releaseFile)
    {
        if (!File.Exists(releaseFile))
            return new List<ReleaseItem>(0);

        var text = File.ReadAllText(releaseFile);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        return deserializer.Deserialize<List<ReleaseItem>>(text);
    }


}