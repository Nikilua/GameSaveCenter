# U02-05/06 共享边界、圆角与裁切

## 去重与实现

实施前核对 U00 基线、U01 共享主题、Round4 滚动/选框约束和现有 `DesignTokens`、`Redesign`、WPF-UI、Acrylic 生产字典。边框厚度与分隔线是重复的 1 DIP 值，圆角则散落在共享样式与模板中；没有证据要求更换当前页面架构或改动业务控件。

提交 `417bed510806f965122554beb4419082443b905e` 引入 `GeometryTokens.xaml` 的圆角、hairline 与分隔线令牌，以及 `RoundedClipBehavior`。行为依据 Border 实时大小和 CornerRadius 生成圆角裁切，并与原有消费者 Clip 相交；共享卡片、壳层、表格、按钮、状态/空态面板和游戏选框沿用各自原有命令、Binding 与键盘行为。

生产 XAML 盘点发现 WPF-UI、Acrylic、设置和工作区还有字面量 1 DIP 边框。提交 `f59fe4c86c78cb2fc11414a431aceb49821910ee` 将所有当前 Playnite 项目 XAML 中的单 DIP 边界统一引用 `GscHairlineThickness`，并让相关字典在自身作用域加载几何令牌。没有添加负 Margin；保留游戏选框、滚动条系统和现有页面结构，不改变选择/滚动行为、业务状态或安全语义。

## 本地验证

- Windows 10.0.22000；固定 SDK `.tmp/round5-u02-sdk8.0.423`，SDK 8.0.423 / MSBuild 17.11.48。
- 精确源码 SHA `f59fe4c86c78cb2fc11414a431aceb49821910ee`：聚焦 `WpfUiResourceDictionaryTests`、`ProductionShellChromeSourceTests`、`R09PixelStrokeBehaviorTests` 共 204 项，165 通过、39 个既有 skip、0 失败。39 个 skip 保留原原因，不计通过；TRX SHA-256 `e51966c5e255f9bcb695db919b3a6f9f5f26852ef8142ea52aea057b7bae62ff`。
- `python scripts/validate-source.py` 通过；`scripts/check-xaml.ps1` 对 26 个文件通过；WPF 静态审查扫描 37 个 XAML，0 errors、26 warnings、179 info（warnings/info 为既有审查项）。
- 精确 SHA 的 Release RenderHarness 编译 0 warnings、0 errors；双主题、工作区/壳层及滚动样本完整运行，报告结论 `render-qa OK`、0 `PROBLEM`。抽检概览、壳层、存档页截图未见边缘越界或新增双描边。截图与逐文件 SHA-256 在 [screenshots.json](screenshots.json)；原报告为 [render-qa-report.txt](render-qa/render-qa-report.txt)，SHA-256 `80ec539cdbb4276204f12e6ad58215c3717fd9ec84f2fd485a91143ade18dc2f`。
- `R09PixelStrokeBehaviorTests` 在 100%、125%、150%、175%、200% 缩放检查圆角内容裁切与共享线宽。测试/离屏渲染不等于真实 Playnite、UIA、实体显示器或高对比宿主验收。

## 远端、全链与磁盘边界

`windows-build.yml` 精确 SHA 的两次推送运行仍在检查中，baseline 与 SDK 10.0.401 job 身份见 [ci-runs.json](ci-runs.json)。required-checks/branch-protection 查询此前返回 403；不推断保护状态。

本批尚未运行整轮最终中文根入口、发布/打包/安装身份/启动链或独立 `scripts/build.cmd` 职责验证；真实宿主、UIA/键盘/IME、物理 DPI 待验。真实存档恢复、媒体删除和云写入不在本轮验证范围。

本阶段前后记录的 D 盘可用空间为 7,550,947,328 与 6,532,808,704 字节。当前 U02-05/06 证据目录约 53,959,678 字节（334 PNG 共 44,987,438 字节）；其余净空间变化不归因于该证据目录。当前渲染审计与最终 TRX 均是文档引用证据，未删除。此前 `.tmp` 清理受执行策略拒绝，本轮未换 Shell、未触碰其他任务 TRX/TestResults，也没有清理用户数据。
