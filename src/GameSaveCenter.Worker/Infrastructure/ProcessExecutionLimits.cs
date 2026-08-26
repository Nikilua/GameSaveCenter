namespace GameSaveCenter.Worker.Infrastructure;

internal static class ProcessExecutionLimits
{
    internal const int MaximumOutputBytes = 4 * 1024 * 1024;
    internal const string OutputLimitExceededCode = "PROCESS_OUTPUT_LIMIT_EXCEEDED";
    internal const string TimeoutCode = "PROCESS_TIMED_OUT";
    internal const string ExecutableNotFoundCode = "EXECUTABLE_NOT_FOUND";
}
