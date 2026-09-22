# R14-01 归类建议解释

日期：2026-09-19  
代码提交：`7735cd7c`（`补充媒体归类建议依据`）  
分支：`codex/ui-finesse-round2`

## 已实现事实

- 先复用 `MediaSyncService.BuildClassificationSuggestion`、现有媒体来源规则、游戏会话、进程映射和文件名匹配，不新增服务、IPC、归类移动或持久化表。
- `MediaClassificationSuggestionDto` 新增结构化 `Evidence`：来源规则显示真实目录/模式，会话显示实际时间窗与进程（未记录结束时明确显示“未记录结束”），进程映射显示可执行文件与映射目标，文件名依据继续保留。
- 多候选建议不伪造目标，预览卡逐条保留候选游戏和对应依据；没有任何依据时 `HasEvidence=false`，界面显示“待判断”，既有低置信度与不可应用门禁保持不变。
- Media 预览继续使用既有有限高度、Recycling 虚拟化、Inspector 内滚动、命令绑定和确认/应用/撤销链；本批不移动文件、不删除媒体、不写真实存档或云端。

## 夹具与验证

- Worker 合成 SQLite 夹具扩展了来源规则正例、多会话候选、进程映射正例和无依据负例；Core 增加结构化依据显示/待判断负例；Playnite 源夹具检查预览绑定、三类依据和无依据文案；RenderHarness 合成预览包含有依据与无依据两类样本。
- `python scripts/validate-source.py`：通过。
- `scripts/check-xaml.ps1 -ProjectRoot`：24/24，通过；`git diff --check`：通过。
- Contracts/Core Release 隔离输出：均 `0 warning / 0 error`。该隔离输出已清理。
- Core 定向测试尝试未进入 testhost，项目引用目标框架评估以 `0 warning / 0 error` 退出 `1`，因此没有测试通过数可记录；Worker restore 仍受当前 SDK/Workload 环境阻塞，Playnite 定向测试未执行。

## 未验边界

- 当前主机只有 .NET SDK `9.0.302`；`global.json` 的 `8.0.100` 向上滚动仍命中缺失 Workload resolver，不能据此宣称 Worker、Playnite `net462` 或 RenderHarness 通过。
- 未运行真实 Playnite/package-host、最终屏幕呈现、物理 DPI/跨屏、UIA/读屏、IME、ETW 或宿主性能；Demo 原目录不可用，沿用恢复生产基线。
- 当前新建的 R14 隔离构建目录已清理；此前用于 R13 复核的 `.tmp/r13-verify-source` 曾短暂被占用，阶段末已精确删除，未强杀未知进程。main 用户改动保持未触碰、未合并。
