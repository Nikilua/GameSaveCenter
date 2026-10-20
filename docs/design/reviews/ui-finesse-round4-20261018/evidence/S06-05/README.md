# S06-05 逐项处理转交受控验收

实现提交：`0aa8054783fd11ee85c895c3e09157f419530789`（`增加待办手动完成操作`，已推送 `origin/main`）

审计：[S06-05 等价能力审计](AUDIT.md)，基线为 `8089876dc533b91eb97d0c5e3645efcd5d7ccfa1`。

## 交付内容

- 人工待办浏览器增加唯一选中条目的“标记为已完成”/“恢复为未完成”动作；完成状态沿用清单设置持久化，可直接撤销。
- 状态仅表示人工跟进，不表示备份或校验成功；续接对象、原业务命令的成功、失败或取消都不自动切换状态。
- 按对象类型和稳定 ID 精确变更，不改备注、顺序或其他条目。保存后重建浏览器行时，保留已有引用解析状态和关联 Playnite ID，其他条目可继续处理，无需重新刷新。
- 表格人工状态继续显示“未完成”/“已完成（手动）”；按钮提供清楚的工具提示和 UI Automation 名称/帮助文本，并复用主题共享按钮样式。

## 受控验证

- SDK `9.0.302`，Release 全解决方案重编译成功：0 warning、0 error。提交前最后一次完整 solution 增量构建同为 0/0，包含最终 `IsBusy` 防护。
- Playnite `WorklistDefinitionTests`、`WorklistBrowserEntryTests`、`WorklistContinuationResolverTests`：17/17 通过；覆盖单一对象种类+稳定 ID 修改、备注/次序/其他项不变、反向撤销、无效目标拒绝，以及续接不改人工完成状态。
- `scripts/validate-source.py` 通过；XAML 结构检查 25/25；Round4 台账测试 5/5；`git diff --check` 通过。
- WPF 静态审查 0 errors、27 warnings、179 info；警告/信息为仓库全局既有检查项，本次 TaskCenter 按钮未新增审查错误。真实窗口、主题/DPI 和 Playnite 呈现未作声称。

## 构建与测试产物

- [Playnite TRX](tests/s06-05-playnite.trx)，SHA-256 `D1A84A57FDBDFBD5BA1E3FB2EA8C94A3AC1E9AA7F7735E0BD7C3CA497C3082C3`
- Release `GameSaveCenter.Playnite.dll` SHA-256 `D868FB96E2CE6A6BF8757AB2DB51F869CEE4389DD7EEEDF87FA7F59497BB8E98`
- Release `GameSaveCenter.Playnite.pdb` SHA-256 `600F6E933F7AE1322603288E761EC50D7D9DAE95C54A2ACF3D5DDD72B0CD0862`
- Release `GameSaveCenter.Playnite.Tests.dll` SHA-256 `670B4E56C121C39C0979D50A7BE6B3E8AB96E1D8122012DD688532F23A7D75C6`

## 未验边界

本机仅安装 SDK `9.0.302`，仓库锁定的 `8.0.423` 未安装。未运行真实 Playnite 安装、宿主窗口、Keyboard-only、UI Automation、主题/DPI 与高对比度视觉检查；一键开发入口会安装并启动宿主，本批保持在受控构建/测试范围。未触碰 CEF/ETW 权限边界，未触发 CI。保留原 Round3 未跟踪 TRX 和 Playnite `TestResults`；本批测试 TRX 单独复制到此证据目录。
