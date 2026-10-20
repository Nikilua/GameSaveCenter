# S06-04 续接上下文受控验收

实现提交：`2af5841f39a3871db3fd4e4da07b81b6a25ef3c5`（`增加待办对象续接入口`，已推送 `origin/main`）

## 交付内容

- 工作清单浏览器新增“续接对象”动作；只有单选且 Worker 已确认索引存在/记录已解决时可用。
- 存档版本按长度前缀拆分出的 Playnite ID 与备份 ID 打开；媒体按精确 Media ID 搜索/选中；后台任务以精确 Task ID 打开详情；校验问题通过新增只读 finding ID 查询定位，包括已解决记录。
- 不按名称或路径猜目标，不选相邻对象；定位只筛选/选择详情，不执行备份、恢复、校验建议或任务重试。对象过期时留在清单、显示诊断且可移除。
- 清单引用继续使用已有设置持久化。任务续接返回时在当前会话恢复原清单、条目、搜索和分组状态；不持久化对话框、命令或旧操作授权。

## 受控验证

- SDK `9.0.302`，Release 全解决方案重编译成功：0 warning、0 error。构建从系统临时目录启动，以使用当前已安装 SDK；仓库 `global.json` 锁定的 SDK `8.0.423` 本机未安装。
- Worker `WorklistReferenceResolutionTests`：2/2，通过 SQLite 验证精确媒体/问题父游戏 ID、已解决问题精确读取和失效 ID。
- Playnite `WorklistContinuationResolverTests` + `R10NavigationBehaviorTests`：10/10，覆盖长度前缀 ID、媒体父游戏缺失、任务/已解决问题精确 ID、失效状态禁用，以及返回时清单选择上下文。
- `scripts/validate-source.py` 通过；XAML 结构检查 `25/25`；`git diff --check` 通过。

测试结果：

- [Worker TRX](tests/s06-04-worker.trx)，SHA-256 `E1EAA9EB64346514B39B0F19955D1ACAF93021500081584FD20CBC7D6A3597AC`
- [Playnite TRX](tests/s06-04-playnite.trx)，SHA-256 `46C12510A0E2F6812D8E18B72922E757592567A3AC4BF0A6D0D2F543A361378C`
- Release Playnite DLL SHA-256：`D97BFB30319EBDDC34346583698E61788083F01D01BCED8E00060A8F699FDB4C`
- Release Worker DLL SHA-256：`228C6D7C99C1043EDB740369B7777BFEA4C7EEF10D73BB549589C61D4B3BCF6D`

## 未验边界

本机没有运行真实 Playnite 宿主或实际窗口交互；上述测试与 XAML 检查不作为屏幕呈现证据。SDK8.0.423 未安装。没有探测或绕过 CEF/ETW 权限，也没有触发 CI。
