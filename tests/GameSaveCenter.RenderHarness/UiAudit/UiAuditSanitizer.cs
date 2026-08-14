using System;
using System.Text.RegularExpressions;

namespace GameSaveCenter.RenderHarness.UiAudit;

public static class UiAuditSanitizer
{
    private static readonly Regex UserProfileRegex = new Regex(
        @"[A-Za-z]:\\Users\\(?<user>[^\\\""]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string Sanitize(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrEmpty(userProfile))
        {
            value = Regex.Replace(
                value,
                Regex.Escape(userProfile),
                "%USERPROFILE%",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        var tempPath = Environment.GetEnvironmentVariable("TEMP");
        if (!string.IsNullOrEmpty(tempPath))
        {
            value = Regex.Replace(
                value,
                Regex.Escape(tempPath),
                "%TEMP%",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        value = UserProfileRegex.Replace(value, "%USERPROFILE%\\");
        return value;
    }

    public static string SanitizeJson(string json)
        => Sanitize(json);

    public static string SanitizeMarkdown(string markdown)
        => Sanitize(markdown);
}
