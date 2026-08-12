namespace GameSaveCenter.Worker.Infrastructure;

public enum RcloneFailureKind
{
    Unknown,
    Network,
    Authentication,
    Permission,
    RemoteNotFound,
    Incomplete
}

/// <summary>Converts provider-specific Rclone text into stable, actionable task codes.</summary>
public static class RcloneFailureClassifier
{
    public static RcloneFailureKind Classify(string? error)
    {
        var value = (error ?? string.Empty).ToLowerInvariant();
        if (value.Contains("authentication") || value.Contains("unauthorized") || value.Contains("invalid token") || value.Contains("expired token")) return RcloneFailureKind.Authentication;
        if (value.Contains("permission denied") || value.Contains("access denied") || value.Contains("forbidden")) return RcloneFailureKind.Permission;
        if (value.Contains("not found") || value.Contains("doesn't exist") || value.Contains("does not exist") || value.Contains("no such file")) return RcloneFailureKind.RemoteNotFound;
        if (value.Contains("partial") || value.Contains("incomplete") || value.Contains("transferred") && value.Contains("error")) return RcloneFailureKind.Incomplete;
        if (value.Contains("timeout") || value.Contains("timed out") || value.Contains("connection") || value.Contains("network") || value.Contains("temporarily unavailable") || value.Contains("429")) return RcloneFailureKind.Network;
        return RcloneFailureKind.Unknown;
    }

    public static bool IsRetryable(string? errorCode)
        => string.Equals(errorCode, "RCLONE_NETWORK_FAILED", StringComparison.OrdinalIgnoreCase)
           || string.Equals(errorCode, "RCLONE_TRANSFER_INCOMPLETE", StringComparison.OrdinalIgnoreCase);

    public static string GetErrorCode(RcloneFailureKind kind) => kind switch
    {
        RcloneFailureKind.Authentication => "RCLONE_AUTH_FAILED",
        RcloneFailureKind.Permission => "RCLONE_PERMISSION_DENIED",
        RcloneFailureKind.RemoteNotFound => "RCLONE_REMOTE_NOT_FOUND",
        RcloneFailureKind.Network => "RCLONE_NETWORK_FAILED",
        RcloneFailureKind.Incomplete => "RCLONE_TRANSFER_INCOMPLETE",
        _ => "RCLONE_COPY_FAILED"
    };

    public static string GetUserMessage(RcloneFailureKind kind) => kind switch
    {
        RcloneFailureKind.Authentication => "Rclone 凭据无效或已过期，请重新验证远端配置。",
        RcloneFailureKind.Permission => "Rclone 没有访问该远端的权限，请检查凭据和目录权限。",
        RcloneFailureKind.RemoteNotFound => "Rclone 找不到配置的远端或目标目录，请检查远端名称。",
        RcloneFailureKind.Network => "网络暂时不可用，已保留本地备份并安排有限次重试。",
        RcloneFailureKind.Incomplete => "远端只收到部分内容，已保留本地备份并安排有限次重试。",
        _ => "云端复制失败；本地备份已保留。"
    };
}
