using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using GameSaveCenter.Core.Models;

namespace GameSaveCenter.Playnite.ViewModels;

/// <summary>
/// Session-only candidate retention inputs. This draft is deliberately not part of settings,
/// persistence, Worker requests, or the cleanup command path.
/// </summary>
public sealed class RetentionScenarioDraft : INotifyPropertyChanged, INotifyDataErrorInfo
{
    public const int MaximumKeepAllForHours = 876000;
    public const int MaximumKeepDailyDays = 36500;
    public const int MaximumKeepWeeklyWeeks = 5200;
    public const int MaximumKeepMonthlyMonths = 1200;
    public const int MaximumGrowthMiBPer30Days = 1000000;

    private int keepAllForHours = 24;
    private int keepDailyDays = 30;
    private int keepWeeklyWeeks = 12;
    private int keepMonthlyMonths = 24;
    private int? growthMinimumMiBPer30Days;
    private int? growthMaximumMiBPer30Days;

    public int KeepAllForHours
    {
        get => keepAllForHours;
        set => SetPolicyValue(ref keepAllForHours, value);
    }

    public int KeepDailyDays
    {
        get => keepDailyDays;
        set => SetPolicyValue(ref keepDailyDays, value);
    }

    public int KeepWeeklyWeeks
    {
        get => keepWeeklyWeeks;
        set => SetPolicyValue(ref keepWeeklyWeeks, value);
    }

    public int KeepMonthlyMonths
    {
        get => keepMonthlyMonths;
        set => SetPolicyValue(ref keepMonthlyMonths, value);
    }

    public int? GrowthMinimumMiBPer30Days
    {
        get => growthMinimumMiBPer30Days;
        set
        {
            if (growthMinimumMiBPer30Days == value) return;
            growthMinimumMiBPer30Days = value;
            OnPropertyChanged(nameof(GrowthMinimumMiBPer30Days));
            NotifyGrowthValidationChanged();
        }
    }

    public int? GrowthMaximumMiBPer30Days
    {
        get => growthMaximumMiBPer30Days;
        set
        {
            if (growthMaximumMiBPer30Days == value) return;
            growthMaximumMiBPer30Days = value;
            OnPropertyChanged(nameof(GrowthMaximumMiBPer30Days));
            NotifyGrowthValidationChanged();
        }
    }

    /// <summary>Returns a fresh value object so edits cannot mutate an effective game policy.</summary>
    public RetentionPolicy CandidatePolicy => new RetentionPolicy
    {
        KeepAllFor = TimeSpan.FromHours(keepAllForHours),
        KeepDailyDays = keepDailyDays,
        KeepWeeklyWeeks = keepWeeklyWeeks,
        KeepMonthlyMonths = keepMonthlyMonths
    };

    public bool HasErrors => !IsValid;

    public bool IsValid =>
        IsInRange(keepAllForHours, MaximumKeepAllForHours)
        && IsInRange(keepDailyDays, MaximumKeepDailyDays)
        && IsInRange(keepWeeklyWeeks, MaximumKeepWeeklyWeeks)
        && IsInRange(keepMonthlyMonths, MaximumKeepMonthlyMonths)
        && IsInRange(growthMinimumMiBPer30Days, MaximumGrowthMiBPer30Days)
        && IsInRange(growthMaximumMiBPer30Days, MaximumGrowthMiBPer30Days)
        && growthMinimumMiBPer30Days.HasValue
        && growthMaximumMiBPer30Days.HasValue
        && growthMinimumMiBPer30Days.Value <= growthMaximumMiBPer30Days.Value;

    public string Error => string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            var errors = new List<string>();
            foreach (var name in new[]
            {
                nameof(KeepAllForHours), nameof(KeepDailyDays), nameof(KeepWeeklyWeeks), nameof(KeepMonthlyMonths),
                nameof(GrowthMinimumMiBPer30Days), nameof(GrowthMaximumMiBPer30Days)
            })
                errors.AddRange((IEnumerable<string>)GetErrors(name));
            return errors;
        }

        if (propertyName == nameof(KeepAllForHours) && !IsInRange(keepAllForHours, MaximumKeepAllForHours))
            return new[] { $"请输入 0–{MaximumKeepAllForHours} 小时。" };
        if (propertyName == nameof(KeepDailyDays) && !IsInRange(keepDailyDays, MaximumKeepDailyDays))
            return new[] { $"请输入 0–{MaximumKeepDailyDays} 天。" };
        if (propertyName == nameof(KeepWeeklyWeeks) && !IsInRange(keepWeeklyWeeks, MaximumKeepWeeklyWeeks))
            return new[] { $"请输入 0–{MaximumKeepWeeklyWeeks} 周。" };
        if (propertyName == nameof(KeepMonthlyMonths) && !IsInRange(keepMonthlyMonths, MaximumKeepMonthlyMonths))
            return new[] { $"请输入 0–{MaximumKeepMonthlyMonths} 个月。" };
        if (propertyName == nameof(GrowthMinimumMiBPer30Days))
        {
            if (!IsInRange(growthMinimumMiBPer30Days, MaximumGrowthMiBPer30Days))
                return new[] { $"请输入 0–{MaximumGrowthMiBPer30Days} MiB。" };
            if (growthMinimumMiBPer30Days.HasValue && growthMaximumMiBPer30Days.HasValue
                && growthMinimumMiBPer30Days.Value > growthMaximumMiBPer30Days.Value)
                return new[] { "增长下限不能高于上限。" };
        }
        if (propertyName == nameof(GrowthMaximumMiBPer30Days))
        {
            if (!IsInRange(growthMaximumMiBPer30Days, MaximumGrowthMiBPer30Days))
                return new[] { $"请输入 0–{MaximumGrowthMiBPer30Days} MiB。" };
            if (growthMinimumMiBPer30Days.HasValue && growthMaximumMiBPer30Days.HasValue
                && growthMaximumMiBPer30Days.Value < growthMinimumMiBPer30Days.Value)
                return new[] { "增长上限不能低于下限。" };
        }

        return Array.Empty<string>();
    }

    private void SetPolicyValue(ref int field, int value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (field == value) return;
        field = value;
        OnPropertyChanged(propertyName);
        OnPropertyChanged(nameof(CandidatePolicy));
        OnPropertyChanged(nameof(IsValid));
        OnPropertyChanged(nameof(HasErrors));
        if (propertyName != null) ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    private void NotifyGrowthValidationChanged()
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(GrowthMinimumMiBPer30Days)));
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(GrowthMaximumMiBPer30Days)));
        OnPropertyChanged(nameof(IsValid));
        OnPropertyChanged(nameof(HasErrors));
    }

    private void OnPropertyChanged(string? propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private static bool IsInRange(int value, int maximum) => value >= 0 && value <= maximum;
    private static bool IsInRange(int? value, int maximum) => value.HasValue && IsInRange(value.Value, maximum);
}
