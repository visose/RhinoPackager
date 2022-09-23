using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RhinoPackager;

class Manifest
{
    public string Name { get; private set; }
    public string Version { get; private set; }
    public string[] Authors { get; private set; }

    [YamlMember(ScalarStyle = ScalarStyle.Literal)]
    public string Description { get; private set; }
    public string Url { get; private set; }
    public string[] Keywords { get; private set; }
    public string Icon { get; private set; }

    public Manifest(Props props)
    {
        Name = props.Get("Product");
        Version = props.GetVersion();
        Authors = props.GetList("Authors");
        Description = GetDescription(props, Version);
        Url = props.Get("PackageProjectUrl");
        Keywords = props.GetList("PackageTags");
        Icon = props.Get("Icon");
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
        var description = new StringBuilder();
        description.AppendLine(props.Get("Description"));

        string releaseFile = props.Get("ReleaseNotes");
        var notes = ReleaseNotes.GetReleaseNotes(releaseFile, version);

        if (notes is null)
            return description.ToString();

        description.AppendLine();
        description.Append(notes);

        return description.ToString();
    }
}