using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RhinoPackager;

sealed record ReleaseItem
{
    public required string Version { get; init; }
    public required List<string> Changes { get; init; }
}

static class ReleaseNotes
{
    public static string? GetReleaseNotes(string? releaseFile, string version)
    {
        if (releaseFile is null)
            return null;

        var matchingNotes = GetAllReleaseNotes(releaseFile)
            .Where(note => note.Version == version)
            .ToArray();

        if (matchingNotes.Length == 0)
            throw new InvalidOperationException($"Release notes for version '{version}' were not found in {releaseFile}.");

        if (matchingNotes.Length > 1)
            throw new InvalidOperationException($"Release notes for version '{version}' are duplicated in {releaseFile}.");

        var notes = matchingNotes[0];
        StringBuilder text = new();
        _ = text
            .Append("Changes in ")
            .Append(version)
            .AppendLine(":");

        foreach (var change in notes.Changes)
        {
            _ = text
                .Append(" - ")
                .AppendLine(change);
        }

        return text.ToString();
    }

    static List<ReleaseItem> GetAllReleaseNotes(string releaseFile)
    {
        if (!File.Exists(releaseFile))
            throw new FileNotFoundException($"Release notes file was not found: {releaseFile}", releaseFile);

        var text = File.ReadAllText(releaseFile);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var notes = deserializer.Deserialize<List<ReleaseItem>>(text)
            ?? throw new InvalidOperationException($"Release notes file is empty: {releaseFile}.");

        foreach (var note in notes)
        {
            if (string.IsNullOrWhiteSpace(note.Version))
                throw new InvalidOperationException($"Release notes file contains an item without a version: {releaseFile}.");

            if (note.Changes is null)
                throw new InvalidOperationException($"Release notes for version '{note.Version}' do not define changes: {releaseFile}.");
        }

        return notes;
    }
}
