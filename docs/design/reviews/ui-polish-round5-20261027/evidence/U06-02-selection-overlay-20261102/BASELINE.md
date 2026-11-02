# U06-02 / U08-08 选中框与共享列表内容稳定

日期：2026-11-02。实现提交 `cb7b78fd514e7d6eaa2a34e341340d5d06610508`（`修正列表选中框内容偏移`），已推送 `origin/main`。

## 发现与改动

用户复核首页“最近任务”时指出：选中行有蓝色圆角框，但行内容整体右下偏移，固定状态图标因此贴近并可能被裁。`OverviewRecentTaskItemStyle` 原先将内容放在选中边框内，选中态又将整个 Border 设为 `Margin=4,2`；边框、内距与 Margin 共同挤压内容区。修复后选框仍保留原来的 4×2 DIP 内缩、蓝色语义色、填充与 14 DIP 圆角，但作为独立覆盖层绘制，内容不再由该 Margin/线宽重新测量。

共享隐式 `ListBoxItem` 模板也曾在包住内容的 Border 上改变选中/焦点线宽。现将 `ItemChrome` 与 `ContentPresenter` 分层，原 `Padding` 通过 ContentPresenter.Margin 保留。共享列表的选中框与 2 DIP 键盘焦点框仍可见，模板不再因线宽变化移动内容。

选中模板盘点：共享 `DataGridRow` 已用独立的 `RowBackground` / `RowChrome` 与 `RowContent`；媒体卡片已有 `MediaSelection` 覆盖框；概览保护项只切换填充/描边颜色；ComboBox 选择只切换颜色/字重。这些模板没有发现相同的内容几何偏移，本批未改。游戏选框、滚动条、命令与业务状态均未改。

## 双主题同条件截图

旧图来自源提交 `25bcbf7934575e6aa933373e1f508a1234b28b97`，新图来自本批精确提交 `cb7b78fd514e7d6eaa2a34e341340d5d06610508`。均为生产 Overview、1040×700 DIP、固定样本时间 `2026-11-02T00:00:00Z`；首行任务处于选中态。前图中图标相对蓝色框向右下偏，新图中图标回到原行内位置，蓝色圆角框仍存在。

| 主题 | 修改前 | 修改后 |
| --- | --- | --- |
| Light | [Overview 1040×700 前图](../U05-01-U06-07-U09-04-active-layout-20261102/renderqa/samples/Overview-1040x700-light.png) | [Overview 1040×700 后图](renderqa/samples/Overview-light-1040x700-after-cb7b78fd.png) |
| Dark | [Overview 1040×700 前图](../U05-01-U06-07-U09-04-active-layout-20261102/renderqa/samples/Overview-1040x700-dark.png) | [Overview 1040×700 后图](renderqa/samples/Overview-dark-1040x700-after-cb7b78fd.png) |

WPF 行为回归使用生产 `OverviewView` 和真实 `ListBoxItem` 容器，测量 34×34 DIP 状态图标相对容器的坐标与尺寸；依次比较未选中、选中、键盘焦点，要求几何差异不超过 0.01 DIP，并检查蓝色边框仍在。两种模板分别验证：概览专用模板继续保留 `Margin=4,2` 的圆角选框；公共隐式模板继续保留圆角选择框与焦点框。选中前后边界测试通过。

## 验证

- SDK 8.0.423、Release 解决方案编译：0 warnings / 0 errors；最终候选构建诊断为 `build-diagnostics/20261102T051039213Z-2f274610/`。编译后再执行目标行为测试，没有跳过测试来取得编译绿灯。
- `R10RecentAccessBehaviorTests`：3/3 通过，0 失败、0 skip；完整 TRX 为 `r10-class-tests/R10RecentAccessBehaviorTests.trx`，SHA-256 `20771c5e6a494b53689a6e4c6d097a9e91a5613f5559cbc930d9830793670135`。最终新增选中几何测试首次在仅完成页面专用修复时 1/1 通过；迁移共享模板后又在完整类中通过。
- `python scripts/validate-source.py`：退出码 0，日志见 `source-validation.log`。
- `python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .`：退出码 0，218 XAML / 0 errors / 208 warnings / 1327 info。Overview 的两条既有“StackPanel 内含大型可滚动控件”提示位于页面其他布局行（513、821），本批模板改动未触及；共享模板无 error。
- 精确提交 Render QA：`renderqa/render-qa-report.txt`，Light/Dark，394 PNG / 0 `PROBLEM`，固定时间，`WorkingTreeClean=True`；报告 SHA-256 `f67f06704762bb980c36f3b566247155045ec55f3567ab67e39edb480896b3ad`。全量图哈希在 `renderqa/png-manifest.json`，只保留两张代表性后图以控制证据体积。
- `git diff --check` 退出码 0。

GitHub `windows-build.yml` 对精确提交的 run `41392841712`（#1425）首次查询仍在执行：基线 job `125107766820` 与 SDK 10.0.401 复核 job `125107767536` 均在编译/测试阶段；当时尚无 artifact。不得按本地结果提前认定远端通过；本段待 run 完成后补齐结论、artifact ID/digest 和原始 run 链接。

## 边界与清理

本批验证为离屏生产 XAML 和受控 STA 窗口。未声称 Playnite 真实宿主、实体鼠标、系统 UIA、IME 或物理 DPI 已验；这些会在最终候选按适用门禁继续处理。没有运行真实存档恢复、媒体删除或云端写入。原 Overview 命令、Binding、自动化名称与键盘路径未改，现有键盘访问回归仍通过。

Render QA 全量输出、隔离构建与测试 TEMP 均归本阶段，路径和链接边界核对后安全清理：1112 个文件、216,170,503 bytes；C 盘空闲从 4,673,675,264 增至 4,892,680,192 bytes。保留当前构建诊断、测试 TRX/日志、完整 Render QA 报告、394 项 SHA manifest、前图引用与两张代表性后图。详情见 `cleanup.json`。
