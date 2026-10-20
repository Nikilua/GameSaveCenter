# S05-02 双方案预览

实现提交：`5f1431f264ca62f89818554da47af85f308821f0`（`增加保留策略双方案预览`）。

## 等价能力审计

现有 `RetentionSimulationService.PreviewAsync` 只按每个游戏当前已生效的策略生成清理预览，并为后续二次确认保留清理句柄；`StorageAnalysisService` 的历史趋势也没有候选策略侧。因此它们不能满足同一版本快照上的“当前策略 vs 会话候选策略”比较。S05-01 已收集候选规则和增长假设，但尚未把输入交给 Worker 规划。本批新增独立的只读对比请求和结果 DTO，不复用清理预览句柄。

## 实现行为

- `SqliteStateStore.GetRetentionScenarioSnapshotAsync` 用单条 SQLite 查询读取已索引版本及对应游戏策略；两侧使用同一结果和同一 UTC 截止时点。
- 当前侧按每个游戏已生效策略规划；候选侧把用户输入的全量/每日/每周/每月规则统一应用到所有游戏。两侧均调用现有 `RetentionPlanner`，不复制保留与保护算法。
- 结果并列显示共同基线、保留数、保护项去重数、锁定/PreRestore/健康恢复点分类数、候选数和候选归档的索引逻辑大小。每侧均满足 `保留数 + 候选数 = 基线版本数`；分类保护数可能重叠，保护项总数按版本去重。
- 用户输入的每 30 天增长上下限会被 Worker 再次校验并回显为假设；本批不把它们用于保留计数或候选大小。结果说明候选规则对所有游戏统一应用，并明确逻辑大小不是磁盘实占或实际可释放空间。
- 候选对比没有 Apply 路由、预览 ID 或文件操作；Worker 测试确认随机清理请求无法引用对比结果，归档文件和索引均未变化。原有全局清理仍走独立的 `PreviewRetentionSimulation` / `ApplyRetentionSimulation` 二次确认链。
- 维护中心“保留策略”页新增比较按钮和并列结果卡，窄宽度时纵向排列。修改任一情景输入会清除旧对比并要求重新生成；页面滚动容器承载结果卡。

## 验证

- SDK `8.0.423`，精确提交 Release solution build：`0 warning / 0 error`。源码构建日志和诊断摘要见 [build-console.txt](build/build-console.txt)、[summary.json](build/summary.json)；摘要 SHA-256 `8127425A939E1932DE6B58D37C956BFC4DBD7E23EB2DC9D23E9D3BCA39F0C1BE`，诊断 ID `20261020T100403905Z-b8a5aa0e`。
- 针对该一键构建输出运行 `RetentionSimulationServiceTests` `14/14`，含双方案与现有 `RetentionPlanner` 直接对账、同一 UTC 截止时点、保护计数、增长边界、计数恒等式和不创建清理授权：[Worker TRX](tests/s05-02-retention-worker-5f1431f2.trx)，SHA-256 `160424EDA88A1E751EF38071105BE825B30BB612849F9B2B7F315CC32C2A1E89`。生产 `MaintenanceView` 的 Light/Dark 行为 `2/2`，确认比较卡可见、受页面滚动承载、按钮绑定正确及窄窗口纵向排列：[Playnite TRX](tests/s05-02-retention-playnite-5f1431f2.trx)，SHA-256 `7C9BB176755B6ADFC29DB719A15A651958C70961171D29B0A305A2B3008FE608`。两测试程序集 SHA-256 分别为 Worker `34CBE3D7B2231959A275C0082A7E6FF545E694B2EF78470D1846D99D4F33229D`、Playnite `D37482F25746F5695DD322ED0CE175E3B87750AF6D115A981497ADE69AEDEF3A`。
- 精确提交 `5f1431f2` 的隔离 no-start 一键构建/Worker 发布/安装验证通过，manifest `0.6.73`、插件 DLL `0.6.73.0`；隔离安装副本、当前版本化安装暂存及 Release 构建插件 DLL 的 SHA-256 均为 `BBE5BCC1A862B0D5B123C4A52ACDE14FDD8AD5720A6BD730E4D7C8BD87EC0AE3`。报告见 [install-report.txt](build/install-report.txt)，原始 transcript 见 [one-click-install.log](build/one-click-install.log)。该命令没有运行测试；上面的定向测试随后针对同一一键构建程序集执行。
- 精确提交的双主题 RenderHarness `render-qa OK`、`0 PROBLEM`：[原始报告](render/render-qa-report.txt)，SHA-256 `1ADD68F8B795354F58048E750C3D2ACBF5647064B4BB9D447EE82AD8346DF9BF`。报告提交身份为 `5f1431f2…`、逻辑 DPI `1.00`；`WorkingTreeClean: False` 只对应保留的用户 Round3 TRX/TestResults。
- `validate-source.py`、XAML 结构检查 `25/25`、`git diff --check` 通过；WPF 静态审查 `0 errors / 26 warnings / 179 info`，未引入审查错误。

未运行完整 WPF 独立进程循环，也没有启动真实 Playnite 检查页面。Light/Dark 结果来自生产 WPF 控件的受控行为测试；RenderHarness 是离屏渲染，不代表真实桌面、设备或物理 DPI。真实宿主页面呈现仍待验证。
