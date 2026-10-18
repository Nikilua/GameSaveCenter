# S03-02 版本关系视图

将备份历史接入来源关系展示和按需分页。历史第一页最多返回 100 条；“加载更早版本”使用创建时间原始值与备份 ID 组成的稳定游标，不用偏移量分页。Worker 只为当前页计算内容指纹，不向界面投影清单路径。版本详情按时间展示来源父版本、分叉、内容相同版本和设备；父版本尚未加载与完整索引确认缺失会分别说明，未知或歧义关系不会根据时间顺序猜测。

视图以选中版本为中心读取有界上下文，保留已有时间范围、选择和恢复命令。新增 SQLite 复合索引支持游标顺序。证据截图来自 RenderHarness 的 1366×768 保存页浅色主题，版本详情区域可见父版本与内容相同关系。

## 受控验证

- SDK `9.0.302` Release 全解方案构建：`0 warning / 0 error`。当前安装只含 SDK `9.0.302`，仓库 `global.json` 锁定 `8.0.423`；本轮没有 SDK8 正式基线。
- Playnite 定向测试 `12/12`：关系视图 4、历史时间导航 6、R09 焦点行为 2；Worker 定向测试 `2/2`：稳定分页/指纹 1、版本元数据持久化 1。完整 TRX 和运行日志见 [`tests`](tests/)。当前匹配构建的 R09 为 `2/2`，但不能识别或解释用户此前缺少同次 `summary.json`、TRX、DLL/PDB 的失败运行。
- RenderHarness 完整矩阵报告 `render-qa OK`，`0 PROBLEM`；报告与代表截图见 [`render`](render/)。source validation 通过，XAML 结构检查 25 个文件通过；WPF 静态审查 `0 errors / 26 warnings / 177 info`。原始日志见 [`build`](build/)。
- 插件 DLL/PDB 与 Playnite 测试加载副本哈希一致；Worker DLL 与 Worker 测试加载副本哈希一致。SDK、源码、二进制和 TRX 的 SHA-256 清单见 [`verification-identity.json`](verification-identity.json)。

## 验收边界

本轮没有安装或启动真实 Playnite，也没有在实际用户数据上执行备份、恢复或迁移。SDK `8.0.423` 正式构建基线仍待可用环境验证。用户此前贴出的 R09 运行缺少同次诊断文件；本轮 R09 通过仅说明当前受控构建的行为，不能证明旧运行加载了哪份源码或程序集。
