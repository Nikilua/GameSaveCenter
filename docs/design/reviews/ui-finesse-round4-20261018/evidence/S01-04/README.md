# S01-04 保存与复制视图

实现提交：`c6eb8d62566bbc74f577eaa6e7c5b9132365af1c`（`实现表格视图保存与切换`）。

任务页、存档历史和候选路径表格现在可以保存当前排列、另存副本、重命名和删除命名视图。命名视图记录筛选预设 ID、稳定排序列与方向、可见列键、列宽和密度；每个工作区单独保存活动视图。名称按工作区不区分大小写，最多 48 个字符，每页最多 32 个视图。重命名和删除会先确认；删除活动视图后回到默认视图。修改默认排列时，“保存”会创建命名视图，避免覆盖全局默认列宽、显隐和任务筛选状态。

## 验证

提交后使用 SDK `8.0.423` 完成 Release 解决方案构建：`0` warnings、`0` errors。构建诊断 ID 为 `20261018T072356967Z-15a7f819`，记录的源码提交为上述完整 SHA；[构建摘要](build/summary.json)随证据归档。源码校验和 `git diff --check` 通过。

下列测试类分别在独立进程运行，总计 `29/29` 通过：

| 测试类 | 结果 | TRX |
| --- | ---: | --- |
| ViewConfigurationDefinitionTests | 9/9 | [TRX](tests/ViewConfigurationDefinitionTests.trx) |
| DataGridViewConfigurationBehaviorTests | 1/1 | [TRX](tests/DataGridViewConfigurationBehaviorTests.trx) |
| DataGridColumnVisibilityBehaviorTests | 4/4 | [TRX](tests/DataGridColumnVisibilityBehaviorTests.trx) |
| R06ColumnWidthPersistenceBehaviorTests | 6/6 | [TRX](tests/R06ColumnWidthPersistenceBehaviorTests.trx) |
| R09FocusOutlineBehaviorTests | 2/2 | [TRX](tests/R09FocusOutlineBehaviorTests.trx) |
| R10FilterPresetBehaviorTests | 6/6 | [TRX](tests/R10FilterPresetBehaviorTests.trx) |
| Q14ToolbarAlignmentBehaviorTests | 1/1 | [TRX](tests/Q14ToolbarAlignmentBehaviorTests.trx) |

当前 Playnite 插件 DLL SHA-256：`7EDB762CEB808C685E08B1524884AABE616F7EE0B2619AC1AE3A2FDC4E2F5D8A`；PDB：`FA8D2E71C1394A840B1CEB9645C87477488C75E3140D0134F0A2DC0C2B556318`。测试 DLL SHA-256：`9AC3A179D50E69AC7887DA060F2A8E5472800B364336EC2106A29832E03CCFBE`；PDB：`E506CEF2B4DACB85109DBC671102B0915498B159C45D91D71B4C3E1F898D3654`。

Render QA 在离屏矩阵中返回 `render-qa OK`；Task 页面 `1040×700` 显示 `4/4` 条可读行，见[截图](render-qa/Task-1040x700.png)。这是合成窗口检查，不代表 Playnite 宿主呈现。

## 尚未验证

本项未运行完整 WPF/安装门禁，也未启动真实 Playnite。此前完整隔离门禁在 R05 读取系统指针时遇到 Win32 error 5；不绕过权限检查。用户提供的 R09 外机失败没有对应诊断目录、TRX 或 DLL/PDB 身份；当前提交的本机隔离 R09 为 `2/2`，不能据此解释外机输入。真实宿主设置保存、Light/Dark 宿主呈现仍待验证。
