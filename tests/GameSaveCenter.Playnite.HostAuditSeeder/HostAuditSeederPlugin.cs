using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

namespace GameSaveCenter.Playnite.HostAuditSeeder;

public sealed class HostAuditSeederPlugin : GenericPlugin
{
    private static readonly Guid PluginId = Guid.Parse("9466c3cb-4c5d-4909-8334-21c608eeb309");
    private const string ManifestFileName = "synthetic-library-manifest.json";

    public HostAuditSeederPlugin(IPlayniteAPI api) : base(api)
    {
        Properties = new GenericPluginProperties { HasSettings = false };
    }

    public override Guid Id => PluginId;

    public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
    {
        var directory = Environment.GetEnvironmentVariable("GSC_UI_AUDIT_SEED_DIRECTORY");
        var runId = Environment.GetEnvironmentVariable("GSC_UI_AUDIT_SEED_RUN_ID");
        if (string.IsNullOrWhiteSpace(directory) || !Guid.TryParse(runId, out _))
        {
            return;
        }

        var countText = Environment.GetEnvironmentVariable("GSC_UI_AUDIT_SEED_COUNT");
        var count = int.TryParse(countText, NumberStyles.None, CultureInfo.InvariantCulture, out var parsedCount)
            ? parsedCount
            : 64;
        var manifestPath = Path.Combine(directory, ManifestFileName);

        try
        {
            Directory.CreateDirectory(directory);
            var catalog = SyntheticGameCatalog.Create(count);
            var before = PlayniteApi.Database.Games.ToList();
            var knownIds = new HashSet<string>(before.Select(game => game.GameId).Where(value => !string.IsNullOrWhiteSpace(value))!, StringComparer.OrdinalIgnoreCase);
            var knownNames = new HashSet<string>(before.Select(game => game.Name).Where(value => !string.IsNullOrWhiteSpace(value))!, StringComparer.OrdinalIgnoreCase);
            var addedCount = 0;

            foreach (var metadata in catalog)
            {
                if (knownIds.Contains(metadata.GameId) || knownNames.Contains(metadata.Name))
                {
                    continue;
                }

                PlayniteApi.Database.ImportGame(metadata);
                knownIds.Add(metadata.GameId);
                knownNames.Add(metadata.Name);
                addedCount++;
            }

            var after = PlayniteApi.Database.Games.ToList();
            var presentIds = new HashSet<string>(after.Select(game => game.GameId).Where(value => !string.IsNullOrWhiteSpace(value))!, StringComparer.OrdinalIgnoreCase);
            var presentNames = new HashSet<string>(after.Select(game => game.Name).Where(value => !string.IsNullOrWhiteSpace(value))!, StringComparer.OrdinalIgnoreCase);
            var presentCount = catalog.Count(item => presentIds.Contains(item.GameId) || presentNames.Contains(item.Name));
            WriteManifest(manifestPath, new SeedManifest
            {
                Status = presentCount == catalog.Count ? "seeded" : "partial",
                RunId = runId!,
                RequestedCount = catalog.Count,
                PresentCount = presentCount,
                AddedCount = addedCount,
                GameIds = catalog.Where(item => presentIds.Contains(item.GameId) || presentNames.Contains(item.Name))
                    .Select(item => item.GameId).ToArray(),
                AssemblyIdentity = typeof(HostAuditSeederPlugin).Assembly
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown",
                CompletedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
            });
        }
        catch (Exception exception)
        {
            try
            {
                Directory.CreateDirectory(directory);
                WriteManifest(manifestPath, new SeedManifest
                {
                    Status = "failed",
                    RunId = runId!,
                    RequestedCount = count,
                    ErrorType = exception.GetType().FullName ?? exception.GetType().Name,
                    AssemblyIdentity = typeof(HostAuditSeederPlugin).Assembly
                        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown",
                    CompletedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
                });
            }
            catch
            {
                // The runner treats a missing marker as not observed; this plugin never
                // turns an audit-only seed failure into a Playnite startup failure.
            }
        }
    }

    private static void WriteManifest(string path, SeedManifest manifest)
    {
        var json = new StringBuilder();
        json.Append('{')
            .Append("\"status\":\"").Append(Escape(manifest.Status)).Append("\",")
            .Append("\"runId\":\"").Append(Escape(manifest.RunId)).Append("\",")
            .Append("\"requestedCount\":").Append(manifest.RequestedCount).Append(',')
            .Append("\"presentCount\":").Append(manifest.PresentCount).Append(',')
            .Append("\"addedCount\":").Append(manifest.AddedCount).Append(',')
            .Append("\"assemblyIdentity\":\"").Append(Escape(manifest.AssemblyIdentity)).Append("\",")
            .Append("\"completedUtc\":\"").Append(Escape(manifest.CompletedUtc)).Append("\",")
            .Append("\"errorType\":\"").Append(Escape(manifest.ErrorType)).Append("\",")
            .Append("\"gameIds\":[")
            .Append(string.Join(",", manifest.GameIds.Select(id => "\"" + Escape(id) + "\"")))
            .Append("]}");
        File.WriteAllText(path, json.ToString(), new UTF8Encoding(false));
    }

    private static string Escape(string? value)
        => (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"")
            .Replace("\r", "\\r").Replace("\n", "\\n");

    private sealed class SeedManifest
    {
        public string Status { get; set; } = string.Empty;
        public string RunId { get; set; } = string.Empty;
        public int RequestedCount { get; set; }
        public int PresentCount { get; set; }
        public int AddedCount { get; set; }
        public string[] GameIds { get; set; } = Array.Empty<string>();
        public string AssemblyIdentity { get; set; } = string.Empty;
        public string CompletedUtc { get; set; } = string.Empty;
        public string ErrorType { get; set; } = string.Empty;
    }
}
