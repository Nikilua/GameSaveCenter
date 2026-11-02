# U06-02 / U08-08 最近活动选框对齐

日期：2026-11-02。生产提交 `f9d09cb2b7665355a74926e2cd4a801567c60baa`（`对齐最近活动选框`）已推送 `origin/main`。本批落实用户对首页最近活动选中效果的补充：蓝色圆角框可以保留，选中行不应为外框右移或下移。

## 实现与选择态盘点

- `OverviewView.xaml` 的 Overview 专用模板移除了选中触发器上的 `TaskItemChrome.Margin=4,2`。原内缩会把描边放在内容槽内部，造成行内容与选框视觉错位，极端时图标贴近裁切边界。现保留原选中色与 14 DIP 圆角描边，描边不再施加布局偏移。
- 共享隐式 `ListBoxItem` 模板选择/焦点描边已在内容层外绘制且 Margin 为零。本批的 `R10RecentAccessBehaviorTests` 同时约束Overview模板与公共列表项的未选中、选中、键盘焦点三态：容器、图标tile及其相对列表槽位几何保持不变。
- 盘点其它选择效果：`DataGridRow`和媒体卡片已有独立覆盖层；Overview保护项和ComboBox只改变颜色。本批不为统一外观重写无偏移问题的样式。
- 命令、Binding、UI Automation契约、键盘/IME路径、虚拟化、业务状态、游戏选框和滚动条系统均未改。

浅色与深色主题首页截图分别见[Light](overview-light-1040x700.png)和[Dark](overview-dark-1040x700.png)，尺寸为1040×700 DIP。完整离屏报告在[`render-qa-report.txt`](render-qa-report.txt)。报告绑定精确提交SHA，时间固定为 `2026-11-02T00:00:00Z`；Light/Dark共394张PNG、0 `PROBLEM`，逻辑DPI 1.00。它不代表Playnite宿主、外接显示器物理DPI或真实输入验收。

## 源码与行为验证

- 精确SHA的 `R10RecentAccessBehaviorTests`：3/3通过、0失败、0跳过，TRX已复制到本目录；完整候选全量运行中的同类也为3/3。测试源码通过 `python scripts/validate-source.py`。WPF静态审查覆盖26份XAML、0错误、26条既有警告、164条信息。
- 当前SDK 8.0.423 Release底层完整脚本直接运行成功，解决方案0警告/0错误；158份TRX共2025项、1967通过、0失败、58未执行，未执行项不计为通过；Core 157/157、Worker 401/401、Playnite源码557通过/18未执行、155个WPF隔离类852通过/40未执行。58项均是既有skip：57条断言指向已撤销的“今日工作台”架构，另1项为默认跳过的系统鼠标指针移动测试。完整TRX清单、哈希和分类见[`local-gates.json`](local-gates.json)；原始日志和TRX保留在仓库 `artifacts/` 下记录的路径。

## 构建、安装与入口状态

当前精确源码直接调用 `scripts/dev-install-run.ps1` 完整Release构建、测试、Worker发布、扩展打包、安装身份核验与Playnite启动，退出码0。ZIP/PEXT均45,737,931 bytes、SHA-256 `CE8AE7BBD4E49E1215B6151FD10317F02B22A728BCC966E442BE025D1F81BE29`。安装清单版本0.6.73；插件程序集身份 `0.6.73+f9d09cb2b7665355a74926e2cd4a801567c60baa`。磁盘程序集SHA、报告与日志见[`local-gates.json`](local-gates.json)。Playnite进程已启动，路径 `D:\software\Playnite\Playnite.DesktopApp.exe`；该身份检查不证明插件页面已经人工观察。

实际存在的中文入口 `GameSaveCenter-一键构建安装运行.cmd` 转交 `GameSaveCenter-Run.cmd`，后者调用 `scripts/dev-install-run.ps1`；`scripts/build.cmd` 独立调用 `scripts/build.ps1`。在前序提交`0067525f768a26b82a00ee0dd7966b51d06c3bfe`执行这两个cmd均以退出码1失败：子PowerShell为ConstrainedLanguage，拒绝对不同语言模式定义进行dot-source（原始日志已复制到本目录）。这些入口和相关脚本在`0067525f`至当前源码提交间未变，故按用户要求不重复试探相同拒绝；本轮直接脚本的成功不冒充中文根入口或独立`build.cmd`通过。U09-05/U09-08继续进行中，不能结案。

## GitHub与未验边界

`windows-build.yml` run [41449095659](https://github.com/Nikilua/GameSaveCenter/actions/runs/41449095659)（#1432）绑定精确head SHA `f9d09cb2b7665355a74926e2cd4a801567c60baa`。截至 `2026-11-02T14:00:23Z` run为 `completed/success`：baseline job `125289540888` 15/15步成功，含编译/测试、Render QA、WPF静态审查、Playnite扩展包生成及上传；SDK 10.0.401复核job `125289540494` 8/8步成功。三个绑定该SHA的产物为preview `13148495442`（90,790,562 bytes，`sha256:7e05b81fd9d159308cb4e3a4917dc0daeae91104c339a9895c0f6d7cb10c6304`）、baseline diagnostics `13147039787`（2,029,488 bytes，`sha256:b184e7e9e0795880bac33a9d2ba72fc20ad7ac5107a8bd829028481bb6691551`）和SDK10 diagnostics `13145586337`（974,304 bytes，`sha256:2c6258e938242f28c746a7f483e8a9ca3632632f7430b6c90036704bab3f5058`）。活动job期间日志接口曾返回`BlobNotFound`，仅记接口时点，不影响后续核实的终态；run/job/artifact字段见[`ci-run-41449095659.json`](ci-run-41449095659.json)。读取required checks/保护设置的接口此前403，不重复查询，U09-06仍记“远端待验”。

Playnite真实界面外观、外部UIA、实体键盘/IME与物理DPI仍未验；没有执行真实存档恢复、媒体删除或云端写入。当前80项计数维持5项已满足、71项代码完成待验收、U09-04/05/08进行中、U09-06远端待验。

空间快照和保留产物见[`cleanup.json`](cleanup.json)；先前删除两个已核实的过期阶段构建目录清单见[`cleanup-stale-build-roots.json`](cleanup-stale-build-roots.json)。当前候选构建、包、全量TRX、Render QA原始输出继续保留；未删除用户或未知文件。
