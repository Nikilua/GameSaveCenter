using System;
using System.Linq;
using GameSaveCenter.Core.Models;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class RetentionScenarioDraftTests
{
    [Fact]
    public void CandidatePolicyUsesExistingRetentionPolicyAndReturnsAnIndependentValue()
    {
        var draft = new RetentionScenarioDraft
        {
            KeepAllForHours = 48,
            KeepDailyDays = 21,
            KeepWeeklyWeeks = 8,
            KeepMonthlyMonths = 18,
            GrowthMinimumMiBPer30Days = 120,
            GrowthMaximumMiBPer30Days = 480
        };

        var candidate = draft.CandidatePolicy;
        Assert.Equal(TimeSpan.FromHours(48), candidate.KeepAllFor);
        Assert.Equal(21, candidate.KeepDailyDays);
        Assert.Equal(8, candidate.KeepWeeklyWeeks);
        Assert.Equal(18, candidate.KeepMonthlyMonths);
        candidate.KeepDailyDays = 99;

        Assert.Equal(21, draft.CandidatePolicy.KeepDailyDays);
        Assert.True(draft.IsValid);
    }

    [Fact]
    public void GrowthRangeReportsFieldErrorsUntilLowerBoundDoesNotExceedUpperBound()
    {
        var draft = new RetentionScenarioDraft
        {
            GrowthMinimumMiBPer30Days = 500,
            GrowthMaximumMiBPer30Days = 200
        };

        Assert.True(draft.HasErrors);
        Assert.Contains("不能高于上限", Assert.Single(draft.GetErrors(nameof(draft.GrowthMinimumMiBPer30Days)).Cast<string>()));
        Assert.Contains("不能低于下限", Assert.Single(draft.GetErrors(nameof(draft.GrowthMaximumMiBPer30Days)).Cast<string>()));

        draft.GrowthMaximumMiBPer30Days = 750;

        Assert.False(draft.HasErrors);
        Assert.True(draft.IsValid);
        Assert.Empty(draft.GetErrors(nameof(draft.GrowthMinimumMiBPer30Days)).Cast<string>());
    }

    [Theory]
    [InlineData(nameof(RetentionScenarioDraft.KeepDailyDays), -1)]
    [InlineData(nameof(RetentionScenarioDraft.KeepMonthlyMonths), RetentionScenarioDraft.MaximumKeepMonthlyMonths + 1)]
    public void PolicyInputLimitsRejectValuesOutsidePlannerSafeHorizon(string propertyName, int value)
    {
        var draft = new RetentionScenarioDraft();
        typeof(RetentionScenarioDraft).GetProperty(propertyName)!.SetValue(draft, value);

        Assert.NotEmpty(draft.GetErrors(propertyName).Cast<string>());
        Assert.False(draft.IsValid);
    }
}
