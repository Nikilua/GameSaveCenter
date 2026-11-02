# Round5 U06-02/U08-08 与 U09-05 最终候选

日期：2026-11-03（UTC 日志时间为 2026-11-02）。最终候选源码 SHA：`34d8a99faf7ec827b2fea8303baf8c18a8edbefd`。

## 最近活动选中效果

按用户补充复核首页“最近活动”：原生产 `OverviewView` 模板的 `TaskItemChrome.Margin=4,2` 会让整个选中行内容偏移。提交 `f9d09cb2` 移除该偏移；蓝色边框、强调色和 14 DIP 圆角仍保留。后续几何回归在 Overview 专用模板和共享隐式 `ListBoxItem` 模板中比较未选中、选中、键盘焦点三态的图标、内容与容器槽位，容器 `Margin`、`RenderTransform`、`LayoutTransform` 均保持不变。

共享选择态也逐类核对：公共 `ListBoxItem` 选择层与内容分层；四类生产 `DataGrid`（任务、媒体收件箱、存档、维护）的背景和圆角框是独立覆盖层。DataGrid 为边缘/滚动条留出的 inset 只应用于覆盖层，不改 `DataGridCell` 与文字坐标；现有 `R23ProductionResourceStateBehaviorTests.EachProductionPageGridKeepsSelectedFocusAndDisabledRowStatesAcrossThemes` 在两主题下对四页的单元格/文字几何作前后断言。本轮根链中 R10 类 3/3、R23 几何方法均通过。没有把选中蓝框删掉，也没有添加位移动画。

## 构建与行为检查

- 中文根入口 `GameSaveCenter-一键构建安装运行.cmd` 按实际转交调用 `GameSaveCenter-Run.cmd`；它再执行 `scripts/dev-install-run.ps1` 的完整 Release 链。此前一次入口运行在构建前因 C 盘可用空间低于预检门槛退出码 1，没有开始构建或安装。按清单清理本阶段拥有的旧 `artifacts/gsc-b/` 编译二进制后，同一入口在候选 SHA 上完整完成，最终退出码 0。
- 完整链使用 SDK 8.0.423、MSBuild 17.11.48，8 个项目编译无警告/错误；发布、ZIP/PEXT 打包、安装目录清单与 DLL 版本核验均完成，并启动 Playnite。`scripts/build.cmd` 另按默认参数独立执行其实际职责，调用 `build.ps1` 完成 Release 编译和测试，退出码 0；它不替代安装启动链。
- 中文入口 TRX 共 158 份、2025 个结果：1967 通过、0 失败/错误/超时/中止、58 跳过；其中 57 项是已撤销“今日工作台”架构断言，1 项需真实系统指针，均不计为通过。独立 `scripts/build.cmd` 共 2025 个结果：1957 通过、0 失败/错误/超时/中止、68 跳过；它有同样 58 项，再加 9 项 IPC Named Pipe 客户端测试和 1 项隔离 Worker 重启测试。其守卫检测到当次受限进程无法创建本地 Named Pipe。未改动或扩大 skip；中文完整链在同一 SHA 上这 10 项没有跳过。
- `python scripts/validate-source.py` 退出码 0。WPF 静态检查退出码 0；本地扫描 218 个 XAML、0 错误、208 警告、1327 信息（扫描范围含既有 `.tmp/` 快照，警告不作为通过项）。GitHub 精确候选的 render-qa 和 WPF 静态步骤均成功，渲染预览 artifact 绑定同一 SHA。

本地机器记录、包/安装身份、TRX 汇总和检查结果见 [`local-gates.json`](local-gates.json)。完整测试控制台、所有 TRX 与构建诊断保留在 `artifacts/build-diagnostics/20261102T161832992Z-11ce4c4a/` 和 `artifacts/build-diagnostics/20261102T164541518Z-84099eba/`；最新构建目录与包仍保留在 `artifacts/gsc-b/72b38918/` 和 `artifacts/`。没有复制第二份包或截图。

## GitHub 与完成边界

精确 push run [41477919033（#1434）](https://github.com/Nikilua/GameSaveCenter/actions/runs/41477919033) 对 SHA `34d8a99faf7ec827b2fea8303baf8c18a8edbefd` 的 `windows-build.yml` 两个适用 job 及所有步骤均 completed/success。三个诊断/预览 artifact 均绑定该 SHA，ID、digest、job 和步骤见 [`ci-run-41477919033.json`](ci-run-41477919033.json)。required-checks/分支保护 API 的既有 403 未重试，故只记录工作流成功；U09-06 的 required checks 完整性仍标“远端待验”。

本候选安装版本为 `0.6.73`，Playnite 可执行文件在完整链后启动（进程 ID 39452，2026-11-03 00:45:10）。本轮没有观察或声称真实插件页面渲染、外部 UIA、实体键盘/IME 或物理 DPI 已通过。也没有运行真实存档恢复、媒体删除或云端写入。U09-05 满足；U06-02/U08-08 仍是“代码完成待验收”，U09-04 和 U09-08 继续进行中，U09-06 继续远端待验。

清理前后磁盘、旧输出清单及边界核对见 [`stage-start.json`](stage-start.json) 和 [`cleanup.json`](cleanup.json)。清理只移除了归属于本阶段、已被新冻结候选替代的 4104 个编译二进制（1,447,319,716 bytes）；保留所有 `test-temp`、未知输入法临时文件、最新构建、安装包、完整日志/TRX、当前审计与文档引用。两份重复的阶段清理快照已先与此目录中的归档副本逐项比对 SHA-256，再从 `artifacts/` 移除；移除前后剩余空间和文件哈希记录在 `cleanup.json`。
