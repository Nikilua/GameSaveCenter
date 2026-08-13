using System.Text.RegularExpressions;

namespace GameSaveCenter.Worker.Infrastructure;

/// <summary>
/// Central redaction used by every diagnostics export. It removes credentials, tokens,
/// API keys, authorization values, URL query secrets, UNC credentials, email addresses
/// and the current user profile path before content reaches a support package.
/// </summary>
public static class DiagnosticRedactor
{
    private static readonly Regex SecretAssignment = new(
        @"(?i)(password|passwd|token|secret|api[_-]?key|access[_-]?token|authorization|credential)[""']?\s*([=:])\s*[""']?[^""'\s,;}]+",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex SecretWhitespace = new(
        @"(?i)\b(password|passwd|token|secret|api[_-]?key|access[_-]?token)\s+[^""'\s,;}]+",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex AuthorizationValue = new(
        @"(?i)(authorization\s*[:=]\s*)(?:bearer\s+)?[^""'\r\n,;}]+",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex UrlQuerySecret = new(
        @"(?i)([?&](?:token|key|secret|auth|password|signature|sig|access[_-]?key|api[_-]?key)=)[^&\s""']+",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex UncCredential = new(
        @"(?i)(\\\\[^\\]+\\\\)[^\\\s@]+(?=@)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex Email = new(
        @"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex UserProfilePath = new(
        @"(?i)([A-Z]:\\Users\\)[^\\]+",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static string Redact(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value ?? string.Empty;
        var text = value;
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrWhiteSpace(home))
            text = text.Replace(home, "<USER>", StringComparison.OrdinalIgnoreCase);
        text = SecretAssignment.Replace(text, "$1$2[REDACTED]");
        text = SecretWhitespace.Replace(text, "$1 [REDACTED]");
        text = AuthorizationValue.Replace(text, "$1[REDACTED]");
        text = UrlQuerySecret.Replace(text, "$1[REDACTED]");
        text = UncCredential.Replace(text, "$1[REDACTED]@");
        text = Email.Replace(text, "[EMAIL_REDACTED]");
        text = UserProfilePath.Replace(text, "$1<USER>");
        return text;
    }
}
