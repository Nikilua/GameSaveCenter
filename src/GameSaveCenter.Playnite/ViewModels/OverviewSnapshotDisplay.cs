using System;

namespace GameSaveCenter.Playnite.ViewModels
{
    /// <summary>Formats overview scope and numeric state without turning an unloaded snapshot into zero.</summary>
    public static class OverviewSnapshotDisplay
    {
        public static string Count(bool loaded, int value)
            => loaded ? value.ToString() : "—";

        public static string Scope(bool loaded, DateTime generatedUtc)
            => loaded
                ? $"全库 · Playnite 游戏库 · {Updated(loaded, generatedUtc)}"
                : "全库 · 尚未加载；数字显示 —，不代表 0";

        public static string CurrentGameScope(bool loaded, DateTime generatedUtc)
            => loaded
                ? $"当前游戏 · 与全库快照同步 · {Updated(loaded, generatedUtc)}"
                : "当前游戏 · 尚未加载；数字显示 —，不代表 0";

        public static string Updated(bool loaded, DateTime generatedUtc)
            => loaded && generatedUtc != default(DateTime)
                ? $"更新于 {generatedUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}"
                : "更新时间未知";
    }
}
