using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Models;

namespace GameSaveCenter.Playnite.Infrastructure
{
    public sealed class BackupLineageGraphNode
    {
        internal BackupLineageGraphNode(BackupVersionDto backup) => Backup = backup;
        public BackupVersionDto Backup { get; }
        public List<BackupLineageGraphNode> Children { get; } = new List<BackupLineageGraphNode>();
        public bool HasParent { get; internal set; }
    }

    internal sealed class BackupLineageGraph
    {
        internal IReadOnlyList<BackupLineageGraphNode> Roots { get; set; } = Array.Empty<BackupLineageGraphNode>();
        internal BackupLineageGraphNode? Selected { get; set; }
        internal string Summary { get; set; } = string.Empty;
    }

    internal static class BackupLineageGraphBuilder
    {
        private const int MaximumNodes = 80;

        internal static BackupLineageGraph Build(BackupVersionDto? selected, IEnumerable<BackupVersionDto> loaded, int total)
        {
            if (selected == null) return new BackupLineageGraph { Summary = "请选择存档版本" };
            var versions = new[] { selected }.Concat(loaded)
                .Where(item => string.Equals(item.PlayniteId, selected.PlayniteId, StringComparison.OrdinalIgnoreCase))
                .GroupBy(item => item.BackupId, StringComparer.Ordinal).Select(group => group.First())
                .OrderByDescending(item => item.CreatedUtc).ThenBy(item => item.BackupId, StringComparer.Ordinal)
                .ToList();
            var selection = versions.IndexOf(selected);
            var start = Math.Max(0, Math.Min(selection - MaximumNodes / 2, versions.Count - MaximumNodes));
            var context = versions.Skip(start).Take(MaximumNodes).ToArray();
            var model = BackupLineageDisplayBuilder.CreateLineageModel(context);
            var nodes = context.Select(version => new BackupLineageGraphNode(version)).ToArray();
            var circular = new HashSet<int>(model.Issues.Where(issue => issue.Kind == BackupLineageIssueKind.ParentCycle)
                .SelectMany(issue => issue.NodeIndices));
            var parentByChild = model.Relations.Where(relation => relation.Kind == BackupLineageRelationKind.Parent
                    && !(circular.Contains(relation.FromNodeIndex) && circular.Contains(relation.ToNodeIndex)))
                .ToDictionary(relation => relation.ToNodeIndex, relation => relation.FromNodeIndex);
            foreach (var relation in parentByChild.OrderBy(pair => context[pair.Key].CreatedUtc).ThenBy(pair => pair.Key))
            {
                nodes[relation.Value].Children.Add(nodes[relation.Key]);
                nodes[relation.Key].HasParent = true;
            }
            var selectedIndex = selection - start;
            var root = selectedIndex;
            while (parentByChild.TryGetValue(root, out var parent)) root = parent;

            var messages = new List<string>();
            if (context.Length < versions.Count || versions.Count < total) messages.Add("历史范围不完整，关系可能还有后续");
            if (model.Issues.Any(issue => issue.Kind == BackupLineageIssueKind.ParentNotFound)) messages.Add("有父版本未出现在当前范围");
            if (model.Issues.Any(issue => issue.Kind == BackupLineageIssueKind.DuplicateBackupId
                || issue.Kind == BackupLineageIssueKind.AmbiguousParentReference)) messages.Add("有版本标识歧义，相关连接已省略");
            if (circular.Count > 0) messages.Add("循环关系已断开");
            if (messages.Count == 0 && !nodes[root].HasParent && string.IsNullOrWhiteSpace(context[root].ParentBackupId))
                messages.Add("当前分支起点未记录父版本");
            return new BackupLineageGraph
            {
                Roots = new[] { nodes[root] },
                Selected = nodes[selectedIndex],
                Summary = string.Join("；", messages)
            };
        }
    }
}
