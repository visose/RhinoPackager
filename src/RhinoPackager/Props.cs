using System.Xml.Linq;

namespace RhinoPackager;

public class Props(string propsFile = "Directory.Build.props")
{
    readonly XDocument _document = XDocument.Load(propsFile);
    readonly string _propsFile = propsFile;

    public string GetVersion() => Get("Version");
    public string GetName() => Get("Product");

    public string Get(string name)
    {
        return GetOrDefault(name)
            ?? throw new InvalidOperationException($"Property '{name}' was not found in {_propsFile}.");
    }

    public string? GetOrDefault(string name)
    {
        return GetProperty(name)?.Value;
    }

    public string[] GetList(string name) =>
        Get(name).Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    public void Set(string key, string value)
    {
        var element = GetProperty(key)
            ?? throw new InvalidOperationException($"Property '{key}' was not found in {_propsFile}.");

        element.Value = value;
    }

    XElement? GetProperty(string name)
    {
        var root = _document.Root
            ?? throw new InvalidOperationException($"Props file '{_propsFile}' does not have a root element.");

        return root
            .Elements()
            .Where(element => element.Name.LocalName == "PropertyGroup")
            .Elements()
            .FirstOrDefault(element => element.Name.LocalName == name);
    }
}
