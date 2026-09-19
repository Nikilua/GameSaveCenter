using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class SettingsValidationSourceTests
{
    [Fact]
    public void SettingsPageShowsInlineValidationSummary()
    {
        var root = FindRepositoryRoot();
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml"));
        var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml.cs"));
        var pathValidation = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Infrastructure", "SettingsPathValidationService.cs"));
        var settings = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettings.cs"));
        var plugin = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "GameSaveCenterPlugin.cs"));

        Assert.Contains("x:Name=\"SettingsValidationSummary\"", view);
        Assert.Contains("AutomationProperties.Name=\"设置验证错误\"", view);
        Assert.Contains("x:Name=\"SettingsValidationLocateButton\"", view);
        Assert.Contains("Click=\"OnSettingsValidationLocateClick\"", view);
        Assert.Contains("AutomationProperties.Name=\"定位首个设置错误\"", view);
        Assert.Contains("x:Name=\"SettingsValidationDetails\" Header=\"查看错误详情\" Foreground=\"{DynamicResource GscPrimaryTextBrush}\"", view);
        Assert.Contains("x:Name=\"SettingsValidationDetailsText\"", view);
        Assert.Contains("x:Name=\"HealthInspectionStaleAfterDaysTextBox\"", view);
        Assert.Contains("x:Name=\"RecentProtectionWindowComboBox\"", view);
        Assert.Contains("AutomationProperties.Name=\"设置验证错误详情\"", view);
        Assert.Contains("x:Name=\"SettingsGeneralValidationHint\"", view);
        Assert.Contains("x:Name=\"SettingsSaveHintText\"", view);
        Assert.Contains("AutomationProperties.Name=\"设置保存状态\"", view);
        Assert.Contains("AddHandler(TextBox.TextChangedEvent", code);
        Assert.Contains("AddHandler(ComboBox.SelectionChangedEvent", code);
        Assert.Contains("AddHandler(CheckBox.ClickEvent", code);
        Assert.Contains("AddHandler(ToggleButton.CheckedEvent", code);
        Assert.Contains("AddHandler(ToggleButton.UncheckedEvent", code);
        Assert.Contains("AddHandler(Validation.ErrorEvent", code);
        Assert.Contains("QueueValidationSummaryUpdate", code);
        Assert.Contains("DispatcherPriority.Background", code);
        Assert.Contains("RefreshValidationSummary", code);
        Assert.Contains("FindValidationCategoryIndex", code);
        Assert.Contains("有 {entries.Count} 项设置需要修正", code);
        Assert.Contains("SettingsValidationDetailsText.Inlines.Clear()", code);
        Assert.Contains("new Hyperlink(new Run(entry.Message))", code);
        Assert.Contains("FocusValidationTarget", code);
        Assert.Contains("field.BringIntoView()", code);
        Assert.Contains("Keyboard.Focus(field)", code);
        Assert.Contains("SettingsGeneralValidationHint", code);
        Assert.Contains("SettingsSectionTabs.SelectedIndex", code);
        Assert.Contains("settings.VerifySettingsWithoutPathAvailability(out errors)", code);
        Assert.Contains("settings.VerifySettings(out var errors)", code);
        Assert.Contains("SettingsPathValidationService.ValidateAsync", code);
        Assert.Contains("LatestAsyncValidationCoordinator", code);
        Assert.Contains("pathValidationGeneration", code);
        Assert.Contains("InvalidatePathValidation", code);
        Assert.Contains("CancellationToken", pathValidation);
        Assert.Contains("SettingsSaveFeedbackState", code);
        Assert.Contains("正在保存设置 · 请稍候", code);
        Assert.Contains("已写入 Playnite · 正在应用到 Worker", code);
        Assert.Contains("已保存 · Worker 应用失败", code);
        Assert.Contains("Interlocked.CompareExchange", settings);
        Assert.Contains("SettingsSaveFailed", settings);
        Assert.Contains("ApplySettingsAsync(Action<Exception?>? completion)", plugin);
        Assert.Contains("有未保存更改 · 使用 Playnite 保存", code);
        Assert.Contains("存在校验错误 · 保存前请修正", code);
        Assert.Contains("CreateSettingsFingerprint", code);
        Assert.Contains("SettingsCommitted", code);
        Assert.Contains("SettingsReverted", code);
        Assert.Contains("OnHostWindowClosing", code);
        Assert.Contains("继续编辑并返回当前字段", code);
        Assert.Contains("CurrentSettings?.CancelEdit();", code);
        Assert.Contains("RestoreDraftFocus", code);
        Assert.Contains("GetEditBaselineFingerprint", code);
        Assert.Contains("x:Name=\"SettingsSearchTextBox\"", view);
        Assert.Contains("AutomationProperties.Name=\"搜索设置\"", view);
        Assert.Contains("TextChanged=\"OnSettingsSearchTextChanged\"", view);
        Assert.Contains("SearchTermsProperty", code);
        Assert.Contains("RegisterSettingsSearchTargets", code);
        Assert.Contains("搜索只改变可见字段，不会修改设置值", view);
        Assert.Contains("settingsSearchOriginCategory", code);
        Assert.Contains("SettingsSearchSummary", code);
    }

    private static string FindRepositoryRoot()
        => TestRepositoryContext.Root;
}
