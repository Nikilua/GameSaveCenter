using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Core.Models;

namespace GameSaveCenter.Core.Services
{
    /// <summary>Builds bounded, evidence-based relationships between backup summaries.</summary>
    public sealed class BackupLineageBuilder
    {
        public const int MaximumNodeCount = 500;

        public BackupLineageModel Build(
            IEnumerable<BackupSnapshot>? snapshots,
            int inputItemLimit = MaximumNodeCount)
        {
            if (inputItemLimit < 1 || inputItemLimit > MaximumNodeCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(inputItemLimit),
                    $"Input item limit must be between 1 and {MaximumNodeCount}.");
            }

            var model = new BackupLineageModel { InputItemLimit = inputItemLimit };
            var source = (snapshots ?? Enumerable.Empty<BackupSnapshot>()).GetEnumerator();
            var examined = 0;
            try
            {
                while (examined < inputItemLimit && source.MoveNext())
                {
                    examined++;
                    var snapshot = source.Current;
                    if (snapshot == null)
                    {
                        continue;
                    }

                    model.Nodes.Add(new BackupLineageNode
                    {
                        Index = model.Nodes.Count,
                        BackupId = snapshot.BackupId?.Trim() ?? string.Empty,
                        ParentBackupId = snapshot.ParentBackupId?.Trim() ?? string.Empty,
                        CreatedUtc = snapshot.CreatedUtc,
                        SourceDevice = snapshot.SourceDevice?.Trim() ?? string.Empty,
                        TotalBytes = snapshot.TotalBytes,
                        FileCount = snapshot.FileCount,
                        ContentFingerprint = snapshot.ContentFingerprint?.Trim() ?? string.Empty
                    });
                }

                // One look-ahead item is used only to distinguish an exact-limit result from truncation.
                model.InputTruncated = examined == inputItemLimit && source.MoveNext();
            }
            finally
            {
                source.Dispose();
            }

            var idIndex = BuildIdIndex(model);
            var parentByChild = BuildParentRelations(model, idIndex);
            BuildContentRelations(model);
            FindParentCycles(model, parentByChild);
            return model;
        }

        private static Dictionary<string, int> BuildIdIndex(BackupLineageModel model)
        {
            var groups = model.Nodes
                .Where(node => !string.IsNullOrWhiteSpace(node.BackupId))
                .GroupBy(node => node.BackupId, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var uniqueIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var group in groups)
            {
                var indices = group.Select(node => node.Index).ToList();
                if (indices.Count == 1)
                {
                    uniqueIds[group.Key] = indices[0];
                    continue;
                }

                model.Issues.Add(new BackupLineageIssue
                {
                    Kind = BackupLineageIssueKind.DuplicateBackupId,
                    BackupId = group.Key,
                    NodeIndices = indices
                });
            }

            foreach (var node in model.Nodes.Where(node => string.IsNullOrWhiteSpace(node.BackupId)))
            {
                model.Issues.Add(new BackupLineageIssue
                {
                    Kind = BackupLineageIssueKind.MissingBackupId,
                    NodeIndices = new List<int> { node.Index }
                });
            }

            return uniqueIds;
        }

        private static int[] BuildParentRelations(
            BackupLineageModel model,
            IReadOnlyDictionary<string, int> idIndex)
        {
            var duplicateIds = new HashSet<string>(
                model.Issues
                    .Where(issue => issue.Kind == BackupLineageIssueKind.DuplicateBackupId)
                    .Select(issue => issue.BackupId),
                StringComparer.OrdinalIgnoreCase);
            var parentByChild = new int[model.Nodes.Count];
            for (var index = 0; index < parentByChild.Length; index++)
            {
                parentByChild[index] = -1;
            }

            foreach (var node in model.Nodes)
            {
                if (string.IsNullOrWhiteSpace(node.ParentBackupId))
                {
                    model.Issues.Add(new BackupLineageIssue
                    {
                        Kind = BackupLineageIssueKind.UnknownRelationship,
                        BackupId = node.BackupId,
                        NodeIndices = new List<int> { node.Index }
                    });
                    continue;
                }

                if (idIndex.TryGetValue(node.ParentBackupId, out var parentIndex))
                {
                    model.Relations.Add(new BackupLineageRelation
                    {
                        Kind = BackupLineageRelationKind.Parent,
                        FromNodeIndex = parentIndex,
                        ToNodeIndex = node.Index
                    });
                    parentByChild[node.Index] = parentIndex;
                    continue;
                }

                var parentIsAmbiguous = duplicateIds.Contains(node.ParentBackupId);
                model.Issues.Add(new BackupLineageIssue
                {
                    Kind = parentIsAmbiguous
                        ? BackupLineageIssueKind.AmbiguousParentReference
                        : BackupLineageIssueKind.ParentNotFound,
                    BackupId = node.BackupId,
                    RelatedBackupId = node.ParentBackupId,
                    NodeIndices = new List<int> { node.Index }
                });
            }

            return parentByChild;
        }

        private static void BuildContentRelations(BackupLineageModel model)
        {
            var groups = model.Nodes
                .Where(node => !string.IsNullOrWhiteSpace(node.ContentFingerprint))
                .GroupBy(node => node.ContentFingerprint, StringComparer.OrdinalIgnoreCase);

            foreach (var group in groups)
            {
                var indices = group.Select(node => node.Index).OrderBy(index => index).ToList();
                if (indices.Count < 2)
                {
                    continue;
                }

                // A representative star preserves the equivalence group without quadratic pair expansion.
                foreach (var index in indices.Skip(1))
                {
                    model.Relations.Add(new BackupLineageRelation
                    {
                        Kind = BackupLineageRelationKind.EquivalentContent,
                        FromNodeIndex = indices[0],
                        ToNodeIndex = index
                    });
                }
            }
        }

        private static void FindParentCycles(BackupLineageModel model, int[] parents)
        {
            var states = new byte[parents.Length];
            var stack = new List<int>();
            var stackPositions = new Dictionary<int, int>();

            void Visit(int nodeIndex)
            {
                states[nodeIndex] = 1;
                stackPositions[nodeIndex] = stack.Count;
                stack.Add(nodeIndex);

                var parentIndex = parents[nodeIndex];
                if (parentIndex >= 0)
                {
                    if (states[parentIndex] == 0)
                    {
                        Visit(parentIndex);
                    }
                    else if (states[parentIndex] == 1)
                    {
                        var cycle = stack.Skip(stackPositions[parentIndex]).ToList();
                        model.Issues.Add(new BackupLineageIssue
                        {
                            Kind = BackupLineageIssueKind.ParentCycle,
                            BackupId = model.Nodes[parentIndex].BackupId,
                            NodeIndices = cycle
                        });
                    }
                }

                stack.RemoveAt(stack.Count - 1);
                stackPositions.Remove(nodeIndex);
                states[nodeIndex] = 2;
            }

            for (var index = 0; index < parents.Length; index++)
            {
                if (states[index] == 0)
                {
                    Visit(index);
                }
            }
        }
    }
}
