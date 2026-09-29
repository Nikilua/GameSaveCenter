# 设置页分组图标状态色收敛

日期：2026-09-30。实现提交：`c770d37c478fe447241b65fc35934a093345022f`（`统一设置页分组图标颜色`）。这是用户“减少装饰蓝/杂色”报告中的一个生产实例，不改变 192 项任务状态或计数。

## 修改与行为

- “存档格式与历史版本”此前使用 Info 蓝，“自动化与安全”此前使用 Success 绿；二者是静态分组标题，不代表信息提示或成功结果。现在都用 `GscAccentBrush` 与 `GscAccentIconFillBrush`，和“核心工具”“外观与可访问性”“设置迁移”标题保持同一角色。
- Light/Dark 下实际创建生产 `GameSaveCenterSettingsView`，展开备份、自动化分组并读取运行时资源解析后的两个图标前景和背景：均与对应 Accent token 为同一资源对象。两个分组“恢复本类默认”按钮仍以原 Automation Name 出现并保持启用。双主题 `2/2`：[TRX](SettingsSectionHeaders.trx)、[console](behavior-console.txt)。
- Info、Success、Warning、Error 的真实状态显示未做全局替换。Dashboard 游戏备份数指标与“全部备份”主按钮图标的 InfoBrush 尚在后续语义审查范围，不能据本项称全产品色彩盘点完成。

## 验证与视觉

- 精确源码提交 `c770d37c` 的仓库规范 Release build：`scripts/build.ps1 -Configuration Release -SkipTests`，全 solution `0 warning / 0 error`；XAML 结构检查 `24/24`：[build log](release-build.txt)。`scripts/validate-source.py` 与 `git diff --check` 通过。
- 精确 Release `net472` 测试 DLL 的双主题 WPF 行为 `2/2`，未出现 `InvalidComObjectException` 清理噪声。插件 SHA-256 `EEE8B6FDC260712959B424AB42FE8A1379832065539E4A879DB9FF8749B6FCE`，MVID `4fc9f1ad-3978-4ea4-82b8-0b0764e35790`；测试 SHA-256 `72F375FE2D31CBB1A887672D97FED279FD544A251CCCB3EF96BF158674FDA73D`，MVID `e2292fe8-99d1-4603-9ac8-7fcb4f24be51`。两者 ProductVersion 均为 `0.6.73+c770d37c478fe447241b65fc35934a093345022f`。
- 完整 OffscreenRenderHarness `render-qa OK`，report 记录 source commit `c770d37c`、`WorkingTreeClean=True`、Light/Dark 全矩阵、Settings `1040×700` 两主题均通过、逻辑离屏 DPI `1.00`：[摘要](render-qa-summary.txt)。设置页的深色备份/自动化截屏供视觉查看：[备份](Settings-backup-dark-1040x700.png)、[自动化](Settings-automation-dark-1040x700.png)。截图为 OffscreenRenderHarness，不是真实 Playnite 帧。
- R00/R01 freshness `14/14 FRESH`、无 STALE，package identity 未提供：[结果](R00-R01-freshness.txt)。

配色角色遵循 Windows 颜色指南对颜色层级和语义用途的建议；文案与图形仍提供非颜色信息，符合 WCAG 2.2 Use of Color：[Microsoft color guidance](https://learn.microsoft.com/en-us/windows/apps/design/signature-experiences/color)、[W3C WCAG 2.2](https://www.w3.org/WAI/WCAG22/Understanding/use-of-color)。

所有行为数据均为设置视图合成状态；没有启动真实 Playnite、读取或写入用户设置/存档、验证用户安装 DLL、物理 DPI、OS 输入或最终呈现帧。下一小批继续核对 Dashboard 统计/主按钮等非状态 InfoBrush 用途；Media Inbox 真实滚动空白仍需安全隔离宿主同进程 `[GSC-GRID-DIAGNOSTIC]`。
