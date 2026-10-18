# S02-02 收件箱入口

实现提交：`8810d9e6cd0fe0677ee500bcf766b047bcd6e8f2`。本阶段把通知生产路径接入 Worker 本地记录，加入通知收件箱、严重程度与时间筛选、未读计数、刷新和详情查看，并保留原任务状态来源。紧凑布局中列表保持可见，详情由明确按钮展开。

## 受控验证

- SDK `8.0.423` Release 全解方案构建通过：`0 warning / 0 error`。
- Playnite 定向测试 `15/15` 通过，其中 `R09FocusOutlineBehaviorTests` 为 `2/2`；原始 TRX 在 [tests](tests/)。
- Worker `NotificationRecordPersistenceTests` 为 `3/3` 通过；原始 TRX 在 [tests](tests/)。
- Render QA 为 `22/22` 场景通过，覆盖浅色/深色主题和 11 种窗口尺寸；完整报告在 [render](render/render-qa-report.txt)，代表截图为 [浅色](render/Notifications-light-1040x700.png) 与 [深色](render/Notifications-dark-1040x700.png)。
- `python scripts/validate-source.py .` 通过；WPF 静态审查扫描 36 个 XAML，`0 errors / 26 warnings / 177 info`，报告在 [audits](audits/wpf-ui-audit.txt)。
- DLL、PDB、测试源码和 TRX 的 SHA-256 见 [verification-identity.json](verification-identity.json)；本证据不声称 MVID。

## 边界

未完成完整 130 类 WPF 独立进程循环；本阶段只跑了与通知、页面切换、焦点和命令目录相关的定向用例。没有运行真实 Playnite 安装/启动门禁或声称真实宿主渲染通过。

用户最初提供的外机失败 ID `20261018T012631286Z-e027df9d` 暂无 summary/TRX/DLL/PDB 可核对，故旧运行实际使用的源码与程序集仍未知。新鲜构建的当前提交版 R09 为 `2/2` 通过；这不能替代旧运行的身份材料。