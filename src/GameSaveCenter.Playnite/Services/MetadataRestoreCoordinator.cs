using System;
using System.Threading.Tasks;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.Services
{
    /// <summary>
    /// Cross-process metadata restore compensation. If importing or applying plugin
    /// settings fails after the Worker already replaced SQLite/Worker settings, the
    /// coordinator restores the previous plugin settings and asks the Worker to roll
    /// back from the returned PreRestorePath so the whole restore stays atomic.
    /// </summary>
    public static class MetadataRestoreCoordinator
    {
        public static async Task<SettingsImportReport> ApplyPluginSettingsAsync(
            GameSaveCenterSettings settings,
            Action saveSettings,
            Action notifyVisualSettings,
            Func<Task> applySettings,
            string pluginSettingsJson,
            string preRestorePluginJson,
            Func<Task<MetadataRestoreRollbackResultDto>> rollbackMetadataRestore)
        {
            try
            {
                var report = settings.ImportPortableJson(pluginSettingsJson);
                saveSettings();
                notifyVisualSettings();
                await applySettings().ConfigureAwait(false);
                return report;
            }
            catch (Exception ex)
            {
                string? pluginRollbackError = null;
                try
                {
                    settings.ImportPortableJson(preRestorePluginJson);
                    saveSettings();
                    notifyVisualSettings();
                }
                catch (Exception pluginRollbackEx)
                {
                    pluginRollbackError = pluginRollbackEx.Message;
                }

                MetadataRestoreRollbackResultDto rollback;
                try
                {
                    rollback = await rollbackMetadataRestore().ConfigureAwait(false);
                }
                catch (Exception rollbackEx)
                {
                    throw new InvalidOperationException("元数据恢复失败且整体回滚未完成，需要人工介入：" + rollbackEx.Message, ex);
                }

                if (pluginRollbackError != null)
                {
                    throw new InvalidOperationException("元数据恢复失败，且插件设置回滚失败，需要人工介入：" + pluginRollbackError, ex);
                }

                throw new InvalidOperationException("元数据已整体回滚到恢复前状态：" + rollback.Summary, ex);
            }
        }
    }
}
