using System;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

/// <summary>
/// Marks source-contract tests for the discarded production UI architecture.
/// The production pages intentionally use the AcrylicFork baseline restored in
/// the rollback commit, so these assertions remain as history without running
/// against a layout they no longer describe.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class LegacyProductionUiBaselineFactAttribute : FactAttribute
{
    public LegacyProductionUiBaselineFactAttribute()
    {
        Skip = "该断言属于已撤销的今日工作台 UI 架构，当前生产页面恢复 AcrylicFork 基线后不再适用。";
    }
}
