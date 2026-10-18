using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Models;
using GameSaveCenter.Core.Services;

namespace GameSaveCenter.Playnite.Infrastructure;

public sealed class BackupLineageDisplay
{
    public string Summary { get; set; } = string.Empty;
    public IReadOnlyList<string> Items { get; set; } = Array.Empty<string>();
}

/// <summary>Creates a chronological, text-first detail list from the bounded Core lineage model.</summary>
public static class BackupLineageDisplayBuilder
{
    private const int MaximumDisplayedRelatedVersions = 12;

    public static BackupLineageDisplay Build(
        BackupVersionDto? selected,
        IReadOnlyList<BackupVersionDto>? loadedVersions,
        int totalVersionCount)
    {
        if (selected == null)
        {
            return new BackupLineageDisplay { Summary = "选择一个版本后查看已知来源、等价内容和后续分叉。" };
        }

        var loaded = loadedVersions ?? Array.Empty<BackupVersionDto>();
        var selectedId = selected.BackupId?.Trim() ?? string.Empty;
        var selectedPosition = -1;
        for (var index = 0; index < loaded.Count; index++)
        {
            if (string.Equals(loaded[index].BackupId, selectedId, StringComparison.OrdinalIgnoreCase))
            {
                selectedPosition = index;
                break;
            }
        }
        var contextStart = selectedPosition < 0
            ? 0
            : Math.Max(0, selectedPosition - (BackupLineageBuilder.MaximumNodeCount / 2));
        var contextCount = Math.Min(BackupLineageBuilder.MaximumNodeCount, loaded.Count - contextStart);
        var context = new List<BackupVersionDto>(contextCount);
        for (var index = contextStart; index < contextStart + contextCount; index++)
        {
            context.Add(loaded[index]);
        }
        var contextIsPartial = contextStart > 0
            || context.Count < loaded.Count
            || loaded.Count < Math.Max(0, totalVersionCount);

        var model = new BackupLineageBuilder().Build(context.Select(item => new BackupSnapshot
        {
            BackupId = item.BackupId,
            ParentBackupId = item.ParentBackupId,
            CreatedUtc = item.CreatedUtc,
            SourceDevice = item.SourceDevice,
            TotalBytes = item.TotalBytes,
            FileCount = item.FileCount,
            ContentFingerprint = item.ContentFingerprint
        }));

        var selectedNodes = model.Nodes
            .Where(node => string.Equals(node.BackupId, selectedId, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (selectedNodes.Count != 1)
        {
            return new BackupLineageDisplay
            {
                Summary = contextIsPartial
                    ? "所选版本尚未进入当前关系分析范围；加载更多历史后可继续核对。"
                    : "当前关系分析无法唯一识别所选版本。",
                Items = string.IsNullOrWhiteSpace(selected.ParentBackupId)
                    ? new[] { "来源未知：该版本没有记录父版本 ID。" }
                    : new[] { $"记录的父版本 ID：{selected.ParentBackupId}；当前范围无法验证该版本。" }
            };
        }

        var selectedNode = selectedNodes[0];
        var selectedIndex = selectedNode.Index;
        var related = new List<RelatedVersion>();
        var notes = new List<string>();

        if (!string.IsNullOrWhiteSpace(selectedNode.ParentBackupId))
        {
            var parents = model.Nodes.Where(node =>
                string.Equals(node.BackupId, selectedNode.ParentBackupId, StringComparison.OrdinalIgnoreCase)).ToList();
            if (parents.Count == 1)
            {
                related.Add(CreateRelated("来源父版本", parents[0]));
            }
            else if (parents.Count > 1)
            {
                notes.Add($"父版本 ID“{selectedNode.ParentBackupId}”重复，无法唯一确认来源。" );
            }
            else
            {
                notes.Add(contextIsPartial
                    ? $"记录的父版本 ID“{selectedNode.ParentBackupId}”不在当前载入的关系范围；不能据此判断版本缺失。"
                    : $"记录的父版本 ID“{selectedNode.ParentBackupId}”未出现在当前完整索引中。" );
            }
        }
        else
        {
            notes.Add("来源未知：未记录父版本 ID，无法区分初始版本与未采集的历史来源。" );
        }

        var children = model.Relations
            .Where(relation => relation.Kind == BackupLineageRelationKind.Parent
                && relation.FromNodeIndex == selectedIndex
                && relation.ToNodeIndex != selectedIndex)
            .Select(relation => model.Nodes[relation.ToNodeIndex])
            .ToList();
        var childLabel = children.Count > 1 ? "分叉后续版本" : "后续版本";
        related.AddRange(children.Select(node => CreateRelated(childLabel, node)));

        if (!string.IsNullOrWhiteSpace(selectedNode.ContentFingerprint))
        {
            related.AddRange(model.Nodes
                .Where(node => node.Index != selectedIndex
                    && string.Equals(node.ContentFingerprint, selectedNode.ContentFingerprint, StringComparison.OrdinalIgnoreCase))
                .Select(node => CreateRelated("内容相同", node)));
        }
        else
        {
            notes.Add("内容关系未知：当前版本没有可比较的完整清单指纹。" );
        }

        var cycle = model.Issues.Any(issue => issue.Kind == BackupLineageIssueKind.ParentCycle
            && issue.NodeIndices.Contains(selectedIndex));
        if (cycle)
        {
            notes.Add("检测到父版本循环引用；来源链不完整。" );
        }

        var ordered = related
            .GroupBy(item => item.Kind + "\0" + item.BackupId, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(item => item.CreatedUtc)
            .ThenBy(item => item.BackupId, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var visible = ordered.Take(MaximumDisplayedRelatedVersions)
            .Select(item => $"{item.Kind} · {item.CreatedUtc.ToLocalTime():yyyy-MM-dd HH:mm} · {item.BackupId} · {item.Device}")
            .ToList();
        if (ordered.Count > visible.Count)
        {
            visible.Add($"还有 {ordered.Count - visible.Count} 个关联版本未展开。" );
        }

        visible.AddRange(notes.Distinct(StringComparer.Ordinal));
        if (contextIsPartial)
        {
            notes.Add("关系仅根据当前已载入的版本分析；未载入范围不参与等价内容和分叉判断。" );
            if (!visible.Contains(notes[notes.Count - 1], StringComparer.Ordinal))
            {
                visible.Add(notes[notes.Count - 1]);
            }
        }

        if (visible.Count == 0)
        {
            visible.Add("当前已载入范围内没有可确认的父版本、等价内容或后续分叉。" );
        }

        return new BackupLineageDisplay
        {
            Summary = children.Count > 1
                ? $"检测到 {children.Count} 个直接后续版本，形成分叉。"
                : contextIsPartial
                    ? "以下关系按时间排列；尚未载入的版本关系保持未知。"
                    : "以下关系按时间排列；未记录或无法验证的关系会明确说明。",
            Items = visible
        };
    }

    private static RelatedVersion CreateRelated(string kind, BackupLineageNode node)
        => new RelatedVersion(kind, node.BackupId, node.CreatedUtc,
            string.IsNullOrWhiteSpace(node.SourceDevice) ? "设备未记录" : node.SourceDevice);

    private sealed class RelatedVersion
    {
        public RelatedVersion(string kind, string backupId, DateTime createdUtc, string device)
        {
            Kind = kind;
            BackupId = backupId;
            CreatedUtc = createdUtc;
            Device = device;
        }

        public string Kind { get; }
        public string BackupId { get; }
        public DateTime CreatedUtc { get; }
        public string Device { get; }
    }
}
