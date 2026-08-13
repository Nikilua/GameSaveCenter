using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Core.Models;

namespace GameSaveCenter.Core.Services
{
    /// <summary>Compares two file manifests using path, size, write time and hash.</summary>
    public sealed class FileManifestDiffService
    {
        public FileManifestDiff Compare(IEnumerable<FileManifestEntry> before, IEnumerable<FileManifestEntry> after)
        {
            var result = new FileManifestDiff
            {
                IsExactComparison = true
            };
            var oldMap = BuildMap(before, "旧版本", result);
            var newMap = BuildMap(after, "新版本", result);
            result.BeforeTotalBytes = oldMap.Values.Sum(x => Math.Max(0, x.SizeBytes));
            result.AfterTotalBytes = newMap.Values.Sum(x => Math.Max(0, x.SizeBytes));
            result.IsExactComparison = result.IsValid
                && oldMap.Values.Concat(newMap.Values).All(x => !string.IsNullOrWhiteSpace(x.Sha256));

            foreach (var item in newMap)
            {
                if (!oldMap.TryGetValue(item.Key, out var oldEntry))
                {
                    result.Added.Add(item.Value);
                    continue;
                }

                if (IsEquivalent(oldEntry, item.Value)) result.Unchanged.Add(item.Value);
                else result.Modified.Add(item.Value);
            }

            foreach (var item in oldMap)
            {
                if (!newMap.ContainsKey(item.Key)) result.Removed.Add(item.Value);
            }

            return result;
        }

        private static Dictionary<string, FileManifestEntry> BuildMap(IEnumerable<FileManifestEntry> entries, string label, FileManifestDiff result)
        {
            var map = new Dictionary<string, FileManifestEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in entries ?? Enumerable.Empty<FileManifestEntry>())
            {
                var path = NormalizePath(entry.RelativePath);
                if (string.IsNullOrWhiteSpace(path))
                {
                    result.IsValid = false;
                    result.Warnings.Add($"{label}包含空路径条目。");
                    continue;
                }
                if (map.ContainsKey(path))
                {
                    result.IsValid = false;
                    result.Warnings.Add($"{label}包含重复路径：{path}。");
                    continue;
                }
                map[path] = entry;
            }
            return map;
        }

        private static bool IsEquivalent(FileManifestEntry left, FileManifestEntry right)
        {
            if (left.SizeBytes != right.SizeBytes) return false;
            if (!string.IsNullOrWhiteSpace(left.Sha256) && !string.IsNullOrWhiteSpace(right.Sha256))
            {
                return string.Equals(left.Sha256, right.Sha256, StringComparison.OrdinalIgnoreCase);
            }

            return left.LastWriteUtc == right.LastWriteUtc;
        }

        private static string NormalizePath(string path) => (path ?? string.Empty).Replace('/', '\\').TrimStart('\\');
    }
}
