using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameSaveCenter.Contracts;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>Converts mutable Playnite database objects into Worker-safe descriptors.</summary>
    public sealed class PlayniteGameAdapter
    {
        private readonly IPlayniteAPI api;
        public PlayniteGameAdapter(IPlayniteAPI api) { this.api = api; }

        public GameDescriptorDto Convert(Game game)
        {
            var installDirectory = game.InstallDirectory ?? string.Empty;
            var playniteIsInstalled = game.IsInstalled;
            var installDirectoryPresent = IsInstallDirectoryPresent(installDirectory);
            var isInstalled = playniteIsInstalled || installDirectoryPresent;
            var installStateSource = playniteIsInstalled
                ? GameInstallStateSources.PlayniteFlag
                : installDirectoryPresent
                    ? GameInstallStateSources.InstallDirectory
                    : GameInstallStateSources.None;
            var descriptor = new GameDescriptorDto
            {
                PlayniteId = game.Id.ToString("D"),
                Name = game.Name ?? string.Empty,
                Platform = DetectPlatform(game),
                PlatformGameId = game.GameId ?? string.Empty,
                PluginId = game.PluginId.ToString("D"),
                InstallDirectory = installDirectory,
                PlayniteIsInstalled = playniteIsInstalled,
                // Playnite's Steam integration can briefly leave IsInstalled=false while
                // refreshing a profile or after a library is moved to another machine. The
                // install directory is a local, read-only signal and prevents the GameSaveCenter
                // picker default ("已安装") from hiding a game that is actually present.
                IsInstalled = isInstalled,
                InstallStateSource = installStateSource,
                LastPlayedUtc = game.LastActivity,
                Tags = game.Tags == null ? new List<string>() : game.Tags.Select(x => x.Name).Where(x => !string.IsNullOrWhiteSpace(x)).ToList()
            };

            if (game.GameActions != null)
            {
                foreach (var action in game.GameActions)
                {
                    var expanded = SafeExpand(game, action);
                    descriptor.Actions.Add(new GameActionDto
                    {
                        Name = expanded.Name ?? string.Empty,
                        Path = expanded.Path ?? string.Empty,
                        Arguments = expanded.Arguments ?? string.Empty,
                        WorkingDirectory = expanded.WorkingDir ?? string.Empty,
                        IsPlayAction = expanded.IsPlayAction,
                        IsModLoader = LooksLikeModLoader(expanded.Name, expanded.Path)
                    });
                    AddProcessName(descriptor, expanded.Path);
                    AddProcessName(descriptor, expanded.TrackingPath);
                    // Steam/launcher integrations can leave InstallDirectory empty or stale
                    // while their playable action still points to the local executable. Treat
                    // an existing local play action as a second read-only installation signal.
                    if (!isInstalled && expanded.IsPlayAction && IsLocalPathPresent(expanded.Path, expanded.WorkingDir))
                    {
                        isInstalled = true;
                        installStateSource = GameInstallStateSources.PlayAction;
                    }
                }
            }
            descriptor.IsInstalled = isInstalled;
            descriptor.InstallStateSource = installStateSource;

            // Many library plugins don't expose their primary action in GameActions.
            // Top-level executables are useful fallback candidates but remain user-reviewable.
            if (isInstalled && IsInstallDirectoryPresent(installDirectory))
            {
                try
                {
                    foreach (var executable in Directory.EnumerateFiles(Environment.ExpandEnvironmentVariables(installDirectory), "*.exe", SearchOption.TopDirectoryOnly).Take(20))
                        AddProcessName(descriptor, executable);
                }
                catch { }
            }
            return descriptor;
        }

        public GameActionDto ConvertSourceAction(Game game, GameAction action)
        {
            var expanded = SafeExpand(game, action);
            return new GameActionDto
            {
                Name = expanded.Name ?? string.Empty,
                Path = expanded.Path ?? string.Empty,
                Arguments = expanded.Arguments ?? string.Empty,
                WorkingDirectory = expanded.WorkingDir ?? string.Empty,
                IsPlayAction = expanded.IsPlayAction,
                IsModLoader = LooksLikeModLoader(expanded.Name, expanded.Path)
            };
        }

        private GameAction SafeExpand(Game game, GameAction action)
        {
            try { return api.ExpandGameVariables(game, action); }
            catch { return action; }
        }

        private static void AddProcessName(GameDescriptorDto descriptor, string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            var name = Path.GetFileNameWithoutExtension(path!.Trim('"'));
            if (!string.IsNullOrWhiteSpace(name) && !descriptor.KnownProcessNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                descriptor.KnownProcessNames.Add(name);
        }

        private static bool LooksLikeModLoader(string? name, string? path)
        {
            var value = ((name ?? string.Empty) + " " + (path ?? string.Empty)).ToLowerInvariant();
            return new[] { "mod", "skse", "smapi", "f4se", "nvse", "mo2", "modorganizer", "vortex", "frosty", "reloaded", "r2modman", "thunderstore" }.Any(value.Contains);
        }

        internal static bool IsInstallDirectoryPresent(string? installDirectory)
        {
            if (string.IsNullOrWhiteSpace(installDirectory)) return false;
            try { return Directory.Exists(Environment.ExpandEnvironmentVariables(installDirectory)); }
            catch (Exception ex) when (!(ex is OutOfMemoryException) && !(ex is StackOverflowException)) { return false; }
        }

        internal static bool IsLocalPathPresent(string? path, string? workingDirectory)
        {
            return IsLocalFileOrDirectory(path) || IsInstallDirectoryPresent(workingDirectory);
        }

        private static bool IsLocalFileOrDirectory(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            try
            {
                var expanded = Environment.ExpandEnvironmentVariables(path!.Trim().Trim('"'));
                if (Uri.TryCreate(expanded, UriKind.Absolute, out var uri) && !uri.IsFile)
                    return false;
                return File.Exists(expanded) || Directory.Exists(expanded);
            }
            catch (Exception ex) when (!(ex is OutOfMemoryException) && !(ex is StackOverflowException)) { return false; }
        }

        private static GamePlatformKind DetectPlatform(Game game)
        {
            var text = string.Join(" ", new[]
            {
                game.Source == null ? string.Empty : game.Source.Name,
                game.Platforms == null ? string.Empty : string.Join(" ", game.Platforms.Select(x => x.Name))
            }).ToLowerInvariant();
            if (text.Contains("steam")) return GamePlatformKind.Steam;
            if (text.Contains("xbox") || text.Contains("game pass") || text.Contains("microsoft")) return GamePlatformKind.Xbox;
            if (text.Contains("epic")) return GamePlatformKind.Epic;
            if (text.Contains("ubisoft") || text.Contains("uplay")) return GamePlatformKind.Ubisoft;
            if (text.Contains("ea") || text.Contains("origin")) return GamePlatformKind.Ea;
            if (text.Contains("gog") || text.Contains("galaxy")) return GamePlatformKind.Gog;
            return GamePlatformKind.Other;
        }
    }
}
