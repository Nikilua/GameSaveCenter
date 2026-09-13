using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GameSaveCenter.Playnite.Infrastructure;
using Playnite.SDK;
using Snackbar = GameSaveCenter.Playnite.Controls.Snackbar;

namespace GameSaveCenter.Playnite.Views.Development;

public partial class UiFrameworkProbeView : UserControl
{
    private static readonly ILogger Logger = LogManager.GetLogger();
    private readonly UiFrameworkProbeFeedback feedback;

    public UiFrameworkProbeView()
    {
        InitializeComponent();
        ProbeRows = new ObservableCollection<ProbeRow>
        {
            new ProbeRow("赛博朋克 2077：往日之影 / Cyberpunk 2077", "1,024 / 99,999", "已完成", @"D:\Games\中文目录\Cyberpunk 2077\一个非常长的备份文件名.zip"),
            new ProbeRow("最终幻想 XIV：黄金的遗产 / FINAL FANTASY XIV", "9% → 100%", "需关注", "FLING_DOWNLOAD_FORBIDDEN · 可复制完整诊断"),
            new ProbeRow("NieR Replicant / Pokémon / 龍が如く", "128 KB", "失败", "备份完成，云端校验失败。可以稍后重试。"),
            new ProbeRow("罕见字：𠮷；中文标点，。！？；：「」《》", "1.25 GB", "已完成", "2026-09-13 09:41 · 00:09 / 12:59")
        };
        DataContext = this;
        feedback = new UiFrameworkProbeFeedback(
            exception => Logger.Error(exception, "GameSaveCenter WPF-UI probe dialog failed."),
            ShowProbeFailure);
    }

    public ObservableCollection<ProbeRow> ProbeRows { get; }

    public sealed class ProbeRow
    {
        public ProbeRow(string name, string value, string state, string detail)
        {
            Name = name;
            Value = value;
            State = state;
            Detail = detail;
        }

        public string Name { get; }
        public string Value { get; }
        public string State { get; }
        public string Detail { get; }
    }

    private void OnShowDialogClick(object sender, RoutedEventArgs e)
    {
        ShowProbeFailure("WPF-UI ContentDialogHost 只能在每个 Window 注册一次；Playnite 内嵌页面不创建该宿主。正式确认继续使用 GameSaveCenter 的插件内对话层，避免影响其他扩展。");
    }

    private async void OnShowSnackbarClick(object sender, RoutedEventArgs e)
    {
        await feedback.TryShowAsync(() =>
        {
            var snackbar = new Snackbar(SnackbarHost)
            {
                Title = "WPF-UI Snackbar",
                Content = "这是资源隔离和浮层显示验证，不是任务成功提示。",
                Timeout = TimeSpan.FromSeconds(3),
                IsCloseButtonEnabled = true
            };

            snackbar.Show();
            return Task.CompletedTask;
        });
    }

    private void ShowProbeFailure(string message)
    {
        ProbeFailureText.Text = message;
        ProbeFailurePanel.Visibility = Visibility.Visible;
    }
}
