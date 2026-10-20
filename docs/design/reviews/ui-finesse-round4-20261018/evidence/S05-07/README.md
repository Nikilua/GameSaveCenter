# S05-07 转交策略编辑

实现提交：`6807ae842f5a5b1ff478cb231a1fb8fdfc805a2b`（`增加保留情景策略转交入口`）。

## 等价能力审计

`SaveCenterView` 的“备份策略”页已有模板草稿编辑、字段差异、批量目标选择和确认；Worker 应用模板时按稳定游戏 ID 重新读取当前游戏/模板并取得游戏操作锁，应用动作只更新策略。清理使用独立的 Worker 保留预览 ID，应用时重新检查预览时效、版本/策略、保护状态和归档身份。维护中心此前缺少的只有从已保存保留情景跳转并带入模板草稿的入口。

## 实现行为

- 维护中心“保留策略”卡新增“转交策略编辑”按钮。只有选中已保存情景后命令可用；若情景输入、现有模板或当前游戏策略有未保存修改，会拒绝覆盖并说明处理方式。
- 转交复用现有 `CreatePolicyTemplate` 生成未保存副本，将全量/每日/每周/每月四项保留期限映射到模板策略。其他模板字段沿用现有策略克隆行为；模板克隆器继续强制关闭模板不支持的自动恢复。增长范围保留为比较假设，不进入策略模板。
- 自动切换至存档中心“备份策略”页。转交会清除旧比较明细并提示重新比较当前数据；仅生成本地草稿，不保存或应用策略，也不删除归档。
- 后续应用继续由既有差异/确认路径完成；若用户执行保留清理，必须单独生成当前 Worker 预览句柄并由 Worker 复核实时数据和保护边界。转交与旧比较均不构成删除授权。

## 验证

- 精确源码提交 Release solution build 成功，`0 warning / 0 error`；[build-console.txt](build/build-console.txt) SHA-256 `E42425B7D3362950EB924148F3C0BC387AB97AA23341444376B03AB39B3B19A2`，[summary.json](build/summary.json) SHA-256 `1FCDFABA913758251D3AF31EFA64D36F221816849E86D524ADE4A94F855B7A42`。本机 `global.json` 锁定 SDK `8.0.423`，但该 SDK 当前未安装；本次从仓库外启动已安装 SDK `9.0.302` 并指定隔离输出目录构建，没有修改 `global.json`。因此记录为 SDK9 受控构建，不声称 SDK8 精确构建。
- 定向 Playnite Release 测试 `4/4`：策略映射仅覆盖四项保留期限并保持独立副本、拒绝无效情景；生产 `MaintenanceView` 在 Light/Dark 测试主题中检查新按钮可见、命令绑定和安全提示。[TRX](tests/retention-handoff-6807ae84.trx) SHA-256 `5B3FAF3AB746CB7F2E7FE6F1BB885A60BA1BC2135245E30C74BA39813ED06B56`；[tests-console.txt](tests/tests-console.txt) SHA-256 `C60A4DCD4DD9842E73CDC2009F8A5F2B39F2DD2FF1E041E620357E3E087589BC`。
- XAML structural validation `25/25`；[XAML 日志](validation/xaml-validation.txt) SHA-256 `8AD3036B34FC650288D0FF7C995640C8FE5832E5FB5A5AB2CF43C7CF4F44C5BD`；[source validation](validation/source-validation.txt) 通过，SHA-256 `77F4BEDED9CF425FB763B9619B28231640A35746BFFDA882195FCE8787CB8A9B`；[`git diff --check`](validation/diff-check.txt) 通过，SHA-256 `42FB55EBA9B39F8403FB5C27EA2A446A069F486E267BADAB5B96AE698129EB78`。插件/测试程序集与 PDB 身份哈希见 [summary.json](build/summary.json)。

行为测试加载生产 WPF 页面但使用合成上下文和样例数据；没有运行真实 Playnite/宿主屏幕，也没有提交真实屏幕证据。完整 WPF 隔离/安装门禁、物理 DPI 和 CEF/设备权限均未验；没有触发 CI。用户原 Round3 未跟踪 TRX 与 TestResults 保留且未加入本批。
