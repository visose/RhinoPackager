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

        _client = new GitHubClient(new ProductHeaderValue(_owner))
        {
            Credentials = new Credentials(GetSecret("GITHUB_TOKEN"))
        };
    }

    public async Task<string?> GetLatestVersionAsync()
    {
        try
        {
            var latest = await _client.Repository.Release.GetLatest(_owner, _repo);
            return latest.TagName;
        }
        catch (NotFoundException)
        {
            Log($"No non-preview releases found in this repo.");
            return null;
        }
    }

    public async Task<Release> AddReleaseAsync(string version, string body)
    {
        var release = new NewRelease(version)
        {
            Name = version,
            Body = body,
            Prerelease = false,
        };

        return await _client.Repository.Release.Create(_owner, _repo, release);
    }
}
