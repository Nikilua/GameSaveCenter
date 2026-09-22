using System;
using System.Text.RegularExpressions;

namespace GameSaveCenter.Contracts
{
    /// <summary>Shared allowlist-safe redaction for user-visible and copied diagnostics.</summary>
    public static class ClipboardTextSanitizer
    {
        private static readonly Regex KeyValueSecret = new Regex(
            @"(?ix)\b(password|passwd|pwd|token|secret|client_secret|access_token|refresh_token|api[_-]?key|authorization|credential|private_key)\b\s*([=:])\s*(""[^""]*""|'[^']*'|Bearer\s+[^\s,;&]+|[^\s,;&]+)",
            RegexOptions.Compiled);
        private static readonly Regex BearerSecret = new Regex(@"(?i)\bBearer\s+[^\s,;]+", RegexOptions.Compiled);
        private static readonly Regex UriCredential = new Regex(@"(?i)(https?://)[^\s/@]+:[^\s/@]+@", RegexOptions.Compiled);

        public static string Sanitize(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            var sanitized = UriCredential.Replace(value, "$1[已隐藏]@");
            sanitized = KeyValueSecret.Replace(sanitized, match => match.Groups[1].Value + match.Groups[2].Value + "[已隐藏]");
            return BearerSecret.Replace(sanitized, "Bearer [已隐藏]");
        }
    }
}
