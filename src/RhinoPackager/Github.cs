using Octokit;
using static RhinoPackager.Util;

namespace RhinoPackager;

public class Github
{
    readonly string _owner;
    readonly string _repo;
    readonly GitHubClient _client;

    public Github(string owner, string repo)
    {
        _owner = owner;
        _repo = repo;

        _client = new(new ProductHeaderValue(_owner))
        {
            Credentials = new(GetSecret("GITHUB_TOKEN"))
        };
    }

    public async Task<bool> TagExistsAsync(string tag)
    {
        var tags = await _client.Repository.GetAllTags(_owner, _repo);
        return tags.Any(t => t.Name.Equals(tag, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Release> AddReleaseAsync(string version, string body)
    {
        var preTags = new[] { "alpha", "beta" };
        var isPrerelease = preTags.Any(t => version.Contains(t, StringComparison.OrdinalIgnoreCase));

        NewRelease release = new(version)
        {
            Name = version,
            Body = body,
            Prerelease = isPrerelease,
        };

        return await _client.Repository.Release.Create(_owner, _repo, release);
    }

    public async Task AddReleaseAssetsAsync(Release release, string[] files)
    {
        await Parallel.ForEachAsync(files, async (file, cancel) =>
        {
            var name = Path.GetFileName(file);
            var mime = "application/octet-stream";
            using var stream = File.OpenRead(file);
            ReleaseAssetUpload uploadData = new(name, mime, stream, null);
            await _client.Repository.Release.UploadAsset(release, uploadData, cancel);
        });
    }
}
