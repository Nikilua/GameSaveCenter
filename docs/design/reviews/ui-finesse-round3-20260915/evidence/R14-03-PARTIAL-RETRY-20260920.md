# R14-03 部分成功处理证据

日期：2026-09-20  
代码提交：`aef251b1`（`补充媒体批量失败重试`）  
分支：`codex/ui-finesse-round2`

## 实现事实

- 复用 Worker 已有的逐项 best-effort 结果：批量归类、忽略和恢复分别继续处理其他稳定媒体 ID，成功项进入 `UpdatedItems`，失败项保留 `MediaId` 与 `ErrorMessage`。
- Playnite ViewModel 保存最近批次的失败集合、操作类型和归类目标；失败集合按稳定媒体 ID 去重，成功项不会被加入重试集合。
- Media 收件箱底栏以有限高度/Recycling 列表逐项显示失败 ID 和原因；“仅重试失败项”先确认，再使用上次失败集合和原目标提交，不使用当前 DataGrid 选择，不扩大请求范围。
- 重试后失败集合替换为本次仍失败项；全部成功时提示区隐藏。原有取消、错误、刷新、归档副本保留和当前列表模式语义保持。

## 夹具与验证

- Worker 增加隔离 SQLite/fake 行为夹具：一个有效忽略项成功，一个缺失媒体项失败；断言成功项已变为 `Ignored`，失败项保留稳定 ID 和非空原因。
- Playnite 源契约夹具覆盖失败集合、重试命令、逐项原因和“成功项不会再次执行”文案；不是把 UI 字符串作为唯一行为证据。
- `python scripts/validate-source.py`：通过。
- `scripts/check-xaml.ps1 -ProjectRoot ...`：24/24 通过。
- `git diff --check`：通过。
- Worker/Playnite 定向测试、Release/net462、RenderHarness 和真实宿主未执行；当前 Core/Worker 项目引用与 SDK/Workload 环境仍不能产出可签收的运行时测试结果。

## 边界与清理

- 仅使用合成数据、fake 服务和隔离目录，未读取或写入真实存档、媒体、用户云端或对外诊断；未绕过被拒绝的 ETW/系统跟踪权限。
- Demo 原目录不可用，视觉基准沿用恢复生产基线。离屏/静态检查不等价真实 Playnite 呈现、物理 DPI/跨屏、UIA/读屏/IME、presented frame、ETW 或宿主性能。
- 本批没有留下新的 `artifacts/` 或 `.tmp/` 产物；main 用户改动、`src.zip` 未修改、未合并。

下一可执行任务：在可用 SDK/Workload 环境补跑 R13-07/R13-08/R14-01/R14-02/R14-03 定向夹具与相关回归；实际未验边界是 Worker/Playnite 运行时与真实宿主呈现，之后推进 R14-04 撤销边界说明。
