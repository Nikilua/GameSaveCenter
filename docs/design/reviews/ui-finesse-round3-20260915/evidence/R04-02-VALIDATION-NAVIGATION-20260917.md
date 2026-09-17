# R04-02 错误摘要导航证据（2026-09-17）

## 结论

R04-02 在当前可控范围内已满足：设置页顶部错误详情现在把可定位的校验消息渲染为 Hyperlink；点击后选择对应分类、将字段带入现有 `SettingsScroller` 并聚焦。字段同时保留错误原因的 Automation HelpText，隐藏的恢复巡检字段在关闭巡检时不再阻止保存。代码提交为 `6524f944f05921f02f0b32b7f5aa3dfa702ac165`（`补齐设置校验错误定位`），已推送到 `codex/ui-finesse-round2`。

- 先核对并复用了现有 `GameSaveCenterSettings.VerifySettings`、页头摘要/详情、字段绑定校验模板、设置分类 ListBox 和 `SettingsScroller`；没有把 main 的旧实现带入当前分支，也没有替换游戏选框或滚动条系统。
- 设置视图为已存在的路径、数量、压缩、毛玻璃、间隔和统计窗口控件登记字段目标。模型错误按真实中文错误文案映射到字段；WPF `Validation.GetErrors` 也会加入启用字段的字段级错误。摘要详情中有目标的消息使用 Hyperlink，点击后调用 `BringIntoView`、`Focus` 和 `Keyboard.Focus`；字段 HelpText 与链接的 Automation Name/HelpText 含具体原因和目标字段。
- `HealthInspectionEnabled=false` 时，`HealthInspectionIntervalMinutes` 与 `HealthInspectionStaleAfterDays` 的无效旧值不再进入 `VerifySettings`；UI 仍通过 `IsEnabled` 隐藏/禁用对应输入，保存、取消和其他安全语义未改。
- 新增 `SettingsValidationNavigationBehaviorTests.ErrorLinkSelectsAutomationTabAndFocusesTheFixableField`：隔离临时目录中使用合成 Worker/目录和真实设置页，在约束滚动视口下点击恢复巡检间隔错误链接，实测选择自动化 Tab、字段可见、字段取得键盘焦点、滚动偏移大于 0，并读出字段 HelpText 原因；链接 Automation Name 也包含完整错误原因。

## 验证结果

- `6524f94` clean commit 隔离 Release：XAML 结构 `24/24`；解决方案编译 `0` 警告、`0` 错误；`python scripts/validate-source.py` 通过。
- clean commit 隔离产物的 R04-02 定向 Playnite 测试 `5/5` 通过：`SettingsValidationSourceTests`、`SettingsPathValidationTests`、`SettingsValidationNavigationBehaviorTests`；包含 3 个已有设置路径/源校验，以及新增的禁用巡检字段负例和真实 WPF 跨 Tab 行为。
- RenderHarness clean 报告：`Commit=6524f944f05921f02f0b32b7f5aa3dfa702ac165`、`WorkingTreeClean=True`、`DpiScale=1.00`（offscreen logical DIP）、`Themes=light,dark`、`357` 张 PNG，报告末尾为 `render-qa OK`。新增设置导航探针记录 `links=1`、`selectedCategory=1`、`fieldVisibility=Visible`，压缩字段 HelpText 为“压缩等级必须为 -7–22；zstd 建议使用 3。”；人工抽查 Light/Dark `Settings-1040x700` 与 Light `Settings-1366x768`，页头摘要、定位入口、折叠详情和现有设置布局保持可读。
- 同代码内容的 Playnite 全量 testhost 复跑曾分别报告 `553` 通过/`18` 失败/`57` 跳过和 `548` 通过/`23` 失败/`57` 跳过（总计 `628`）；失败集中在已有 WPF 测试的 `PresentationSource` 为空、Visual 上级不匹配、缩略图时序和动效时序，不命中 R04-02 定向测试。没有把该环境性全量失败写成通过，也没有放宽门禁；后续可在稳定的 WPF testhost/宿主环境复测。

可复现命令：

```powershell
powershell.exe -ExecutionPolicy Bypass -File scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/r04-02-build-clean
dotnet vstest .tmp/r04-02-build-clean/bin/GameSaveCenter.Playnite.Tests/Release/net472/GameSaveCenter.Playnite.Tests.dll /TestAdapterPath:.tmp/r04-02-build-clean/bin/GameSaveCenter.Playnite.Tests/Release/net472 /TestCaseFilter:"FullyQualifiedName~SettingsValidationSourceTests|FullyQualifiedName~SettingsPathValidationTests|FullyQualifiedName~SettingsValidationNavigationBehaviorTests"
python scripts/validate-source.py
$env:GSC_BUILD_COMMIT = (git rev-parse HEAD); $env:GSC_SOURCE_ROOT = (Get-Location).Path; powershell.exe -ExecutionPolicy Bypass -File scripts/render-qa.ps1 -Configuration Release -Output .tmp/r04-02-render-clean
```

## 边界与保留项

验证使用合成设置、fake Worker 文件、隔离临时目录、真实生产 WPF 控件和 STA Window；没有读取或修改真实存档、删除真实媒体、写用户云端或发送诊断。RenderHarness 的 `fieldIsVisible=False`、`fieldFocused=False` 和 `scrollOffset=0` 是未连接真实 PresentationSource 的离屏事实，不覆盖 STA Window 已测的焦点/滚动，也不代表真实桌面 presented frame。Windows 屏幕阅读器实际朗读、Playnite 嵌入设置宿主、物理 DPI/跨屏、真实输入法、ETW、宿主帧率和耐久性能仍未验。命令/Binding、取消/错误语义、恢复保护、有限列表、当前游戏选框、滚动条和 Playnite/net462 目标均保持。

下一可执行任务：R04-03 未保存离开保护；继续先核对已有编辑/取消语义，再以小批量行为测试推进。
