# R16-01 设置搜索定位证据

日期：2026-09-20  
实现提交：`a4e35578`（`补齐设置搜索定位`）  
分支：`codex/ui-finesse-round2`

## 实现事实

- 设置页头部新增轻量搜索框和结果摘要；搜索项通过 `GameSaveCenterSettingsView.SearchTerms` 附加属性登记，构造时扫描五个现有分类的逻辑树，不复制设置 DTO、服务或绑定。
- 输入查询时只切换匹配字段宿主和分类可见性，并定位到第一个命中分类；首次搜索记录原分类。清空搜索恢复原分类和全部字段可见性，搜索文本不写入配置对象，也不改变保存/取消编辑状态。
- 命中字段仍使用原来的 TextBox、ComboBox、CheckBox、ToggleSwitch 和绑定；验证错误定位会先清空搜索，再沿用已有分类、滚动和焦点路径。
- 查询支持中文连续短语和空格分隔词；无命中显示明确结果，不把无结果当成配置错误或自动修改值。

## 自动与受控行为验证

- 外部隔离源码副本 `D:\workplace\github\GameSaveCenter\.tmp\r16-01-source` 执行 Release solution 单节点构建：`0 errors/10 warnings`。警告为离线 NuGet `NU1900` 与既有 `src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml.cs:664` 两条 nullable warning；本次搜索代码不再产生新增 nullable warning。
- `R16SettingsSearchBehaviorTests` 独立 testhost `1/1`：合成设置和隔离目录下，搜索“恢复巡检间隔”切到自动化分类，匹配数值字段可见且可编辑，常规 Worker 字段隐藏，配置路径不变；清空后回到原常规分类，字段恢复可见且没有 pending edit。
- `SettingsValidationSourceTests` `1/1`；既有 `SettingsValidationNavigationBehaviorTests` 和 `SettingsDraftLifecycleBehaviorTests` 分别独立运行，各 `1/1`。
- `validate-source.py` 通过；XAML 结构门禁 `24/24`；`git diff --check` 通过；WPF 技能静态检查 `0 errors/28 warnings/177 info`。联合筛选上述 WPF 测试时出现 `2 passed/2 failed` 的既有 AppDomain `Application` 多实例夹具冲突，分离 testhost 后四项均通过，未作为产品失败隐藏。

## 公共门禁与边界

- 保留现有游戏选框、滚动条、命令绑定、设置 DTO、保存/取消、错误/验证、恢复保护、有限列表性能和 Playnite `net462` 路径；本批只增加设置字段可见性索引和受控定位。
- 只使用合成设置、fake/隔离目录和受控 STA WPF；未读写真实存档、媒体、云端、用户诊断或系统剪贴板。Demo 原目录不可用，沿用恢复生产基线。
- 未验真实 Playnite/package-host 中的最终布局、浅/深主题屏幕呈现、RenderHarness presented frame、DPI/跨屏、UIA/屏幕阅读器、IME、ETW 或宿主性能；linked worktree 的 WPF `obj` 仍受 `Access denied`，因此构建使用外部隔离副本。main 的用户改动和 `src.zip` 未碰，未合并。

下一可执行任务：`R16-02 策略差异预览`，先核对现有策略/模板 DTO、继承值与显式覆盖值，再以只读差异预览和取消无写入为边界推进。
