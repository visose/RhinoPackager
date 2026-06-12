using System.Diagnostics;

namespace RhinoPackager;

public static class ProcessRunner
{
    public static int Run(
        string executable,
        IReadOnlyList<string> arguments,
        string? workingDirectory = null,
        IEnumerable<string>? redacted = null)
    {
        var secrets = GetSecrets(redacted);
        using var process = CreateProcess(executable, arguments, workingDirectory);
        var command = FormatCommand(executable, arguments, secrets);

        Util.Log($"> {command}");
        process.OutputDataReceived += (_, line) => LogLine(line.Data, secrets);
        process.ErrorDataReceived += (_, line) => LogLine(line.Data, secrets);

        _ = process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"Command failed ({process.ExitCode}): {command}");

        return process.ExitCode;
    }

    public static string Capture(
        string executable,
        IReadOnlyList<string> arguments,
        string? workingDirectory = null,
        IEnumerable<string>? redacted = null)
    {
        var secrets = GetSecrets(redacted);
        using var process = CreateProcess(executable, arguments, workingDirectory);
        var command = FormatCommand(executable, arguments, secrets);

        _ = process.Start();
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            var message = Redact(error.Trim(), secrets);
            throw new InvalidOperationException($"Command failed ({process.ExitCode}): {command}{Environment.NewLine}{message}");
        }

        return Redact(output.Trim(), secrets);
    }

    public static string FormatCommand(string executable, IReadOnlyList<string> arguments, IEnumerable<string>? redacted = null)
    {
        var command = string.Join(" ", [executable, .. arguments.Select(Quote)]);
        return Redact(command, GetSecrets(redacted));
    }

    static Process CreateProcess(string executable, IReadOnlyList<string> arguments, string? workingDirectory)
    {
        Process process = new()
        {
            StartInfo = new()
            {
                FileName = executable,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }
        };

        if (workingDirectory is not null)
            process.StartInfo.WorkingDirectory = workingDirectory;

        foreach (var argument in arguments)
            process.StartInfo.ArgumentList.Add(argument);

        return process;
    }

    static string[] GetSecrets(IEnumerable<string>? redacted) =>
        redacted?
            .Where(secret => !string.IsNullOrEmpty(secret))
            .Distinct(StringComparer.Ordinal)
            .ToArray()
        ?? [];

    static void LogLine(string? line, string[] secrets)
    {
        if (line is not null)
            Util.Log(Redact(line, secrets));
    }

    static string Redact(string text, string[] secrets)
    {
        foreach (var secret in secrets)
            text = text.Replace(secret, "...", StringComparison.Ordinal);

        return text;
    }

    static string Quote(string value) =>
        value.Contains(' ', StringComparison.Ordinal) ? $"\"{value}\"" : value;
}
