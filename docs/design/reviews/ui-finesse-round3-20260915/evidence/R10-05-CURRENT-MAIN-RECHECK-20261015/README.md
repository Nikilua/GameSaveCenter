# R10-05 筛选预设当前 main 复核

更新：2026-10-15；分支 `main`；代码测试基线 `e2353aa38e8c125d49ab1a1a37d67d37e1cbca8c`。本轮只补充行为测试和当前 main 证据，没有改生产筛选逻辑或 XAML。

## 已有实现与本轮补证

现有 `FilterPresetDefinition` 是仅含字符串标量的持久化 DTO；设置入口会规范化非法旧值、去重重复 ID 并限制数量。任务/媒体工作区已有筛选预设入口、保存/应用/重命名/删除命令，重命名和删除默认沿用 Playnite 确认界面。

复查发现旧测试的工具栏按钮使用测试命令，生产 ViewModel 的任务筛选快照与应用只由源码断言覆盖。本轮新增 `ProductionTaskPresetSnapshotAppliesFiltersAndFallsBackWhenDynamicOptionsExpire`：在隔离 STA 中调用生产 `CreateTaskFilterPreset` 和 `ApplyTaskFilterPreset`，验证搜索、状态、游戏、类型值写入标量快照后可恢复；动态游戏/类型/状态选项消失时回退到“全部”。只使用合成筛选状态与空任务集合，没有启动真实 Worker 或写用户设置。

同一类另外覆盖非法/旧设置回退、重复 ID、只保存稳定标量的 JSON 往返、生产重命名/删除方法收到取消后保留集合顺序、原对象、选择与草稿。取消分支没有进入设置持久化；删除确认仍带危险操作标记。按钮绑定行为已有独立 WPF 测试，但该夹具使用测试命令，生产筛选应用由本轮上述 ViewModel 方法行为测试覆盖。

## 当前 main 验证

- 本机只安装 SDK `9.0.302` / VSTest `17.14.1`；仓库 `global.json` 锁定 SDK `8.0.423`，本机不可用。本轮从仓库外启动 dotnet 并使用隔离输出，全解方案 Release restore/build `0 warning / 0 error`。SDK9 结果不替代 SDK8 基线或 CI SDK `10.0.401`。
- `R10FilterPresetBehaviorTests` `6/6`；其中新增生产快照/应用正例和过期动态选项回退负例通过。相邻 `TaskCenterViewResponsiveTests` `8/8`、`Q14ToolbarAlignmentBehaviorTests` `1/1`，合计 `15/15`，0 个测试失败或跳过。各 WPF 测试类在独立 testhost 中执行。
- Q14 用例完成并报告 `1/1` 通过、进程 exit 0；用例结束后 testhost 输出 WPF COM `InvalidComObjectException` 清理诊断。这不是测试断言失败，但作为该次运行的 teardown 观察保留记录。
- XAML 结构检查 `24/24`，`scripts/validate-source.py` 与 `git diff --check` 通过。无 XAML 或视觉变化，因此没有生成截图。

脱敏 TRX：

- [筛选预设行为](tests/R10FilterPresetBehaviorTests-sdk9302-e2353aa3.trx) — SHA-256 `9C57964374769941F5139538E620F96EB7AEF2AC683706C4933DFB310D463F4E`
- [任务中心响应式回归](tests/TaskCenterViewResponsiveTests-sdk9302-e2353aa3.trx) — SHA-256 `DE4AE51D3385E9803D109638628DA5B94D69BD92DB6B5D3163AC1A131A8D29A9`
- [工具栏布局回归](tests/Q14ToolbarAlignmentBehaviorTests-sdk9302-e2353aa3.trx) — SHA-256 `DDABE254691B49025764C531713488F94D4AB6EC03CF1C4488A719A46307848B`

构建身份：Playnite DLL SHA-256 `DEA244A61DA76453A464DDC15A50E8080CDEC7FC4617AE03B0C0BFC5A927AF1E`、MVID `31047390-8992-4e9f-aa39-82758ff10a0d`、PDB SHA-256 `D30F34A1442EFD7D0B3DCB6D5E215CAF50AEFFCB48B3A1CE1C1BA1919075B2B9`；测试 DLL SHA-256 `DF89029A8D8E39D284C49C463787DB3268A33A0A01045D58A2BF02E7FEFDCB7F`、MVID `33761333-58a7-4b91-82eb-3b3d3d5f97fe`、PDB SHA-256 `C5C2985723122BEAA26AFBB1C57EAE74F333334A3542AB8657B2E37301E92C58`。ProductVersion 为 `0.6.73+e2353aa38e8c125d49ab1a1a37d67d37e1cbca8c`；新增测试源码 SHA-256 `6E7D697F501B88AA7EC8193554BA836B721D80BAB95293C5451F809047BD5EB3`。

相关未改生产源码身份：`DashboardViewModel.FilterPresets.cs` `0C9F9F6C1AE0B17443BF460CD56432D3ABCDEEE7C7EECB5D86CBDBB4B3A57A87`；`FilterPresetDefinition.cs` `41CA85AC24D002A0996EC75E6D00AFF2665FC9965C09D25185A3927E1CC237A8`；`GameSaveCenterSettings.cs` `4C1D01668403FEBF21D2C55CB0F955A251FA34B44943873E3AC67077D721AEB0`；`TaskCenterView.xaml` `00EBAC27AB45A858BBF9107DE60C64C28DA8F95C70CB3FB8425A6ACE5D65C0E2`。

## 边界与后续

测试使用合成设置/过滤值、空集合和隔离 net472/WPF testhost；没有访问或修改真实 Playnite 设置、存档、媒体、用户云端或诊断。未验证真实 Playnite 确认对话框、实际设置存盘、UIA/读屏、IME、物理 DPI/跨屏、最终呈现帧、ETW 或宿主性能。

前置 #991 失败的完整日志仍不可读，根因未知；本轮本机结果不替代 CI。提交推送后的 Actions run ID/终态还需跟踪到成功或失败。下一项 R10-06：复核现有来源提示和清除命令，确认清除只移除导航带入条件并保留其他筛选草稿。
