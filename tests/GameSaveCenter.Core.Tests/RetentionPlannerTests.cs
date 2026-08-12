using GameSaveCenter.Core.Models;
using GameSaveCenter.Core.Services;
using GameSaveCenter.Contracts;
using Xunit;
namespace GameSaveCenter.Core.Tests;
public sealed class RetentionPlannerTests
{
    [Fact]
    public void LockedAndPreRestoreVersionsAreAlwaysKept()
    {
        var now=DateTime.UtcNow;
        var versions=new[]{new BackupSnapshot{BackupId="locked",CreatedUtc=now.AddYears(-3),IsLocked=true},new BackupSnapshot{BackupId="pre",CreatedUtc=now.AddYears(-2),IsPreRestore=true}};
        var plan=new RetentionPlanner().CreatePlan(versions,new RetentionPolicy(),now);
        Assert.Contains(plan.Keep,x=>x.BackupId=="locked");Assert.Contains(plan.Keep,x=>x.BackupId=="pre");
    }

    [Fact]
    public void HealthyRestorePointRemainsProtectedFromRetentionCandidates()
    {
        var now = DateTime.UtcNow;
        var healthy = new BackupSnapshot
        {
            BackupId = "healthy",
            CreatedUtc = now.AddYears(-3),
            ReadinessStatus = RestoreReadinessStatus.Ready
        };
        var plan = new RetentionPlanner().CreatePlan(
            new[] { healthy },
            new RetentionPolicy { KeepAllFor = TimeSpan.Zero, KeepDailyDays = 0, KeepWeeklyWeeks = 0, KeepMonthlyMonths = 0 },
            now);

        Assert.Contains(plan.HealthProtected, x => x.BackupId == "healthy");
        Assert.Contains(plan.Keep, x => x.BackupId == "healthy");
        Assert.DoesNotContain(plan.DeleteCandidates, x => x.BackupId == "healthy");
    }
}
