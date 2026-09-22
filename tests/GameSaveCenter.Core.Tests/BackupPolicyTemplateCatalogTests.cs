using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using Xunit;

namespace GameSaveCenter.Core.Tests;

public sealed class BackupPolicyTemplateCatalogTests
{
    [Fact]
    public void ProvidesFiveStableBuiltInsWithSafeRestoreBoundary()
    {
        var templates = BackupPolicyTemplateCatalog.CreateBuiltIns();

        Assert.Equal(5, templates.Count);
        Assert.Equal(new[]
        {
            BackupPolicyTemplateCatalog.DefaultId,
            BackupPolicyTemplateCatalog.ImportantId,
            BackupPolicyTemplateCatalog.HighFrequencyId,
            BackupPolicyTemplateCatalog.ExitOnlyId,
            BackupPolicyTemplateCatalog.ManualOnlyId
        }, templates.Select(x => x.TemplateId));
        Assert.All(templates, template =>
        {
            Assert.True(template.IsBuiltIn);
            Assert.False(template.Policy.AllowAutomaticRestore);
        });
        Assert.False(templates.Single(x => x.TemplateId == BackupPolicyTemplateCatalog.ManualOnlyId).Policy.Enabled);
        Assert.False(templates.Single(x => x.TemplateId == BackupPolicyTemplateCatalog.ExitOnlyId).Policy.BackupDuringPlay);
        Assert.Equal(BackupAnomalyProtectionLevel.Strict, templates.Single(x => x.TemplateId == BackupPolicyTemplateCatalog.ImportantId).Policy.AnomalyProtectionLevel);
    }

    [Fact]
    public void CloneCopiesPolicyWithoutSharingMutableState()
    {
        var original = BackupPolicyTemplateCatalog.CreateBuiltIns().First();
        var clone = BackupPolicyTemplateCatalog.Clone(original);

        clone.Name = "Changed";
        clone.Policy.DuringPlayIntervalMinutes = 1;

        Assert.NotEqual(clone.Name, original.Name);
        Assert.NotEqual(clone.Policy.DuringPlayIntervalMinutes, original.Policy.DuringPlayIntervalMinutes);
    }

    [Fact]
    public void PolicyDiffSeparatesSavedBaselineAndExplicitValues()
    {
        var inherited = new BackupPolicyDto { DuringPlayIntervalMinutes = 30, UploadAfterBackup = false };
        var explicitValue = BackupPolicyTemplateCatalog.ClonePolicy(inherited);
        explicitValue.DuringPlayIntervalMinutes = 15;
        explicitValue.UploadAfterBackup = true;

        var diff = BackupPolicyDiff.Compare(inherited, explicitValue);

        Assert.Equal(new[] { "DuringPlayIntervalMinutes", "UploadAfterBackup" }, diff.Select(x => x.FieldKey));
        Assert.Equal("30 分钟", diff[0].InheritedValue);
        Assert.Equal("15 分钟", diff[0].ExplicitValue);
        Assert.Equal("关闭", diff[1].InheritedValue);
        Assert.Equal("开启", diff[1].ExplicitValue);
    }

    [Fact]
    public void CopyPolicyToRestoresDraftWithoutChangingTheSource()
    {
        var source = new BackupPolicyDto { BackupDuringPlay = false, KeepDailyDays = 90 };
        var target = new BackupPolicyDto { BackupDuringPlay = true, KeepDailyDays = 1 };

        BackupPolicyDiff.CopyTo(source, target);

        Assert.False(target.BackupDuringPlay);
        Assert.Equal(90, target.KeepDailyDays);
        Assert.True(source.BackupOnGameStop);
    }

    [Fact]
    public void PolicyChangesRaiseFieldNotificationsForThePreviewBinding()
    {
        var policy = new BackupPolicyDto();
        var changed = new List<string>();
        policy.PropertyChanged += (_, args) => changed.Add(args.PropertyName ?? string.Empty);

        policy.DuringPlayIntervalMinutes = 15;
        policy.DuringPlayIntervalMinutes = 15;
        policy.UploadAfterBackup = true;

        Assert.Equal(new[] { "DuringPlayIntervalMinutes", "UploadAfterBackup" }, changed);
    }
}
