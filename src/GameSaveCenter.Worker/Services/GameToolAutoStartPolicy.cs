using GameSaveCenter.Contracts;

namespace GameSaveCenter.Worker.Services;

public static class GameToolAutoStartPolicy
{
    public static bool IsAllowed(GameToolDto tool, bool hasAntiCheat, out string reason)
    {
        if (hasAntiCheat)
        {
            if (tool.ToolType != GameToolType.CustomExecutable)
            {
                reason = "当前游戏存在反作弊风险，修改器/Cheat Table 不允许自动启动";
                return false;
            }
            if (tool.RiskCategory == GameToolRiskCategory.GameModification)
            {
                reason = "当前游戏存在反作弊风险，游戏修改工具不允许自动启动";
                return false;
            }
            if (tool.RiskCategory == GameToolRiskCategory.Unknown && !tool.AllowUnknownToolWithAntiCheat)
            {
                reason = "当前游戏存在反作弊风险，未分类工具需要显式授权后才能自动启动";
                return false;
            }
        }
        reason = string.Empty;
        return true;
    }
}
