using System.Xml.Linq;

namespace RhinoPackager;

public class Props
{
    readonly XElement _element;

    public Props(string propsFile = "Directory.Build.props") =>
        _element = GetPropsElement(propsFile);

    public string GetVersion() => Get("Version");
    public string GetName() => Get("Product");

    public string Get(string name)
        => GetOrDefault(name).NotNull(name);

    public string? GetOrDefault(string name)
        => _element.Element(XName.Get(name))?.Value;

    public string[] GetList(string name) =>
        Get(name).Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    public void Set(string key, string value)
    {
        var element = _element.Element(XName.Get(key))
            ?? throw new($"Element {key} not found");

        element.Value = value;
    }

    static XElement GetPropsElement(string propsFile)
    {
        var doc = XDocument.Load(propsFile);
        XElement props = (doc.Root?.Descendants().First()).NotNull();
        return props;
    }
}
