using System;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.ViewModels
{
    /// <summary>Formats overview scope and numeric state without turning an unloaded snapshot into zero.</summary>
    public static class OverviewSnapshotDisplay
    {
        public static string Count(bool loaded, int value)
            => loaded ? value.ToString() : "—";

        public static string Scope(bool loaded, DateTime generatedUtc)
            => loaded
                ? $"全库 · Playnite 游戏库 · {UpdatedRelative(loaded, generatedUtc)}"
                : "全库 · 尚未加载；数字显示 —，不代表 0";

        public static string CurrentGameScope(bool loaded, DateTime generatedUtc)
            => loaded
                ? $"当前游戏 · 与全库快照同步 · {UpdatedRelative(loaded, generatedUtc)}"
                : "当前游戏 · 尚未加载；数字显示 —，不代表 0";

        public static string Updated(bool loaded, DateTime generatedUtc)
            => UpdatedFull(loaded, generatedUtc);

        public static string UpdatedRelative(bool loaded, DateTime generatedUtc)
            => loaded && generatedUtc != default(DateTime)
                ? $"更新于 {TimeDisplayFormatter.Relative(generatedUtc, DateTime.UtcNow)}"
                : "更新时间未知";

        public static string UpdatedFull(bool loaded, DateTime generatedUtc)
            => loaded && generatedUtc != default(DateTime)
                ? $"更新于 {TimeDisplayFormatter.Full(generatedUtc)}"
                : "更新时间未知";

        public static string UpdatedRawUtc(bool loaded, DateTime generatedUtc)
            => loaded && generatedUtc != default(DateTime)
                ? TimeDisplayFormatter.RawUtc(generatedUtc)
                : "未记录 UTC 时间";
    }
}
