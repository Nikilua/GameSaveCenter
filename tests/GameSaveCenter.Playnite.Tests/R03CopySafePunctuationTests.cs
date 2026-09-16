using System;
using System.Threading;
using System.Windows.Controls;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R03CopySafePunctuationTests
{
    [Fact]
    public void TaskFilterLabelsUseFullWidthColonsInTheProductionView()
    {
        Exception? failure = null;
        string[] labels = Array.Empty<string>();

        var thread = new Thread(() =>
        {
            try
            {
                var view = new TaskCenterView();
                var viewType = typeof(TaskCenterView);
                labels = new[]
                {
                    ((TextBlock)viewType.GetField("TaskStatusFilterLabel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(view)!).Text,
                    ((TextBlock)viewType.GetField("TaskTypeFilterLabel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(view)!).Text,
                    ((TextBlock)viewType.GetField("TaskHistoryScopeLabel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(view)!).Text,
                    ((TextBlock)viewType.GetField("TaskHistoryRangeLabel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(view)!).Text
                };
            }
            catch (Exception caught)
            {
                failure = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(failure);
        Assert.Equal(new[] { "状态：", "类型：", "范围：", "时间：" }, labels);
    }

    [Fact]
    public void FailureDetailUsesDisplaySeparatorsWithoutChangingTechnicalCodeOrRawMessage()
    {
        const string errorCode = "RCLONE_COPY_FAILED";
        const string errorMessage = "无法复制 C:\\Saves\\A:1；请重试";
        var task = new TaskStatusDto
        {
            State = TaskState.Failed,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage
        };

        Assert.Equal(errorCode, task.ErrorCode);
        Assert.Equal(errorMessage, task.ErrorMessage);
        Assert.Equal($"错误码：{errorCode}；{errorMessage}", task.DetailMessage);
        Assert.DoesNotContain($"{errorCode}: ", task.DetailMessage);
        Assert.Contains("C:\\Saves\\A:1", task.DetailMessage);
    }

    [Fact]
    public void MissingErrorCodeAndNonFailureMessageAreNotGivenAnOrphanLabel()
    {
        var missingCode = new TaskStatusDto
        {
            State = TaskState.Failed,
            ErrorMessage = "Worker: 原始失败原因"
        };
        var running = new TaskStatusDto
        {
            State = TaskState.Running,
            Message = "Worker: 正在处理"
        };

        Assert.Equal("Worker: 原始失败原因", missingCode.DetailMessage);
        Assert.Equal("Worker: 正在处理", running.DetailMessage);
        Assert.DoesNotContain("错误码：", missingCode.DetailMessage);
        Assert.DoesNotContain("错误码：", running.DetailMessage);
    }
}
