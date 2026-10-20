# S05-05 策略差异定位

实现提交：`2fd0aa04474c314a179bac0a1c641def813c6d71`（`补全保留策略差异明细`）。

## 等价能力审计

S05-02 的情景比较只返回两侧总计；既有全局清理预览虽有候选版本列表，但只对应当前策略、最多显示 200 条，也不呈现候选策略相对当前策略改变了哪些版本。存档时间线已有单版本保护说明，但没有接入策略对比。现有能力因此不能从总量差异下钻到逐游戏、逐版本结果。

## 实现行为

- Worker 在同一 SQLite 版本快照和共同截止时点下复用 `RetentionPlanner`，为每个游戏返回当前/候选的版本数、保留数、淘汰候选数、保护数和变化版本数。逐游戏计数与两侧总数逐项对账，保护数是保留数的子集。
- 维护中心保留策略页增加逐游戏表；选择游戏后显示两侧结论发生变化的版本，以及该游戏的锁定、PreRestore、健康恢复点保护项。保护原因可并列展示；保护项不进入淘汰候选。
- 对进入变化明细的淘汰候选，仅在既有归档根目录、ZIP 扩展名和重解析点安全边界通过后只读检查归档状态。缺失、不可访问或未过路径检查的归档明确标注，不计入现存归档候选。现存状态仅是本次检查结果，执行路径仍会重新检查；索引逻辑大小不承诺可释放量。
- 比较不创建清理预览句柄，不写策略/设置，不调用清理或 Apply。版本明细只列策略结论变化项和保护项。
- 两张表复用维护页共享 DataGrid 样式、Recycling 虚拟化和有限内部视口；外层页面滚动仍可访问差异卡，长版本明细由表格内部滚动。

## 验证

- 精确提交 `2fd0aa04474c314a179bac0a1c641def813c6d71`，SDK `8.0.423` Release solution build `0 warning / 0 error`，XAML 结构检查 `25/25`。诊断 `20261020T163728118Z-36cc11d3`；原始构建日志 [build-console.txt](build/build-console.txt)，SHA-256 `6E3B1DEC7E6859C5ED5EA8618C3D8A42A1C180EDF54A707DC63DF998A025D324`；构建摘要 [summary.json](build/summary.json)，SHA-256 `915883EFFBEBE15C6A8AA06EB5C0A56C9AA06DDBEC3D54A3A06F0620EAACBCF8`。
- Worker `ScenarioComparison` 定向测试 `2/2`：[TRX](tests/retention-scenario-worker.trx)，SHA-256 `7309584E58A79CBE5BC1D079FAE2A189B5E912DB2A0D4804479CF9FDE3A3DC83`。覆盖逐游戏与总量对账、每游戏保留/候选对账、缺失归档排除及保护原因不混入淘汰候选。测试程序集 SHA-256 `B94EC1EC614F7C1852848DED16E71E468EB456159BED5E5B73992E1BDF107F89`。
- 生产维护页 `RetentionScenarioInputBehaviorTests` Light/Dark `2/2`：[TRX](tests/retention-scenario-ui.trx)，SHA-256 `8D0415D5D9F67C1C37BACD43E24D362C6BD2B5FBE09E4FABE4C768E7BAC5FF45`。验证游戏选择、版本明细绑定、21 行超视口明细的内层虚拟滚动，以及外层页面滚动能把明细表带入宿主视口。测试程序集 SHA-256 `0E6B04E5867558E782A895D984508C9441442FD00C2E3C4ADB34DE3EAD7EB38A`。
- 提交 `2fd0aa04` 的 OffscreenRenderHarness 报告为 `render-qa OK` 且无 `PROBLEM`：[render-qa-report.txt](render-qa/render-qa-report.txt)，SHA-256 `F624F2BF352AC6EB7C8D39BA17BF42958227CF60B5E7AD9EBB64A423972CE39E`。这是合成 WPF 离屏检查，**不代表真实 Playnite 窗口、物理 DPI 或宿主滚动渲染**。
- `scripts/validate-source.py` 和 `git diff --check` 通过；源码日志 [source-validation.txt](validation/source-validation.txt)，SHA-256 `30760E7082311AC27AA2D981C07638FC2A4B11D039C265AFB40365B1F98FBCA6`。WPF 静态审查 `0 errors / 27 warnings / 179 info`：[wpf-static-audit.txt](validation/wpf-static-audit.txt)，SHA-256 `A78EBA371FA086F4CDF6AB1819C07D055CEFD47450636D772140E4C84A74CB1B`；新表明确认 `FiniteViewport`、最大/最小高度和 Recycling 虚拟化。

构建摘要中的 `workingTreeClean=false` 仅对应启动时保留的 Round3 未跟踪 TRX 和 `tests/GameSaveCenter.Playnite.Tests/TestResults/`。这些文件未修改、未暂存、未提交，也没有被本轮测试覆盖。没有触发 CI。没有运行真实 Playnite、CEF 或设备/物理 DPI 验收；不把离屏结果当作真实屏幕证据。
