# R05 短窗口游戏选择器视口修复（2026-09-30）

## 复现与根因

完整 Release 测试首次运行到 `R05PopupBoundaryBehaviorTests` 时稳定失败。360 DIP 高的测试窗口中，选择器 overlay/panel 只有 `142 DIP` 高；搜索和三个筛选控件占用大部分空间，虚拟化列表的 ScrollViewer 仅 `19.33 DIP` 高，而最后一条游戏行是 `68.67 DIP`，所以行已实现、偏移也到末尾，却仍无法完整显示。

检查生产 XAML 时还发现 `MainPageHost` 指向了右侧布局中不存在的 `Grid.Row=2`，而正文定义在 `*` 星号行 `1`。现改为 row `1`，让页面宿主填满正文行。

当游戏选择器 overlay 少于 `200 DIP` 时，保留搜索与游戏列表，临时收起可选的三项筛选行并把面板内边距从 `14` 收紧到 `8 DIP`，给虚拟化列表留出至少一条完整游戏行的空间。overlay 恢复到 `200 DIP` 及以上后筛选行和原 `14 DIP` 内边距恢复。列表仍启用虚拟化；游戏选择、搜索、绑定和命令没有替换。

## 验证

- [R05 定向 WPF 行为 TRX](R05PopupBoundaryBehaviorTests.trx)：`2/2 passed`、0 failed/skip。360 DIP 窗口选择最后一个合成游戏后，检查面板没有超出 overlay、筛选行已折叠、面板内边距为 8、完整选中行可见、滚动条仍为 Auto；将窗口扩至 900 DIP 后确认筛选行和原内边距返回，再缩回 360 DIP 验证往返行为。
- 同类工作区边缘 ComboBox Popup 测试通过，仍按系统工作区界限、MaxDropDownHeight、虚拟滚动和选中项可见性检查。
- 当前源码 SHA-256：

| 文件 | SHA-256 |
|---|---|
| `src/GameSaveCenter.Playnite/Views/AcrylicProductionShellView.xaml` | `6E264F527B69CC2021BFC5F58D879E46557303BAE4C740C741834FE14D01E410` |
| `src/GameSaveCenter.Playnite/Views/AcrylicProductionShellView.xaml.cs` | `C585775FB6CA2CECA7814342E4FFF7F5C1EAD2822DBF5150B66775EBE6154566` |
| `tests/GameSaveCenter.Playnite.Tests/R05PopupBoundaryBehaviorTests.cs` | `0D48F357B4C58A20A6BEDDB631B45F1ACC5509485C48C71FDC583219B870A5D9` |

测试使用合成列表和隔离 STA WPF 窗口。没有启动 Playnite，也未验证物理 DPI 或用户显示器最终帧。全量 Release 脚本首次运行在该用例处停止；修复后的完整 Release 结果待本轮最终重跑记录。
