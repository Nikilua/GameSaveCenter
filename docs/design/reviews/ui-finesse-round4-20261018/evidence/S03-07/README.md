# S03-07 脱敏方案导出受控验证

## 实现

实现提交 `eeedd003bb91a0e562529b45b15d4cde67f18db8`。草稿卡片新增“预览导出”入口，先在只读文本框展示摘要，再由用户明确保存为 UTF-8 TXT。摘要只使用游戏名、草稿/版本时间、文件和映射计数、readiness 状态及重验标记等白名单字段；不输出 Playnite/草稿/备份 ID、HMAC 指纹或密钥 ID。动态文本中的本地盘符/UNC/用户目录路径会整体隐藏，URL 参数由共用 `MaintenanceReportRedactor` 脱敏。摘要明确标注不可导入或执行，不包含存档数据、认证参数或自动执行脚本。

导出时会重新生成摘要并与当前预览比较；若草稿的重验状态或其他摘要字段发生变化，会先刷新预览并要求用户再次点击导出。实现没有文本导入命令、Worker 调用或恢复命令路径；删除草稿时会清除相关预览。现有 `RestoreConfirmationGuard` 和恢复前检查未改变。

UI 沿用已有共享 `GscWpfUiContextButton`、`GscWpfUiActionButton`、`GscWpfUiTextBox` 与 `GscRedesignSubCard` 样式。仓库约定的 Demo 目录 `GameSaveCenter.AcrylicFork/src/GameSaveCenter.Playnite/Design` 不在本次 checkout，因此无法核对 Demo 页面；本项只增补既有 SaveCenter 草稿区域，没有新增局部颜色或控件模板。

## 构建、测试与安装

- 精确提交 `eeedd003` 的 SDK `8.0.423` Release solution build 成功，`0` errors。NuGet 漏洞数据源不可达产生 `17` 条 `NU1900` 警告；隔离安装流程的 Release build 为 `0 warnings / 0 errors`。
- 定向 Playnite 测试 `18/18`：导出字段白名单/路径与认证参数脱敏/新鲜度提示/只读预览入口，恢复草稿失效与比较，及 R09 `2/2`。TRX 保存在 [`tests/S03-07-targeted.trx`](tests/S03-07-targeted.trx)。
- `python scripts/validate-source.py .` 通过；`scripts/check-xaml.ps1` 检查 25 个文件通过；WPF 静态审查扫描 31 个 XAML，`0 errors / 26 warnings / 177 info`。
- RenderHarness 在精确提交、干净 worktree 上完成浅色/深色多尺寸矩阵，`WorkingTreeClean: True`，`render-qa OK`；简要记录见 [`render-summary.txt`](render-summary.txt)。离屏 Harness 没有执行“预览导出”按钮交互，也不代表真实 Playnite 渲染。
- `scripts/dev-install-run.ps1 -NoStart` 精确提交隔离安装成功，版本 `0.6.73` / DLL `0.6.73.0`，未启动 Playnite。安装报告见 [`install-report.txt`](install-report.txt)。插件 DLL SHA-256：`7B54AA71CE86C51E829BBD760BE65402EA07F6CE96C5F2B960D8A43C556F8DC6`；Worker DLL SHA-256：`23CF2BB23EE386EDB6BD75E75AC0367ECEDE53B484008DBC70D606B985873448`。

## 边界

未运行完整 WPF 逐类隔离循环或真实 Playnite 宿主。RenderHarness 验证的是现有 SaveCenter 页面在主题/尺寸矩阵中的离屏呈现，按钮交互由 formatter 与源代码行为测试覆盖；实际宿主的文件对话框与预览操作仍待运行确认。旧 R09 诊断输入材料仍不可用，因此当前 R09 通过不反推用户此前失败的运行身份。工作区用户已有未跟踪 TRX 与 `TestResults` 未纳入证据。
