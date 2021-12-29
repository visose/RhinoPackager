using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RhinoPackager;

record ReleaseItem
{
    public string Version { get; init; } = default!;
    public List<string> Changes { get; init; } = default!;
}

static class Release
{
    public static List<ReleaseItem> GetReleaseNotes()
    {
        var releaseFile = Settings.Instance.ReleaseFile;

        if (!File.Exists(releaseFile))
            return new List<ReleaseItem>(0);

        var text = File.ReadAllText(releaseFile);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        return deserializer.Deserialize<List<ReleaseItem>>(text);
    }
}