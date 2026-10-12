# R00/R01 当前源码复核

日期：2026-10-12。源码身份：`f13b9e05de0cf502ac61e97b0fbad8433b339b3f`（main）。本包用于校正该身份下受影响的 R00/R01 证据；它不改变 192 项任务状态，也不代表真实 Playnite、物理 DPI 或最终显示帧已验。

## 构建身份与结果

- 本机 Windows 10.0.22000，.NET SDK `8.0.423`；目标输出位于仓库 `.tmp/r00-r01-refresh-20261012/build`，源码根绑定到当前仓库，`GscBuildCommit` 为上述完整 SHA。`RepositoryIdentityTests` 为 `2/2`。
- 受影响的定向 WPF/source 测试共 `40/40` 通过、0 失败、0 跳过，保存了 9 份 TRX。RenderHarness Release `0 warning / 0 error`；三个探针均报告 `WorkingTreeClean=True` 和源码 SHA `f13b9e05...`。
- `GameSaveCenter.Playnite.dll` SHA-256 `53C2E5D83BE9023C532393B98A322C89E91A95676229FF72B79E52A5413EBE2B`，MVID `7a91cb9a-d3e7-4d29-b3bc-fe1ec8aa6b76`。
- `GameSaveCenter.Playnite.Tests.dll` SHA-256 `7D8C3760093A4161EA1DFFCFACD15F0DC8CAF60698F50C2AF3E3FAD6E1022E20`，MVID `30917483-b628-4be9-8d60-974fc9e3095e`。
- `GameSaveCenter.RenderHarness.exe` SHA-256 `AE2475467ABA3E079A4B6C9492A4E8A45FFD0B0D9F02DFD439CCB0BB74088B90`，MVID `b12cab82-9963-402e-b6ae-b9be302f31c3`。
- 没有生成或安装 Playnite 包；`packageCommit` 与 package identity 仍为未提供。源码/TRX身份与包身份分开记录。

## 按条目对应的复核

- **R00-01/02、R00-05**：`UiDiagnosticsExporterTests` 对比/组合态与 `WpfUiResourceDictionaryTests` ContextButton Light/Dark 合计 `5/5`。Light/Dark 生产资源探针各覆盖 88 个渐变按钮状态样本，0 对比度违规；数字列 4/4 横纵向适配，窄列和黑字暗底负例均被检测。上下文禁用样式仍由单一 Chrome opacity 表达。
- **R00-03、R01-04**：`UiFinesseFoundationTests 9/9`；侧栏取消、终态和卸载时钟清理的 Dispatcher 行为 `3/3`。覆盖动画完成后保留终值、重入连续性、取消不回跳与 unload 清钟；不推断 Windows 动画偏好通知或 Playnite 生命周期已验证。
- **R00-04**：`LargeLibraryPerformanceTests 5/5`。2000 项搜索的 30 个不同查询全部改变可见结果集（30/30）；原始查询和每个样本见 [large-library.txt](benchmarks/large-library.txt)。本次 p50 `45 ms`、p95 `60 ms`、max `60 ms`；不可能命中数量的等待负例在 1 秒内退出。数据为合成列表，基准仅代表本机 WPF ViewModel 路径。
- **R00-06**：几何测试 `3/3`，Light/Dark RenderHarness `mediageometryprobe OK`（10 个组合）。正常窗、水平滚动与替代行密度均报告 4/4 完整行；短窗明确报告 `short-window-page-fallback` 并保留页面滚动；故意阻断父级后报告不可达和 HIGH 门禁。行数按实际行与 DataGrid 可见边界交集计算，含表头、水平条、frame padding/border。
- **R00-07**：`UiAuditSourceTests 6/6`，`toolbarprobe OK`。长设置表单识别为 `settings-form` 并被排除、没有工具栏警告；同一滚动祖先中的注入超宽动作行产生 `TOOLBAR_HORIZONTAL_OVERFLOW`，不可达动作行产生 `TOOLBAR_UNREACHABLE`。报告保留分类理由、可见几何和滚动祖先。
- **R00-08**：`GamePickerKeyboardBehaviorTests 6/6`，覆盖无结果 Enter、活动文本组合/IME 路径、方向键、可见候选确认、Escape 和焦点返回。受控 WPF 事件不等价 OS IME 或真实 Playnite 输入。
- **R01-01**：当前隔离输出的 repository/build identity 测试 `2/2`；程序集内的源码根和构建提交与当前 checkout 匹配，测试源码扫描确认 reader 不从 test output 目录回溯仓库。该复核没有运行第二个 checkout 的实际跨 checkout build；旧跨 checkout 证据仍单独保留。
- **R01-05**：负例注册表 `1/1`，N01–N05 均被各自检测器拒绝：暗底黑字、行高够但数值列过窄、无可见候选 Enter、固定父级超宽子项、Loading 状态底层命中。夹具不进入生产入口。
- **R01-02/03/06/07**：最新 freshness 检查未命中这些记录登记的源码路径，因此保留原证据与原 source commit；本次双主题探针额外记录当前数值列正例/负例。未据此声称重跑了 R01-03 索引抽查、R01-06 宿主归档检查或 R01-08 skip 分类。

逐条结果见 [freshness-summary.json](freshness-summary.json)。更新 10 条受影响记录后为 `14 fresh / 0 stale`；这是源码证据新鲜度，不是 14 项产品或宿主验收。

## CI 未知边界

推送提交对应的 [Actions run 38222818374](https://github.com/Nikilua/GameSaveCenter/actions/runs/38222818374)（run 930）已结束为 failure，失败 job 为“编译、测试与打包”，失败步骤为“编译与测试”。CI workflow SDK 为 `10.0.401`；本机 SDK `8.0.423` 的通过结果不能替代该 CI 结果。

下载完整 run logs 返回 HTTP 403：`Must have admin rights to Repository.`。Run artifact 列表可读到 `GameSaveCenter-build-diagnostics`（488,578 bytes），但匿名下载返回 HTTP 401。现有可读数据只确认失败步骤，不含失败命令、断言、堆栈或实际 SDK 诊断；失败原因未知。本轮没有据此盲改代码。详见 [ci-log-access.txt](ci-log-access.txt)。

## 证据边界

WPF 窗口、主题及样本都由本机受控离屏 harness 生成，DPI 为 logical `1.00`。没有启动 Playnite，没有使用真实存档/媒体/云端，没有通过 ETW 或系统跟踪绕过权限；没有验证物理跨屏、OS IME、Narrator/UIA、用户安装包或最终呈现帧。`artifacts/` 与 `.tmp/` 的可再生截图不归档；本目录仅保留报告、TRX、探针输出和原始搜索样本。
