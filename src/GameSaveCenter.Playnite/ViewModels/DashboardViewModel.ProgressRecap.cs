using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GameSaveCenter.Playnite.Converters;
using GameSaveCenter.Playnite.Infrastructure;
using Microsoft.Win32;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private readonly LatestRequestCoordinator progressRecapRequests = new LatestRequestCoordinator();
        private bool progressRecapKeepMissing = true;
        private bool progressRecapSaving;
        private BitmapSource? progressRecapPreview;
        private string progressRecapStatus = string.Empty;

        public bool ProgressRecapKeepMissing
        {
            get => progressRecapKeepMissing;
            set
            {
                if (progressRecapKeepMissing == value) return;
                SetValue(ref progressRecapKeepMissing, value);
                InvalidateProgressRecap();
            }
        }

        private int SelectedProgressRecapCount => BackupGalleryItems?.Count(item => item.IsSelected) ?? 0;
        public string ProgressRecapSelectionLabel => $"已选 {SelectedProgressRecapCount}/9 个进度";
        public BitmapSource? ProgressRecapPreview => progressRecapPreview;
        public string ProgressRecapStatus => progressRecapStatus;
        public ICommand PreviewProgressRecapCommand { get; private set; } = null!;
        public ICommand SaveProgressRecapCommand { get; private set; } = null!;

        private void InitializeProgressRecapCommands()
        {
            PreviewProgressRecapCommand = new RelayCommand(
                _ => Observe(PreviewProgressRecapAsync()), _ => CanUseBackupGallery && SelectedProgressRecapCount >= 2);
            SaveProgressRecapCommand = new RelayCommand(
                _ =>
                {
                    var preview = progressRecapPreview;
                    if (preview != null) Observe(SaveProgressRecapAsync(preview));
                },
                _ => CanUseBackupGallery && !progressRecapSaving && progressRecapPreview != null);
        }

        private void InvalidateProgressRecap()
        {
            progressRecapRequests?.Cancel();
            progressRecapPreview = null;
            progressRecapStatus = string.Empty;
            NotifyProgressRecapChanged();
        }

        private async Task PreviewProgressRecapAsync()
        {
            var selected = BackupGalleryItems.Where(item => item.IsSelected).ToArray();
            if (selected.Length < 2 || selected.Length > 9) return;
            var keepMissing = ProgressRecapKeepMissing;
            var request = progressRecapRequests.Begin();
            progressRecapPreview = null;
            progressRecapStatus = "正在生成回顾图预览…";
            NotifyProgressRecapChanged();
            try
            {
                var cards = await Task.WhenAll(selected.Select(async item =>
                {
                    var photo = string.IsNullOrEmpty(item.Cover.ArchivePath) ? null :
                        await AsyncThumbnailLoader.LoadAsync(item.Cover.ArchivePath, 460, request.Token).ConfigureAwait(false);
                    var time = item.Backup.CreatedUtc;
                    var date = time == DateTime.MinValue ? "日期未知" : time.ToLocalTime().ToString("yyyy-MM-dd");
                    return new ProgressRecapCard(photo, date, item.Backup.Comment);
                })).ConfigureAwait(false);
                var visible = keepMissing ? cards : cards.Where(card => card.Picture != null).ToArray();
                ApplyOnUi(() =>
                {
                    if (!progressRecapRequests.IsCurrent(request) || !CanUseBackupGallery) return;
                    if (visible.Length < 2)
                    {
                        progressRecapStatus = "可用截图少于 2 张。请多选进度或保留缺图占位。";
                    }
                    else
                    {
                        progressRecapPreview = ProgressRecapRenderer.Render(visible);
                        progressRecapStatus = $"预览包含 {visible.Length} 个进度";
                    }
                    NotifyProgressRecapChanged();
                });
            }
            catch (OperationCanceledException) when (request.Token.IsCancellationRequested) { }
            catch (Exception)
            {
                ApplyOnUi(() =>
                {
                    if (!progressRecapRequests.IsCurrent(request)) return;
                    progressRecapStatus = "无法生成回顾图预览";
                    NotifyProgressRecapChanged();
                });
            }
            finally { progressRecapRequests.End(request); }
        }

        private async Task SaveProgressRecapAsync(BitmapSource preview)
        {
            if (progressRecapSaving) return;
            progressRecapSaving = true;
            RaiseCommandStates();
            try
            {
                var dialog = new SaveFileDialog
                {
                    FileName = $"游玩进度-{DateTime.Today:yyyy-MM-dd}",
                    Filter = "PNG 图片 (*.png)|*.png",
                    DefaultExt = ".png",
                    AddExtension = true,
                    OverwritePrompt = true
                };
                if (dialog.ShowDialog() != true) return;
                var destination = dialog.FileName;
                await Task.Run(() => ProgressRecapRenderer.SavePng(preview, destination)).ConfigureAwait(false);
                ApplyOnUi(() =>
                {
                    if (!ReferenceEquals(preview, progressRecapPreview)) return;
                    progressRecapStatus = "回顾图已保存";
                    NotifyProgressRecapChanged();
                });
            }
            catch (Exception)
            {
                ApplyOnUi(() =>
                {
                    if (!ReferenceEquals(preview, progressRecapPreview)) return;
                    progressRecapStatus = "无法保存回顾图";
                    NotifyProgressRecapChanged();
                });
            }
            finally
            {
                ApplyOnUi(() =>
                {
                    progressRecapSaving = false;
                    RaiseCommandStates();
                });
            }
        }

        private void NotifyProgressRecapChanged()
        {
            OnPropertyChanged(nameof(ProgressRecapSelectionLabel));
            OnPropertyChanged(nameof(ProgressRecapStatus));
            OnPropertyChanged(nameof(ProgressRecapPreview));
            RaiseCommandStates();
        }
    }
}
