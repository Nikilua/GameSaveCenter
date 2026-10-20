# S06-06 失效刷新交付与受控验证

## 范围

提交 `fb33cd11` 完成 S06-06：清单引用继续按对象类型和稳定 ID 查询；索引存在时把当前对象摘要及父游戏摘要放入 `WorklistBrowserEntry` 的瞬时 UI 投影。查询失效、缺结果或不可用时清除旧摘要并保留准确状态，其他条目继续浏览。不使用名称匹配，不把摘要或路径写入工作清单设置。

随后提交 `a1a86e41` 根据用户存档历史、路径校验与任务队列截图修正页面滚动视口：页面 `ScrollViewer.ViewportHeight` 有效时，它严格限制表格可分配高度；只有测量值不可用时才回退宿主高度。页面工具区、表格框架和堆叠详情先从预算中扣除，页面空间不足时由外层滚动承载。此项是 S06-06 验证时的后续 UI 修复，不改变清单数据契约。

## 验证

最终受控验证绑定当前提交 `a1a86e41`（包含 `fb33cd11`）：

- SDK `9.0.302`，Release 全解决方案 Rebuild：`0 warning / 0 error`。
- Worker `WorklistReferenceResolutionTests`：`3/3`。
- Playnite 定向用例：`23/23`，覆盖清单引用刷新、页面视口决策、存档历史/候选路径/任务队列表格与详情几何，以及工作清单入口。
- `validate-source.py`、XAML `25` 文件结构校验、Round4 账本 `5/5` 通过。
- WPF 静态审查：`0 errors / 27 warnings / 179 info`；全局既有提示没有被当作本次滚动或清单行为的实屏证据。

## 原始证据与 SHA-256

| 文件 | SHA-256 |
| --- | --- |
| [Release rebuild 日志](build/S06-06-release-rebuild-a1a86e41.txt) | `C00FF6F96F91048AA9473A2E867AC6FAB9956B02DCE14B882B01F673AC49CBB4` |
| [Playnite 定向 TRX](tests/S06-06-playnite-scroll-and-entry-behavior-a1a86e41.trx) | `DF2C1F8A5430B0965029813B315DAAF36A425A480AEA44B9CEB3BC6988A29660` |
| [Worker 定向 TRX](tests/S06-06-worker-resolution-a1a86e41.trx) | `A45032ED12065DE45C1D958EFD02BBEBECEC6502E476F98449C9EBD54DE467B0` |

实现提交为 `fb33cd11`；`a1a86e41` 为额外滚动边界修复和本证据记录中的最终验证提交。历史审计见 [AUDIT.md](AUDIT.md)。

## 边界

本机安装 SDK `9.0.302`，未安装仓库锁定的 `8.0.423`。未启动或安装真实 Playnite；几何测试是 STA/离屏 WPF，不代表用户物理屏幕、实际 Playnite 窗口或物理 DPI。索引中的记录存在不证明底层文件 ACL、物理存在、可读或可恢复；本任务不探测或绕过这些权限边界。此前未跟踪的 Round3 TRX 与 `tests/GameSaveCenter.Playnite.Tests/TestResults/` 原样保留。
