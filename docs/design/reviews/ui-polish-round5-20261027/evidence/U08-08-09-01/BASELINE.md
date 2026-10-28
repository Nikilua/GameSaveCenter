# U08-08 / U09-01 跨页状态与证据复核

2026-10-29，源码基线 `d19570fad7f056e44b2f0d5aee2eca3260d04ae7`。八类页面首轮共享资源盘点已在[U08-07/08证据](../U08-07-08/BASELINE.md)记录；本批继续检查生产状态呈现一致性，并修复该基线CI发现的行为测试夹具时序错误。

修改器“当前游戏工具”列表中留有一个永久 `Collapsed` 的旧 `TextBlock`，文字与同区 `WorkspaceStatePresenter` 重复且没有触发条件。本批删除无效节点；共享状态模板、空态说明、导入命令和加载期间的空态抑制保持不变。新增源码回归要求工具列表仅使用共享 Presenter，并保留 `IsTrainerToolsLoading=False` 条件。没有改业务逻辑、资源字典或安全语义。

Light `Trainer-1040x700` 删除前后 SHA-256 均为 `7E5F9EDF44E654448B2C2A6B506456B4EE8C514819F2EE36C63163B22ABB12B3`；被删节点原本始终折叠，因此结果像素一致。前一张图来自 `18481e6d` 旧 Render QA，未固定样本 UTC；它只验证静态 Trainer 夹具，没有作为跨页或时间敏感数据的同条件证据。固定 `2026-10-29T00:00:00Z` 的双主题 Render QA 逐项说明见[screenshots.json](screenshots.json)。

## 验证

- SDK `8.0.423`、Release，`WorkspaceStateSourceTests`、`Round5OverviewSaveContextTests`、`Round5ToolMaintenanceHelpTests` 与 `TrainerInstalledToolActionsSeparateWrappedRowsAndRestoreInCompactDrawer` 共17项：16通过、0失败、1既有skip。skip是已撤销“今日工作台”架构的历史源码断言，不计通过。原始TRX SHA-256 `31C94F65F0631C2128F52927E1910B3B09293AB0CAD0690F68F707A6F27B4175`；脱敏副本见[tests/U08-08-focused.redacted.trx](tests/U08-08-focused.redacted.trx)，SHA-256 `1533D6F3C3516DF4778D6B43DB6DEB062E8E2F59481094040D2279BB613F7810`。
- GitHub提交 `d19570fa` 的run `40737167888`中，baseline job `122988713401`与SDK10 job `122988712933`均失败；各有54个布局行为用例，52过、2失败，失败均为浅/深主题下 `TrainerInstalledToolActionsSeparateWrappedRowsAndRestoreInCompactDrawer`。根因是窗口加载前采集了六个子项的边距，加载后忙碌状态文本折叠，实际仅五个动作可见。测试现在先加载窗口、断言空闲状态文本折叠，再采集可见动作边距，随后保留桌面两行间距及紧凑抽屉恢复断言。SDK8.0.423双主题专项2/2通过，组合测试见上项。失败TRX原始SHA `0D42D0C47633405A807B9F75E3B9C8AE1947ECE7DAF72B7C9FC5FA94D83FB32B`；脱敏诊断见[tests/row-gap-restore-capture-failure.redacted.trx](tests/row-gap-restore-capture-failure.redacted.trx)，SHA `0B1EB12B4E7CDD9DE5D91E3131B5413AD409C51B7415B5A8CB2B80C6A88AAAAB`。
- 首次手动测试未传 `GscBuildCommit` 元数据，身份守卫按预期失败；补齐提交与 source root 参数后再运行。该诊断不是产品断言失败，原始SHA `C48D4614C1A0AFCEE72386734745DBDDC5BD9740DB32FD6936FB9E9A571F7450`；脱敏TRX见[tests/identity-metadata-missing-initial-invocation.redacted.trx](tests/identity-metadata-missing-initial-invocation.redacted.trx)，SHA `103EF086ADC52A7663A8D7E9877B431257FC688C6EB4BDAA040399D51AE739DA`。
- `python scripts/validate-source.py`退出0；`python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .`退出0，42 XAML、0 errors、26 warnings、179 info。警告与info延续全库审查基线。
- `scripts/render-qa.ps1` 的RenderHarness Release编译0警告/0错误，整组退出0；双主题、394 PNG、0 PROBLEM，逻辑DPI 1.00、固定UTC样本。原始报告SHA `D562C430759B4CBD59E5595542FE4C1B36DE8D4907D1F27CCE2DA44DDE8847A7`；脱敏报告[render-qa-report.txt](render-qa-report.txt) SHA `3FFC015EB8BAFE363062DE628A5729FC7DAABA0F651FFA2C0D34106DC6D43385`。
- `d19570fa` CI artifacts：baseline `GameSaveCenter-build-diagnostics` ID `12839593450`，digest `sha256:67f72c73f82b05f1e1b48ed3aedd41c5b7daad6a6ff826fcf1d96591536ab773`；SDK10 `GameSaveCenter-sdk10-build-diagnostics` ID `12839945589`，digest `sha256:63df4d8cbfbf590280ebb57dacdd3ecfa19b6973de268515702e8e258200eb0e`。d6前置提交run `40731004737`两个job已通过，SDK10诊断artifact `12835609657`；baseline打包artifact `12837674680`及预览artifact `12837438404`。required-checks接口先前403，规则保护仍“远端待验”，不据此推断。

## 边界与状态

U08-08为“代码完成待验收”，其它跨页字体、间距和状态差异仍需逐状态复核。U09-01保持进行中：本批报告进入最小证据集，但其它Round5批次同条件双主题前后配对仍有缺口。`d19570fa`的失败已定位；本修正提交的双SDK CI仍待查询。Render QA PNG留在`.tmp/round5-u08-01-02-08/render`作为本批当前审计。离屏结果不代表真实Playnite、外部UIA/Narrator、实体键盘/IME、物理跨屏DPI或实际安装版本。