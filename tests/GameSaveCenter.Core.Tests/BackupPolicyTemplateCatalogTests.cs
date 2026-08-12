using System.Linq;
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
}
