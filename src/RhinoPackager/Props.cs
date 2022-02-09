using System.Xml.Linq;

namespace RhinoPackager;

static class Props
{
    public static string GetVersion()
    {
        var props = GetPropsElement("Directory.Build.props");
        return props.GetItem("Version");
    }

    public static XElement GetPropsElement(string propsFile)
    {
        var doc = XDocument.Load(propsFile);
        XElement props = (doc.Root?.Descendants().First()).NotNull();
        return props;
    }

    public static string GetItem(this XElement element, string name) =>
    (element.Element(XName.Get(name))?.Value).NotNull();

    public static string[] GetList(this XElement element, string name) =>
        element.GetItem(name).Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}