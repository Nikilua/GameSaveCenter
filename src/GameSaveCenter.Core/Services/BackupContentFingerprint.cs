using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using GameSaveCenter.Core.Models;

namespace GameSaveCenter.Core.Services
{
    /// <summary>
    /// Creates strong, deterministic evidence for cross-device comparison.
    /// A fingerprint is intentionally unavailable when any manifest entry lacks a hash;
    /// size and file count alone must never prove that two save contents are equivalent.
    /// </summary>
    public static class BackupContentFingerprint
    {
        public static string Compute(IEnumerable<FileManifestEntry> entries)
        {
            var list = (entries ?? Enumerable.Empty<FileManifestEntry>()).ToList();
            if (list.Count == 0 || list.Any(x => string.IsNullOrWhiteSpace(x.Sha256))) return string.Empty;

            var normalized = list
                .Select(x => new
                {
                    Path = NormalizePath(x.RelativePath),
                    x.SizeBytes,
                    Hash = x.Sha256.Trim().ToUpperInvariant()
                }).ToList();
            if (normalized.Any(x => string.IsNullOrWhiteSpace(x.Path)) ||
                normalized.Select(x => x.Path).Distinct(StringComparer.OrdinalIgnoreCase).Count() != normalized.Count)
            {
                // An invalid manifest must not become strong evidence for device equivalence.
                return string.Empty;
            }

            var canonical = string.Join("\n", normalized
                .OrderBy(x => x.Path, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Path, StringComparer.Ordinal)
                .Select(x => $"{x.Path}\t{x.SizeBytes}\t{x.Hash}"));

            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(canonical)))
                .Replace("-", string.Empty);
        }

        private static string NormalizePath(string path) => (path ?? string.Empty).Replace('\\', '/').Trim('/').ToUpperInvariant();
    }
}
