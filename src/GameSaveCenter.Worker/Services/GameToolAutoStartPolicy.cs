using GameSaveCenter.Contracts;

namespace GameSaveCenter.Worker.Services;

public static class GameToolAutoStartPolicy
{
    public static bool IsAllowed(GameToolDto tool, bool hasAntiCheat, out string reason)
    {
        if (tool.ToolType == GameToolType.CustomExecutable && tool.RiskCategory == GameToolRiskCategory.Unknown)
        {
            reason = "工具尚未分类，自动启动已暂缓";
            return false;
        }
        if (hasAntiCheat && (tool.ToolType != GameToolType.CustomExecutable || tool.RiskCategory == GameToolRiskCategory.GameModification))
        {
            reason = "当前游戏存在反作弊风险，该工具类别不允许自动启动";
            return false;
        }
        reason = string.Empty;
        return true;
    }
}
