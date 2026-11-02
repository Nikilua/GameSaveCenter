# U09-05 / U09-06 冻结候选门禁记录：77482a9a

日期：2026-11-03。生产候选 `77482a9a56f8d1507de3cf819c94559388dddc4f`（`修正共享导航焦点框位移`）已提交并推送 `origin/main`。源码树 `969c175762168e3de37e548d447dc5f659986549`、测试树 `622aa8c606d8b2ef1a963c34947dff27db3daf2b`、脚本树 `efbde3774901e75ad253a2d77cd90f83796e2931`。此证据区分源码候选的成功门禁与本机入口拒绝；后者尚未达到U09-05要求。

## 选中/焦点实现与UI_CHANGE_GATE

- 首页最近活动先前已移除 `TaskItemChrome.Margin=4,2`，保留蓝色圆角选框且不移动图标和整行。
- 本候选继续把共享 `AcrylicNavItem` 键盘焦点描边移到独立 sibling overlay。焦点态不再把内容边框加粗到2 DIP，也不叠加第二道内框；选中色、焦点色、RadioButton导航、Tag/Checked、鼠标命中与UIA契约保持。
- SDK 8.0.423 Release 的定向R23 WPF用例2/2通过、0失败、0 skip；两主题覆盖未选中、选中、选中并聚焦及焦点移出。TRX于提交前对与候选内容完全相同的工作树生成，摘要和SHA见 `local-gates.json`，不把其构建元数据称为候选SHA。
- 提交后 `python scripts/validate-source.py` 退出0。WPF静态扫描218份XAML、0错误、208警告、1327信息；warning/info包括仓库保留的历史 `.tmp` 快照。
- 提交后 `scripts/render-qa.ps1` 退出0：Light/Dark共394 PNG、0 `PROBLEM`、逻辑DPI 1.00。报告明确绑定候选SHA、`WorkingTreeClean: True`；固定样本UTC `2026-11-03T00:00:00Z`。报告SHA-256为 `560A572CA52573D477F01F6DF25D8DD7D3A96E2E78B3EDC8C64A71C16695F3DB`，本地报告与截图保留于 `artifacts/ui-qa/U06-02-selection-frame-align-20261102/`。离屏数据不证明Playnite真实呈现。

## 本机完整入口结果

已核实实际存在的 `GameSaveCenter-一键构建安装运行.cmd`、`GameSaveCenter-Run.cmd` 与 `scripts/build.cmd`。中文包装器按实际实现调用英文根入口；本次同一条链返回退出码1。错误发生在进入构建前：子PowerShell因 `ConstrainedLanguage` 拒绝 dot-source（`DotSourceNotSupported`）。独立 `scripts/build.cmd` 也在构建前以退出码1遇到同类语言模式拒绝。两入口原始控制台日志及SHA见 `local-gates.json`，没有测试/TRX、发布包、安装报告或当前候选安装身份产生。

不得把上一候选 `34d8a99f` 的成功安装/启动替代本候选；不得把GitHub打包、定向 `dotnet test` 或局部构建写成中文根入口通过。当前构建脚本与入口未变，不在同一被拒绝的执行上下文重复试探。要完成硬门禁，下一步是在组织允许的标准 PowerShell FullLanguage 开发环境中运行既有真实入口；若当前限制来自组织策略，由管理员恢复获准的执行上下文。不要关闭或绕过策略/门禁。

## GitHub Actions

精确候选的 `.github/workflows/windows-build.yml` push run [41498607041（#1436）](https://github.com/Nikilua/GameSaveCenter/actions/runs/41498607041) 于 `2026-11-02T19:15:13Z` 完成成功。基线SDK job `125459993079` 与SDK10.0.401复核job `125459992623` 均 `completed/success`，所有适用步骤成功，包括编译/测试、Render QA、WPF静态审查、Playnite扩展包、上传与诊断保留。三个artifact的ID、digest与精确head SHA见 [`ci-run-41498607041.json`](ci-run-41498607041.json)。这证明远端构建/测试/打包，不证明本机中文入口安装和Playnite启动。

前一纯文档提交 `e5bb6db3` 的run [41493023902（#1435）](https://github.com/Nikilua/GameSaveCenter/actions/runs/41493023902) 亦完成成功；它只验证当时文档提交，artifact记录见 [`ci-run-41493023902.json`](ci-run-41493023902.json)，不代替当前候选。

workflow查询权限可读取，但required-check/分支保护查询此前返回403；按要求不重复访问。故U09-06仍为“远端待验”。下一步由有权限维护者只读核对当前仓库required checks和规则集，确认其与本run适用job一致；本地和GitHub的成功都不能代替这项权限受限的核对。

## 未验范围

U06-02/U08-08为代码完成待真实宿主验收；本机未安装/启动该候选，未观察Playnite页面、外部UIA、实体键盘/IME或物理DPI。没有执行真实存档恢复、媒体删除或云端写入。U09-05因两个本机入口均未开始构建而保持进行中；U09-04性能审计与U09-08整轮交付审查亦未结项。80项当前计数：5项已满足、71项代码完成待验收、3项进行中（U09-04/05/08）、1项远端待验（U09-06）。

空间预检、已确认的阶段清理、上一候选包/日志归档路径和最终磁盘余量见 [`cleanup.json`](cleanup.json)。本机原始入口控制台输出留在该工作树 `artifacts/`，哈希在 [`local-gates.json`](local-gates.json)。
