# R13-08 失败分类帮助

日期：2026-09-19  
代码提交：`96a4c6a9`（`补充云端失败分类帮助`）  
分支：`codex/ui-finesse-round2`

## 本阶段收口事实

- 复用现有 `RcloneFailureClassifier` 和稳定错误码；新增无空间 `RCLONE_NO_SPACE`、限流 `RCLONE_RATE_LIMITED`，限流仍进入既有有界退避重试，未改上传、取消、恢复保护或本地副本保留语义。
- `CloudFailureExplanation` 只按已识别的错误码提供下一步：认证检查凭据，空间不足检查本地/远端空间与配额，远端不存在检查远端名称/目标目录，校验失败先查看差异，限流等待窗口并按队列退避。未知错误返回空解释，不猜测原因。
- 维护页使用共享详情样式显示已识别的分类与下一步；原始错误码和错误详情移入默认折叠的“原始诊断”，保留审计信息和可访问名称。没有新增逐条通知或自动修复。

## 已执行的证据

- `python scripts/validate-source.py`：通过（退出码 `0`）。
- `scripts/check-xaml.ps1`：`24/24` 文件通过（退出码 `0`）。
- `git diff --check`：通过（退出码 `0`）。
- 已加入 Core 解释正例/未知错误负例、Worker 分类正例（无空间/限流）和 Playnite 详情绑定源行为夹具；这些夹具用于后续可用环境复跑，不把源码存在写成测试通过。

## 尚未验与真实限制

- Core/Worker/Playnite 定向测试、Release/net462、RenderHarness 和真实 Playnite 尚未执行。主机只有 .NET SDK `9.0.302`，`global.json` 的 `8.0.100` 向上滚动命中缺失 Workload resolver 目录，Worker restore 退出 `1` 且没有 `project.assets.json`；这是构建主机限制，不是代码通过或失败结论。
- 未运行真实 rclone/网络限流、真实远端配额、最终呈现、物理 DPI/跨屏、UIA/IME、ETW 或宿主性能；未写真实存档、媒体、云端或诊断。Demo 原目录不可用，继续沿用恢复生产基线。
- main 用户改动和 `src.zip` 未碰、未合并；当前分支已推送。上一批 `r13-07-source` 的 Contracts 子目录仍被外部进程占用，需先精确清理，不强杀未知进程。

## 下一步

1. 释放并精确清理 `r13-07-source`。
2. 在可用 SDK/Workload 的隔离目录同时重跑 R13-07、R13-08 新增定向夹具及相关回归，确认分类、筛选汇总、分页和详情状态实际编译运行。
3. 两项验证收口后，再核对依赖并推进 `R14-01 归类建议解释`。
