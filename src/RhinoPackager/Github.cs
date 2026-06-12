using Octokit;
using static RhinoPackager.Util;

namespace RhinoPackager;

public class Github(string owner, string repo)
{
    readonly GitHubClient _client = new(new ProductHeaderValue(owner));
    bool _authenticated;

    public async Task<bool> TagExists(string tag)
    {
        Authenticate();

        try
        {
            _ = await _client.Git.Reference.Get(owner, repo, $"tags/{tag}");
            return true;
        }
        catch (NotFoundException)
        {
            return false;
        }
    }

    public async Task<Release> AddRelease(string version, string body)
    {
        Authenticate();

        var preTags = new[] { "alpha", "beta" };
        var isPrerelease = preTags.Any(t => version.Contains(t, StringComparison.OrdinalIgnoreCase));

        NewRelease release = new(version)
        {
            Name = version,
            Body = body,
            Prerelease = isPrerelease,
        };

        return await _client.Repository.Release.Create(owner, repo, release);
    }

    public async Task AddReleaseAssets(Release release, string[] files)
    {
        Authenticate();

        foreach (var file in files)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException($"Release asset not found: {file}", file);

            var name = Path.GetFileName(file);
            var mime = "application/octet-stream";
            using var stream = File.OpenRead(file);
            ReleaseAssetUpload uploadData = new(name, mime, stream, null);
            _ = await _client.Repository.Release.UploadAsset(release, uploadData);
        }
    }

    void Authenticate()
    {
        if (_authenticated)
            return;

        _client.Credentials = new(GetSecret("GITHUB_TOKEN"));
        _authenticated = true;
    }
}
