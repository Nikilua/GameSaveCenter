using System;
using System.Text.RegularExpressions;

namespace GameSaveCenter.Contracts;

/// <summary>
/// Builds a display-only remote object path without allowing credentials to cross into UI text.
/// The returned value is suitable for diagnostics and user-visible status; it is not used for
/// an rclone invocation.
/// </summary>
public static class CloudRemoteDisplay
{
    private static readonly Regex KeyValueSecret = new Regex(
        @"(?ix)\b(password|passwd|pwd|token|secret|client_secret|access_token|refresh_token|api[_-]?key|authorization|credential|private_key)\b\s*([=:])\s*(""[^""]*""|'[^']*'|Bearer\s+[^\s,;&]+|[^\s,;&]+)",
        RegexOptions.Compiled);
    private static readonly Regex BearerSecret = new Regex(@"(?i)\bBearer\s+[^\s,;]+", RegexOptions.Compiled);
    private static readonly Regex UriCredential = new Regex(@"(?i)(https?://)[^\s/@]+:[^\s/@]+@", RegexOptions.Compiled);

    public static string Combine(string root, string relativePath)
    {
        var safeRoot = Redact(root);
        if (string.IsNullOrWhiteSpace(safeRoot)) return string.Empty;

        var relative = (relativePath ?? string.Empty).Replace('\\', '/').Trim('/');
        if (relative.Length == 0) return safeRoot;

        var separator = safeRoot.EndsWith(":", StringComparison.Ordinal)
            || safeRoot.EndsWith("/", StringComparison.Ordinal)
            ? string.Empty
            : "/";
        return safeRoot + separator + relative;
    }

    public static string Redact(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var sanitized = UriCredential.Replace(value, "$1[已隐藏]@");
        sanitized = KeyValueSecret.Replace(sanitized, match => match.Groups[1].Value + match.Groups[2].Value + "[已隐藏]");
        return BearerSecret.Replace(sanitized, "Bearer [已隐藏]");
    }
}
