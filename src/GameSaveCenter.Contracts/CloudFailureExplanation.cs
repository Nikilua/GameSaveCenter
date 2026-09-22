using System;

namespace GameSaveCenter.Contracts;

/// <summary>Display-only next steps for stable, already classified cloud failures.</summary>
public sealed class CloudFailureExplanation
{
    private CloudFailureExplanation(string categoryDisplay, string nextStepDisplay)
    {
        CategoryDisplay = categoryDisplay;
        NextStepDisplay = nextStepDisplay;
    }

    public string CategoryDisplay { get; }
    public string NextStepDisplay { get; }
    public bool IsRecognized => !string.IsNullOrWhiteSpace(CategoryDisplay);

    public static CloudFailureExplanation Resolve(string? errorCode)
    {
        switch ((errorCode ?? string.Empty).Trim().ToUpperInvariant())
        {
            case "RCLONE_AUTH_FAILED":
                return new CloudFailureExplanation("认证失败", "下一步：前往设置检查云端凭据并重新验证远端，然后再重试。");
            case "RCLONE_NO_SPACE":
                return new CloudFailureExplanation("空间不足", "下一步：释放本地或远端空间、检查配额，完成后再重试。");
            case "RCLONE_REMOTE_NOT_FOUND":
                return new CloudFailureExplanation("远端不存在", "下一步：检查远端名称和目标目录；不要自动创建未知目标。");
            case "RCLONE_CHECK_FAILED":
            case "RCLONE_CHECK_DIFFERENCE":
                return new CloudFailureExplanation("校验差异", "下一步：先查看本地与远端内容差异，确认后再决定重试或恢复。");
            case "RCLONE_RATE_LIMITED":
                return new CloudFailureExplanation("远端限流", "下一步：等待限流窗口结束，按队列退避重试，不要连续点击重试。");
            default:
                return new CloudFailureExplanation(string.Empty, string.Empty);
        }
    }
}
