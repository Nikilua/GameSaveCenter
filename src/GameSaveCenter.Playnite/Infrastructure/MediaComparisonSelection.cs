using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Keeps a small, filter-independent set of media objects selected for visual comparison.
    /// The stored DTOs carry stable MediaIds so later comparison work can revalidate them.
    /// </summary>
    public sealed class MediaComparisonSelection
    {
        public const int MaximumItems = 2;

        public ObservableCollection<MediaItemDto> Items { get; } = new ObservableCollection<MediaItemDto>();

        public string Status { get; private set; } = "选择截图后加入比较；录像和缺失文件不会加入。";

        public string Summary
        {
            get
            {
                if (Items.Count == 0)
                    return "图片比较对象 0/2 · 选择截图后加入";
                if (Items.Count == 1)
                    return $"图片比较对象 1/2 · A：{GetDisplayName(Items[0])} · 再加入一张截图";

                return $"图片比较对象 2/2 · A：{GetDisplayName(Items[0])} · B：{GetDisplayName(Items[1])}";
            }
        }

        public void Add(IEnumerable<MediaItemDto?>? candidates, Func<string, bool>? fileExists = null)
        {
            if (candidates == null)
            {
                Status = "请先在媒体列表中选择一张或多张截图。";
                return;
            }

            var exists = fileExists ?? File.Exists;
            var added = 0;
            var nonImage = 0;
            var missingFile = 0;
            var missingId = 0;
            var duplicate = 0;
            var overCapacity = 0;

            foreach (var item in candidates)
            {
                if (item == null)
                {
                    missingId++;
                    continue;
                }

                if (item.Kind != MediaKind.Screenshot)
                {
                    nonImage++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.MediaId))
                {
                    missingId++;
                    continue;
                }

                if (Items.Any(existing => string.Equals(existing.MediaId, item.MediaId, StringComparison.OrdinalIgnoreCase)))
                {
                    duplicate++;
                    continue;
                }

                if (Items.Count >= MaximumItems)
                {
                    overCapacity++;
                    continue;
                }

                var path = item.ArchivePath;
                if (string.IsNullOrWhiteSpace(path) || !exists(path))
                {
                    missingFile++;
                    continue;
                }

                Items.Add(item);
                added++;
            }

            Status = BuildStatus(added, nonImage, missingFile, missingId, duplicate, overCapacity);
        }

        public bool Remove(string? mediaId)
        {
            if (string.IsNullOrWhiteSpace(mediaId))
                return false;

            var item = Items.FirstOrDefault(candidate =>
                string.Equals(candidate.MediaId, mediaId, StringComparison.OrdinalIgnoreCase));
            if (item == null)
                return false;

            Items.Remove(item);
            Status = $"已移除 {GetDisplayName(item)}；当前保留 {Items.Count}/{MaximumItems} 张图片。";
            return true;
        }

        public void Clear()
        {
            Items.Clear();
            Status = "已清空图片比较对象。";
        }

        private static string BuildStatus(int added, int nonImage, int missingFile, int missingId, int duplicate, int overCapacity)
        {
            var skipped = new List<string>();
            if (nonImage > 0) skipped.Add($"{nonImage} 项不是截图");
            if (missingFile > 0) skipped.Add($"{missingFile} 个截图归档文件不存在");
            if (missingId > 0) skipped.Add($"{missingId} 项没有稳定媒体 ID");
            if (duplicate > 0) skipped.Add($"{duplicate} 项已在比较集合中");
            if (overCapacity > 0) skipped.Add($"{overCapacity} 项超过两张上限");

            if (added == 0 && skipped.Count == 0)
                return "请先在媒体列表中选择一张或多张截图。";

            var result = added > 0 ? $"已加入 {added} 张截图。" : "没有新增截图。";
            return skipped.Count == 0
                ? result
                : result + "已跳过：" + string.Join("；", skipped) + "。";
        }

        private static string GetDisplayName(MediaItemDto item)
            => string.IsNullOrWhiteSpace(item.FileName) ? item.MediaId : item.FileName;
    }
}
