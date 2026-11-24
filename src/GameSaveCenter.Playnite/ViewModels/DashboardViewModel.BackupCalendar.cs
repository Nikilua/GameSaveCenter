using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed class BackupCalendarDay
    {
        public DateTime? Date { get; set; }
        public int Count { get; set; }
        public string DayLabel => Date?.Day.ToString() ?? string.Empty;
        public string CountLabel => Count > 999 ? "999+" : Count > 0 ? Count.ToString() : string.Empty;
        public string AccessibleLabel => Date.HasValue ? $"{Date:yyyy年M月d日}，{Count} 个版本" : string.Empty;
    }

    public sealed partial class DashboardViewModel
    {
        private readonly LatestRequestCoordinator backupCalendarReads = new LatestRequestCoordinator();
        private bool isBackupCalendarOpen;
        private DateTime backupCalendarMonth;
        private DateTime? backupHistoryCalendarDay;
        private string backupCalendarStatus = string.Empty;

        public bool IsBackupCalendarOpen => isBackupCalendarOpen;
        public string BackupCalendarMonthLabel => $"{backupCalendarMonth:yyyy 年 M 月}";
        public string BackupCalendarStatus => backupCalendarStatus;
        public BatchObservableCollection<BackupCalendarDay> BackupCalendarDays { get; } = new BatchObservableCollection<BackupCalendarDay>();
        public ICommand ToggleBackupCalendarCommand { get; private set; } = null!;
        public ICommand PreviousBackupCalendarMonthCommand { get; private set; } = null!;
        public ICommand NextBackupCalendarMonthCommand { get; private set; } = null!;
        public ICommand SelectBackupCalendarDayCommand { get; private set; } = null!;

        private void InitializeBackupCalendarCommands()
        {
            ToggleBackupCalendarCommand = new RelayCommand(_ => ToggleBackupCalendar(),
                _ => CurrentWorkspace == WorkspaceKind.Saves && SelectedGame != null);
            PreviousBackupCalendarMonthCommand = new RelayCommand(_ => AdvanceBackupCalendar(-1),
                _ => isBackupCalendarOpen && backupCalendarMonth.Year > 1);
            NextBackupCalendarMonthCommand = new RelayCommand(_ => AdvanceBackupCalendar(1),
                _ => isBackupCalendarOpen && backupCalendarMonth.Year < 9999);
            SelectBackupCalendarDayCommand = new RelayCommand(
                day => SelectBackupCalendarDay(((BackupCalendarDay)day!).Date!.Value),
                day => day is BackupCalendarDay entry && entry.Date.HasValue && isBackupCalendarOpen);
        }

        private void ToggleBackupCalendar()
        {
            isBackupCalendarOpen = !isBackupCalendarOpen;
            OnPropertyChanged(nameof(IsBackupCalendarOpen));
            if (isBackupCalendarOpen)
            {
                var date = backupHistoryCalendarDay ?? DateTime.Today;
                backupCalendarMonth = new DateTime(date.Year, date.Month, 1);
                Observe(LoadBackupCalendarAsync());
            }
            else backupCalendarReads.Cancel();
            RaiseCommandStates();
        }

        private void AdvanceBackupCalendar(int months)
        {
            backupCalendarMonth = backupCalendarMonth.AddMonths(months);
            Observe(LoadBackupCalendarAsync());
        }

        private async Task LoadBackupCalendarAsync()
        {
            var gameId = SelectedGame?.PlayniteId;
            if (string.IsNullOrEmpty(gameId)) return;
            var month = backupCalendarMonth;
            var days = DateTime.DaysInMonth(month.Year, month.Month);
            var request = backupCalendarReads.Begin();
            BackupCalendarDays.Clear();
            backupCalendarStatus = "正在读取本月备份数量…";
            OnPropertyChanged(nameof(BackupCalendarMonthLabel));
            OnPropertyChanged(nameof(BackupCalendarStatus));
            RaiseCommandStates();
            try
            {
                var values = await plugin.RequestAsync<BackupCalendarDayDto[]>(MessageTypes.GetBackupCalendar,
                    new BackupCalendarQueryDto
                    {
                        PlayniteId = gameId!,
                        UtcDayBoundaries = Enumerable.Range(0, days + 1)
                            .Select(day => month.AddDays(day).ToUniversalTime()).ToList()
                    }, cancellationToken: request.Token).ConfigureAwait(false);
                ApplyOnUi(() =>
                {
                    if (!backupCalendarReads.IsCurrent(request) || !isBackupCalendarOpen || !IsSelectedGame(gameId!)) return;
                    var counts = values.ToDictionary(value => value.Day, value => value.Count);
                    var blanks = ((int)month.DayOfWeek + 6) % 7;
                    var calendarDays = Enumerable.Range(0, blanks).Select(_ => new BackupCalendarDay())
                        .Concat(Enumerable.Range(1, days).Select(day => new BackupCalendarDay
                        {
                            Date = month.AddDays(day - 1),
                            Count = counts.TryGetValue(day, out var count) ? count : 0
                        }));
                    BackupCalendarDays.ReplaceAll(calendarDays);
                    backupCalendarStatus = values.Length == 0 ? "本月没有存档版本" : string.Empty;
                    OnPropertyChanged(nameof(BackupCalendarStatus));
                });
            }
            catch (OperationCanceledException) when (request.Token.IsCancellationRequested) { }
            catch (Exception)
            {
                ApplyOnUi(() =>
                {
                    if (!backupCalendarReads.IsCurrent(request)) return;
                    backupCalendarStatus = "无法读取本月备份数量";
                    OnPropertyChanged(nameof(BackupCalendarStatus));
                });
            }
            finally { backupCalendarReads.End(request); }
        }

        private void SelectBackupCalendarDay(DateTime day)
        {
            ClearBackupCalendarSelection();
            BackupHistoryRange = BackupHistoryDateRange.All;
            BackupHistorySearchText = string.Empty;
            ResetBackupHistorySearch();
            backupHistoryCalendarDay = day.Date;
            OnPropertyChanged(nameof(BackupHistoryHasActiveRange));
            RefreshBackupHistoryView();
            Observe(SearchBackupHistoryAsync(loadMore: false));
        }

        private void ClearBackupCalendarSelection()
        {
            if (!backupHistoryCalendarDay.HasValue) return;
            backupHistoryCalendarDay = null;
            ResetBackupHistorySearch();
            RefreshBackupHistoryView();
            OnPropertyChanged(nameof(BackupHistoryHasActiveRange));
        }

        private void ResetBackupCalendar()
        {
            backupCalendarReads?.Cancel();
            if (isBackupCalendarOpen)
            {
                isBackupCalendarOpen = false;
                OnPropertyChanged(nameof(IsBackupCalendarOpen));
            }
            BackupCalendarDays?.Clear();
            ClearBackupCalendarSelection();
        }
    }
}
