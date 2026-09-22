using System;
using System.Text.RegularExpressions;

namespace GameSaveCenter.Contracts
{
    /// <summary>Identity supplied by the Playnite client when a maintenance report is requested.</summary>
    public sealed class MaintenanceReportRequestDto
    {
        public string PluginVersion { get; set; } = string.Empty;
        public string PluginBuildIdentity { get; set; } = string.Empty;
        public string PlayniteVersion { get; set; } = string.Empty;
    }

    /// <summary>Removes credentials, URL parameters and Windows user names from report text.</summary>
    public static class MaintenanceReportRedactor
    {
        private static readonly Regex UrlParameters = new Regex(
            @"(?<base>https?://[^\s?#]+)(?:[?#][^\s]*)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex WindowsUserPath = new Regex(
            @"(?<root>[A-Za-z]:[\\/]Users[\\/]+)[^\\/\s:]+",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string Redact(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            var sanitized = CloudRemoteDisplay.Redact(value);
            sanitized = UrlParameters.Replace(sanitized, match => match.Groups["base"].Value + "?[参数已隐藏]");
            return WindowsUserPath.Replace(sanitized, match => match.Groups["root"].Value + "[用户]");
        }
    }

    /// <summary>User-readable maintenance health report. This is not the diagnostics ZIP.</summary>
    public sealed class MaintenanceReportDto
    {
        public DateTime GeneratedUtc { get; set; } = DateTime.UtcNow;
        public string Summary { get; set; } = string.Empty;
        public string ReportText { get; set; } = string.Empty;
    }
}
