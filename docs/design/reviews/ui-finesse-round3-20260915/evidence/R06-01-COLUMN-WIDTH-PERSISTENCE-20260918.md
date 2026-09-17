# R06-01 列宽用户记忆证据

日期：2026-09-18  
实现提交：`75a6e6d826e39fe5a7f06438d7aefd1985ad1692`  
范围：Save History、Save Candidates、Task Queue、Media Inbox 四个生产 DataGrid 视图区；Maintenance 表格本批只参与既有滚动/表头回归，不纳入列宽记忆范围。

## 结论

R06-01 在当前可控范围内已满足。列宽状态使用现有 Playnite 设置保存链，按 `v1/{viewKey}/{columnKey}` 隔离视图区；只记录用户拖拽形成的有效 Pixel 宽度，保留 Star/响应式默认布局。恢复时按当前版本和当前列键精确应用，未知或旧版本键被忽略；每个入口提供“重置列宽”。窄窗口仍保留最小主列与既有横向滚动能力。

没有把 main 的旧实现带入当前分支，也没有替换游戏选框、滚动条、命令/绑定、取消/错误、安全恢复或有限列表实现。

## 实现核对

- `GameSaveCenterSettings.DataGridColumnWidths` 使用序列化字典保存用户偏好，并在克隆/导入时限制键数量、键长度、有限正数和最大宽度；控制器只接受当前 `v1`、当前视图和当前列键。
- `DataGridColumnLayoutController` 监听列宽依赖属性变化，忽略初始化/响应式布局过程，仅对用户 Pixel 宽度做 400ms 防抖保存；卸载时 flush，重置时删除当前视图区键并立即持久化。
- Save Center 接入 `save-history`（7 列）和 `save-candidates`（4 列）；Task Center 接入 `tasks`（6 列）；Media Center 收件箱接入 `media-inbox`（5 列）。每个页面保留既有 `CanUserResizeColumns`、最小列宽和自动横向滚动。
- 四个页面的重置按钮均使用现有页面操作区和自动化属性，不改变业务命令或数据源。

## 行为与负例

在隔离 STA WPF、合成 DataGrid 和隔离设置对象中，`R06ColumnWidthPersistenceBehaviorTests` 定向结果为 `5/5`：

1. 用户 Pixel 宽度写入后，控制器重建可恢复；不同视图区互不串值。
2. `v0` 版本键和未知列键不生效；应用宽度服从当前最小宽度。
3. 重置恢复捕获的默认布局，只删除当前视图区键，并验证重置持久化回调。
4. 实际窄 WPF Window 中主列不低于最小宽度，既有横向滚动条仍可见。
5. 生产 XAML 接线、稳定视图区键、设置存储、初始化抑制和仅 Pixel 捕获均有源契约校验。

测试命令：

```text
dotnet vstest .tmp/r06-01-build/bin/GameSaveCenter.Playnite.Tests/Release/net472/GameSaveCenter.Playnite.Tests.dll '--TestCaseFilter:FullyQualifiedName~R06ColumnWidthPersistenceBehaviorTests' '--logger:console;verbosity=minimal'
```

结果：`5 passed, 0 skipped, 0 failed`。没有写真实存档、媒体、云端或用户配置。

## 构建、回归与视觉证据

- `scripts/build.ps1 -Configuration Release -SkipTests -OutputRoot .tmp/r06-01-build`：XAML `24/24`，Release 编译 `0 warnings / 0 errors`，Playnite 输出含 `net462`。
- `python scripts/validate-source.py`、`scripts/check-xaml.ps1`（`24/24`）和 `git diff --check` 通过。
- R05 相关独立进程回归：Popup 生命周期 `1/1`、Tooltip 时序 `1/1`、Popup 边界 `2/2`、焦点范围 `3/3`、开关保存 `1/1`、选项虚拟化 `3/3`、Round2 源契约 `24/24`。
- clean RenderHarness：`.tmp/r06-01-render-clean/render-qa-report.txt`，绑定完整 SHA，`WorkingTreeClean=True`，Light/Dark，`DpiScale=1.00`（离屏逻辑 DIP），357 张 PNG，`render-qa OK`。报告覆盖 50/400/2000/4468 数据量、现有纵横向滚动探针和 2560×1440 → 1100×720 → 2560×1440 resize 恢复序列。
- 抽查图：`Save-1040x700-tab0.png`、`Task-1040x700.png`、`Media-1040x700-tab0.png`；三页均保持当前 Demo-first 壳层层级、表头/行结构和现有滚动呈现，没有出现旧主题残留或主列被裁成不可见。

## 未验边界与下一步

“重启恢复”在本轮以同一隔离设置对象上的控制器销毁/重建行为覆盖，不等价真实 Playnite 进程重启、宿主配置迁移或用户实际拖拽录像。尚未验真实 Playnite 嵌入窗口、物理 DPI/跨屏、Presented frame、OS 输入/IME、屏幕阅读器/UIA、ETW 和宿主性能；没有把离屏逻辑 DIP 或代理性能写成真实呈现结论。下一项为 R06-02 排序提示与稳定性。
