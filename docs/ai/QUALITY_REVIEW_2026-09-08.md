# Q4 / Q5 / X2 交付复核与下一轮计划

> 用户要求扩展为可长期连续实施的队列，详见 [32 项 / 8 阶段开发计划](CONTINUOUS_DEVELOPMENT_PLAN_2026-09-08.md)。本报告保留缺陷证据与质量判断，新计划提供依赖、验收、进度记录和连续执行规则。

基线：`97131f0`，2026-09-08。工作树开始时干净，审阅过程中未发生源码变更。本轮只更新文档与原始证据，不修改生产代码、不安装插件、不操作真实用户数据。

## 完成质量结论

上一轮 Q4 的主要修复与 Q5 设置/动效改进已落地；X2 新增状态反馈、运维总览和构建身份。整体可进入发布前收口，已明显改善小窗口可用性。当前问题集中在新增能力的边界和验收覆盖，不需要再次全面重构页面，也不建议立刻增加新的大型业务模块。

| 范围 | 本轮判断 | 说明 |
| --- | --- | --- |
| Q4-01/02 重试与导航 | 主体完成 | 独立媒体重试 IPC、结构化结果、Media/Save 标签绑定均已接入 |
| Q4-03 / 任务紧凑布局 | 视觉改善确认 | Shell 1040×700 的进程表约 7 行、任务表约 6 行；详情按需展开 |
| Q4-04 分页一致性 | 核心风险已处理 | 持久化修订号、触发器、stale token 和有界重载，已不再只是客户端去重 |
| Q4-00 锚点 | 竞态保护与部分行为测试完成 | 已补代际/生命周期失效和 STA 回调测试；大库连续翻页的实际偏移仍需验收 |
| Q5 设置/动效 | 主体完成 | 保存状态可见、关闭动效有即时终态；真实宿主帧耗时未验证 |
| X2-01 状态反馈 | Q6-01/Q6-02 已完成代码收口，真实宿主待验收 | 重试面板输入与媒体状态上下文已补代码/离线测试；真实 Playnite 故障注入和录屏仍未完成 |
| X2-02 运维总览 | 主体完成，需修状态归并 | 最新成功状态可能被过滤掉，旧失败项继续显示 |
| X2-03 构建身份 | 诊断底座完成，发布链未收口 | SkipBuild 可混入旧插件；unknown 身份兼容规则不完整 |

## 独立验证结果

- `scripts/build.ps1 -Configuration Release -OutputRoot .tmp/review-0908-build`：**0 警告、0 错误**；Core **72 通过**，Worker **303 通过 / 1 跳过**，Playnite **368 通过 / 62 跳过**。
- XAML **19/19**；`scripts/validate-source.py` 通过；WPF 静态扫描 `src/GameSaveCenter.Playnite`：**0 errors / 21 warnings / 157 info**。扫描范围不同，不与历史全仓 info 数量直接比较。
- `scripts/render-qa.ps1`：双主题、多尺寸、resize、媒体/维护/任务 Production Shell 几何 **render-qa OK**。人工查看了媒体、维护、任务与设置截图。
- Windows PowerShell STA 中运行 WPF 父 Border/子 Button 命中探针：父级 `IsHitTestVisible=false` 时，子按钮有效值也是 **false**。结合三处 XAML 确认下述 Q6-01，不把直接执行 ICommand 当成鼠标操作成功。
- [原始截图与完整报告](../design/reviews/2026-09-08-quality/)。这些是构造生产 Shell 的离屏 WPF 画面，不是真实 Playnite；Fake 页头仍可能显示首页，不能据此认定生产导航错误。独立 Settings 场景画布为 744×460，Shell 1040×700 的 PageHost 为 715×577。

未执行真实 Playnite 安装、真实云端上传、恢复/隔离文件写操作、真实大库、跨 DPI、高对比度或完整键盘流程。跳过测试不计为通过；以下静态推导问题未冒充运行时复现。

## 必须优先收口的工作包

### Q6-01：状态面板的重试按钮不能鼠标点击（P1）

证据：`MediaCenterView.xaml` 中绑定 `ReloadMediaInboxCommand`、`ReloadMediaWindowCommand` 的两个 WorkspaceStatePresenter，以及 `MaintenanceView.xaml:810` 绑定 `RefreshDiagnosticsCommand` 的面板，都有局部值 `IsHitTestVisible="False"`。后者位于异常/审计列表区域。父级禁止命中会作用于整个按钮子树；即使模板内按钮设 true 也无法修复。

实施：按交互语义调整共享状态模式与三处使用点。Error/Offline 中提供重试的状态面板允许命中；Loading 若应阻止旧列表操作，使用有界遮罩并禁用重试；Ready 隐藏遮罩；Stale 使用非阻塞提示。不要让可见失败面板的鼠标穿透到背后行按钮。

验收：三处真实模板在 Error/Offline 下按钮能被鼠标命中，键盘 Enter/Space 能触发一次正确命令；Loading 不重复提交、不误触背后操作；Ready 无遮挡。测试实例化 View 和模板并检查 HitTest/命令，不只检查 XAML 字符串。附双主题失败态截图。

#### 2026-09-08 复测结果

- 状态：代码与离屏行为已完成，真实 Playnite 宿主仍待验收。
- 根因：三处带重试命令的 `WorkspaceStatePresenter` 曾设置父级 `IsHitTestVisible="False"`；共享模板的空命令 `DataTrigger` 在实际模板中把 Error/Offline 的重试按钮保持为 `Collapsed`。行为测试确认命令对象仍存在，故不是 IPC 或集合刷新根因。
- 修复：移除三处交互面板的局部禁止命中；共享模板改用 `Trigger Property="RetryCommand"`，Loading 明确隐藏重试按钮而保留阻挡面板；补按钮自动化名称。未修改 FusionX 或 Playnite 全局主题。
- 证据：`WorkspaceStatePresenterBehaviorTests` `5/5`；RenderHarness `stateprobe OK`，双主题 Error/Offline/Loading 图位于 `docs/design/reviews/2026-09-08-quality/state-*.png`。Release 全量为 Core `72/72`、Worker `303/304`（1 跳过）、Playnite `376/438`（62 跳过）。本次完整 `render-qa` 仍有 25 个媒体小视口/侧栏快速切换门禁问题，不能写成全量 Render QA 通过；也没有真实 Playnite 录屏。

### Q6-02：媒体状态记忆缺少游戏/模式边界（P2）

证据：`DashboardViewModel.WorkspaceStates.cs` 的 `mediaDetailsLastSuccessUtc`、`mediaInboxLastSuccessUtc` 是全 VM 单值；本轮全文搜索只有成功时赋值，没有切换上下文清空。`ClearSelectedGameDetails` 会清空 Media，但没有清空状态时间；`FailMediaDetailsLoad` 只要任意历史成功时间存在就判 Stale。由此可构造：游戏 A 成功 → 切换无缓存的 B → B 首次读取失败，界面仍称“保留上次成功内容”，并显示 A 的读取时间。待归类/已忽略共享时间也存在同类语义问题。

实施：状态对象关联数据上下文键（游戏 ID、筛选条件、收件箱模式）。同一上下文的刷新失败可保留旧数据并标 Stale；新上下文没有对应缓存时判 Error；切换时清理旧错误/时间或恢复该上下文自己的缓存。统一用有效状态派生标题、消息、图标：目前 PresenterState 可被 Worker 离线强制覆盖，但标题仍来自局部 Ready/Stale，容易出现离线面板配空标题或过期文案。

验收：A 成功/B 首失败、两种收件箱模式切换、同游戏改筛选后失败、Worker 离线/恢复、旧请求晚返回。断言状态、时间、标题、集合身份和重试目标；同上下文草稿与选择仍保留。

#### 2026-09-08 复测结果

- 状态：代码与离线测试已完成，真实 Playnite/FusionX 状态切换、故障注入和视频式复测待宿主验收。
- 修复：新增 `MediaWorkspaceStateCache`，详情上下文由游戏 ID、媒体筛选、搜索词组成，收件箱上下文由模式组成；同上下文失败才可进入 Stale，新上下文没有成功缓存时进入 Error。筛选、搜索和选中游戏变化会立即推进媒体分页代际、取消旧请求并重置分页状态，旧响应不能覆盖当前上下文。
- 测试：`MediaWorkspaceStateCacheTests` `4/4`；Release 全量 Core `72/72`、Worker `303/304`（1 跳过）、Playnite `380/442`（62 跳过），构建 `0 warning/0 error`，XAML `19/19`。本项无 XAML 改动，未把上一轮完整 `render-qa` 中的 25 个独立小视口/侧栏问题归因于本修复。
- 尚未把“集合身份、编辑草稿和真实重试目标”写成完整 VM/宿主行为通过；当前证据证明状态缓存转移和晚到响应保护，不证明用户视频在真实宿主已解决。

### Q6-03：运维总览先筛失败再去重，会保留已过期告警（P2）

证据：`DashboardViewModel.MaintenanceActions.cs:85` 合并 `Snapshot.CloudTransfers.Items.Concat(CloudTransferItems)`，随后先 `Where(IsCloudAttentionItem)` 再按 TransferKey 分组取 Last。例：Snapshot 中 K=Failed，已刷新明细 K=Uploaded；Uploaded 在去重前被丢弃，失败 K 仍进入总览。反向场景也可能被旧分页结果覆盖新快照，因为来源顺序不是新鲜度。

实施：先按身份合并各来源最新版本，再筛需要关注的状态；明确比较 UpdatedUtc/数据修订与来源权威性，并处理同时间不同状态。已删除/已不适用项不能无限从旧缓存复活。维护列表计数与云端摘要不一致时显示明确刷新语义。

同时修正时间标签：当前把 `LastAttemptUtc` 填到统一“上次验证”，失败上传尝试不等于验证成功；隔离账本 `UpdatedUtc` 也只是状态更新时间。为巡检、上传、账本分别显示“上次验证/上次尝试/账本更新”。

验收：旧失败+新成功、新失败+旧成功、两个失败不同时间、已删除/未加载、同时间冲突等表驱动用例；总览告警与明细一致，不将上传尝试称为验证成功。可先抽纯逻辑 resolver，再由 VM 装配。

### Q6-04：构建身份还需覆盖 SkipBuild 与 unknown（P1，发布路径）

证据：`scripts/package.ps1` 为本次 Worker publish 注入当前 HEAD，但 `-SkipBuild` 直接复制现有插件 DLL，仅校验 FileVersion 是否匹配公共版本。旧插件 A 与当前 Worker B 同为 0.6.73 时可通过打包检查；`WorkerLauncher.IsBuildIdentityCompatible` 又会拒绝两个非空不同身份，导致包可生成而配对不可用。这是脚本调用链推导，本轮未生成混合包、未执行安装。

另外，`Directory.Build.props` 缺提交时生成 `0.6.73+unknown`，但兼容方法只对空字符串放行。known 与 unknown 会被当作两个已知不同构建处理，与“两个身份已知才拒绝复用”的约定不一致。普通本地构建正可能产生 unknown。

实施：读取包内插件、Worker 和共享程序集的实际 InformationalVersion；SkipBuild 必须证明产物同源，否则失败并给出重建指令。unknown 使用显式解析状态，按既定公共版本/协议兼容路径降级并提示身份不可验证。补工作树有改动时的身份策略（拒绝发布或标 dirty），避免把 HEAD 冒充未提交内容的来源；临时环境变量需 finally 恢复，Git 读取失败不得沿用外部残留身份。

验收：正常构建同身份、同公共版本旧插件+新 Worker、known/unknown、旧 Worker 无字段、无 Git、脏工作树；包内校验在生成成功提示之前完成。使用隔离 stage 和替代产物，不覆盖用户正在使用的安装。

## 让下一轮界面变化可见的计划

| 顺序 | 工作包 | 可见结果 | 验收边界 |
| --- | --- | --- | --- |
| 1 | Q6-01 与 Q6-02 | 失败页可重试，切换游戏后不会带着其他游戏的旧时间 | 状态矩阵、鼠标/键盘、真实绑定 |
| 2 | Q6-03 与 Q6-04 | 告警真正消失、运维时间用词准确；包的构建来源可核对 | 合并逻辑测试、隔离打包负例 |
| 3 | UI6-A 状态/运维视觉验收 | Loading、空、首次失败、旧数据失败、离线、恢复六态都有明确样例；运维面板含完整真实形状记录 | 补 FakeDashboardData 属性或用可注入 VM；运行所有相关子标签和操作 |
| 4 | UI6-B 设置首屏减负 | 小窗口先看到字段和保存状态，详细校验原因放字段附近，避免头部重复大段报错 | 同画布前后图；保持 Playnite 保存/取消契约、路径校验、字体可读性 |
| 5 | UI6-C 维护信息密度与大库 | 总览先给少量最需处理项，其余可展开/分页；时间/游戏/动作更容易扫读 | 不全量渲染隔离账本；保留真实 EntryId、确认与文件安全语义 |
| 6 | 发布验收 | 真实安装身份、DPI、键盘与流畅度证据 | 先交付修复和待发布包，再按已授权安装范围执行真实宿主验证 |

UI6-A 尤其重要：`FakeDashboardData` 未提供本轮新增 `MaintenanceActionItems` 和三组状态 Presenter 绑定面，当前 render-qa OK 不能证明运维记录、失败重试和状态转换的显示。应将关键绑定缺失列为失败，并给每个新增状态准备完整数据；不要把 Fake 缺字段造成的空白认定为生产缺陷。

UI6-B 是优化建议，不是对真实用户配置损坏的判断。当前设置截图因审计环境未找到 Worker 而显示校验失败；需要正常、未保存、验证失败三套可控夹具。保留页头的一行保存状态，把同一错误的解释集中到一个位置，减少外层嵌套卡片占高。

UI6-C 的扩展性依据：运维隔离账本当前从只读 IPC 全量返回，再用 ItemsControl 渲染。数据小时可用，大量未协调记录时会增加绑定和布局成本。先以 100/1000 条夹具测量，再决定分页和虚拟化；不为少量数据盲目改控件。

速度方面，本轮离屏侧栏单次最大 frame gap 367.3ms、快速切换 105.5ms，最终均 settled；不据此宣称生产卡顿或流畅。后续应在真实宿主采集输入到首帧/终态、主线程耗时、重复切换内存与容器数，再决定优化。当前没有必要增加更多模糊或逐行动效。

每包独立提交、跑针对性行为测试、附同尺寸前后原图并同步日志。不要重复重做已完成的 Q4 主体；不要通过不断新增源码字符串断言替代用户实际操作验证。
