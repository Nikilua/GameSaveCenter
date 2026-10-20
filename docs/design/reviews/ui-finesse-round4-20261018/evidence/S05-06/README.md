# S05-06 保存情景草稿

实现提交：`313fabc18b9a7f3b627da748cc4dd6c3bd6f0cda`（`保存命名保留情景草稿`）。

## 等价能力审计

此前的 `RetentionScenarioDraft` 只在当前 Dashboard 会话里保存输入值；`CompareRetentionScenario` 返回本次比较时间，但没有持久化命名方案或时间。每游戏策略与策略模板可以保存，却没有保留情景自己的复制、重命名、删除和恢复入口。因此这些能力不是已有功能的重复实现。

## 实现行为

- 新增版本化、标量字段的 `RetentionScenarioDefinition`，最多保存 32 个方案、名称最多 48 个字符。保留期限与增长假设都有范围校验，重复 ID/名称会被规范化流程排除；旧设置缺少该集合时使用空集合。
- 维护中心保留策略页可以选择已保存方案并载入编辑器，也可以保存/更新、复制、重命名或删除。重命名和删除沿用确认流程；删除只移除假设草稿，不更改生效策略、存档或备份。
- 设置只存参数、名称、稳定 ID 和创建/修改/最近快照时间，不缓存比较结果、版本列表或清理预览句柄。比较完成后，只有所选方案和当前输入仍一致时才记录结果时间；后续比较会重新读取当前 Worker 版本快照。
- 编辑器仍使用现有 `RetentionPolicy` 生成独立候选值；草稿与生效策略分离，不调用策略 Apply 或清理命令。可移植设置导入/导出会包含这些非秘密标量设置。

## 验证

- 精确提交 `313fabc18b9a7f3b627da748cc4dd6c3bd6f0cda`，SDK `8.0.423` Release solution build `0 warning / 0 error`，XAML structural validation `25/25`。诊断 `20261020T174421120Z-8e76cb99`；[构建日志](build/build-console.txt) SHA-256 `AA778AAD1EDA6EEDFF0759A37C4BB0EE28981FFA2CC28CF0E76151B42DB77949`，[summary.json](build/summary.json) SHA-256 `1D6F9055F2D3A421D9EE2D0CC14C246A5270047E1BE76CE7D97CBBA4D0E4508D`。插件 DLL SHA-256 `4D678A9935E2C924E1AA8E96DEDE65B0CCF4B6E2851832A8E1B5064EB0B1E888`，PDB SHA-256 `9262BBDC2C0AC1AB90F3EED93BA9A9B990C3D716F8B0C58C7483A3417FAAF792`。
- 定向 Playnite 测试 `13/13`：[TRX](tests/retention-scenario-drafts.trx) SHA-256 `53F0B46CE49D12C53D7493F536551B092609DE9AB42C3E0F27591909844A7124`。覆盖设置往返/编辑回滚、旧设置兼容、未来 schema 拒绝、方案边界/去重/复制/重命名、候选策略独立性、Light/Dark 生产维护页输入验证与绑定，以及便携设置往返/旧包默认值。测试 DLL SHA-256 `C954FB77FDF9262A8521F667B8BD6A47A478EDCFBD97BF6974A3DC9DC05392BF`，PDB SHA-256 `BF9D42AFA4485783BC30D774F94FA4840914CF62189FD234DBD36F4F3A8BD09F`。
- 精确提交的 OffscreenRenderHarness 报告为 `render-qa OK`、`0 PROBLEM`；浅/深主题的多尺寸报告确认保存方案选择器有 1 个合成方案：[报告](render-qa/render-qa-report.txt) SHA-256 `8BB81CD7925C77294A23A94AFDD31FE348849D9DB726FF791F8948EA28987EC5`。它使用合成 WPF 数据、offscreen logical DPI 1.00，不能证明真实 Playnite 窗口、物理 DPI、CEF 或宿主屏幕呈现。
- `scripts/validate-source.py .` 通过：[日志](validation/source-validation.txt) SHA-256 `77F4BEDED9CF425FB763B9619B28231640A35746BFFDA882195FCE8787CB8A9B`。WPF 静态审查 `0 errors / 27 warnings / 179 info`：[报告](validation/wpf-static-audit.txt) SHA-256 `FBF7F48AABC28A1C05A1CEBE6C424DDCFDF4AC59CD63159CFCA01E57E1F0E421`。`git diff --check` 通过，空输出 SHA-256 `E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855`。本证据目录中的文本按仓库规则统一为 LF，哈希对应归档文件字节。

构建摘要记录 `workingTreeClean=false`，因为启动时保留了 Round3 用户未跟踪 TRX 和 `tests/GameSaveCenter.Playnite.Tests/TestResults/`（共 16 个路径）；它们未改动、未暂存、未提交，也未被本轮测试覆盖。没有运行真实 Playnite、完整 WPF 隔离循环、CEF/设备权限验收，也没有触发 CI。受控离屏结果不作为真实屏幕证据。
