using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RhinoPackager;

sealed class Manifest
{
    public string Name { get; }
    public string Version { get; }
    public string[] Authors { get; }

    [YamlMember(ScalarStyle = ScalarStyle.Literal)]
    public string Description { get; }
    public string Url { get; }
    public string[] Keywords { get; }
    public string Icon { get; }

    public Manifest(Props props)
    {
        Name = props.Get("Product");
        Version = props.GetVersion();
        Authors = props.GetList("Authors");
        Description = GetDescription(props, Version);
        Url = props.Get("PackageProjectUrl");
        Keywords = props.GetList("PackageTags");
        Icon = props.GetOrDefault("Icon") ?? props.Get("PackageIcon");
    }

    public void Save(string saveFolder)
    {
        var text = ToYaml();
        var file = Path.Combine(saveFolder, "manifest.yml");
        File.WriteAllText(file, text);
    }

    string ToYaml()
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();

        return serializer.Serialize(this);
    }

    static string GetDescription(Props props, string version)
    {
        StringBuilder description = new();
        _ = description.AppendLine(props.Get("Description"));

        var releaseFile = props.GetOrDefault("ReleaseNotes");
        var notes = ReleaseNotes.GetReleaseNotes(releaseFile, version);

        if (notes is null)
            return description.ToString();

        _ = description.AppendLine();
        _ = description.Append(notes);

        return description.ToString();
    }
}
