# 最近任务 / 生产列表选框完整包裹右侧内容：6cd0f9ec

## 问题与修复

用户反馈的截图显示，最近任务选中框右缘没有包住 `10-20 10:36` 时间；存档表格行框也在时间/最后一列之前结束，最右侧文本看起来落在框外。共享选择装饰的右侧空隙与 DataGrid 宽度换算叠加过大，选框实际边界被推到末列内容左侧。

在精确源码提交 `6cd0f9ec55ada37f03a88d24bc46690ed1837a06` 中，将公共 ListBox 和生产 DataGrid 的选框右侧 inset 统一收至约 4 DIP，并同步背景 overlay 的范围与行宽换算。选框仍是独立 overlay，不改变行内容、时间文本、按钮命令、Binding、虚拟化、滚动条或选中/悬停/焦点状态。最近任务、存档列表、任务队列及 Shell 内任务表的右侧文字现在全部落在选框内部，右侧仍留有窄间距以区分滚动条。

## 修复后图与验证

本目录 `screenshots/` 保存 SDK 8.0.423 Render QA 对固定 1040×700 样本生成的修复后图。Overview 的“今天 07:30”、Save 选中行最后一列、Task 的失败状态/详情及 Shell 内嵌任务表均位于选框右缘以内；边框没有遮挡文字，也没有把行内容向左挤。完整双主题 Render QA 报告为 `render-qa-report.txt`：334 张图，0 条 `PROBLEM`，结果 `render-qa OK`，报告 SHA-256 `5E1D25692EFDB9477C0EA35B1FDE2A0D598B2CCC879CFCDBA738C7F498F8B8E3`。修复后样图：[Overview](screenshots/Overview-1040x700.png)、[Save](screenshots/Save-1040x700-tab0.png)、[Task](screenshots/Task-1040x700.png)、[Shell任务表](screenshots/Shell-Tasks-1040x700.png)。

离屏探针使用 logical DPI scale 1.00；这不代表物理显示器或 Playnite 实页已验收。

- R06 选中状态/容器复用行为、R10 最近任务/共享 ListBox 几何、R23 生产 DataGrid 状态和共享资源断言均在根入口全量测试中通过；针对性 TRX 与完整 158 份 TRX 摘要见 `tests/` 和 `root-summary.json`。
- Source Validation 退出码 0；WPF 静态审查扫描 88 个 XAML，0 错误、78 warnings、507 info（既有审查提示，不计作错误）；输出见 `source-validation.txt`、`wpf-static-audit.txt`。
- 独立 `scripts/build.cmd` 对同一提交 SDK 8.0.423 Release 编译/测试退出码 0：2026 项、1958 通过、0 失败、68 条 skip（不计通过）。TRX标明其中57条是已撤销架构断言、1条系统鼠标 opt-in、10条是本执行环境禁止Named Pipe创建。
- 实际中文入口 `GameSaveCenter-一键构建安装运行.cmd` 转交 `GameSaveCenter-Run.cmd`，SDK 8.0.423 Release 完整编译、测试、发布、打包、安装身份核对与 Playnite 启动退出码 0：8 项目、0 warning、0 error；158 份 TRX 共 2026 项、1968 通过、0 失败、58 条 skip（57条已撤销架构断言、1条鼠标 opt-in，不计通过）。首次调用只在空间预检不足 4 GiB 时退出，按记录清理本阶段已确认过期的输出后重跑通过；清理清单和空间前后见 `cleanup-before-root.json`。独立 build.cmd 按其自身构建职责验证，没有代替安装启动链。 证据归档后又精确删除本阶段 .tmp/round5-u09-final-6cd0f9ec-20261103/：1,454个文件、229,398,142 bytes；D盘空余从4,048,928,768增至4,282,720,256 bytes，清理记录见 [cleanup-final.json](cleanup-final.json)。
- 本机 ZIP 与 PEXT 各 45,737,900 bytes，SHA-256 均为 `B184FBFA5A6904084BD354BBF406EDFBCEDA00BFDF1CBFB79ECAE2C915F4D2DF`；六个已安装程序集的版本、SHA、Playnite 路径和启动身份见 `root-gates.json` 与 `last-dev-install.txt`。 归档文本将本机账户、机器标识及APPDATA用户名脱敏/规范化，原始artifacts transcript未改；改写前后哈希见 [脱敏记录](log-redaction.json)。
- 精确提交 push run [#1446](https://github.com/Nikilua/GameSaveCenter/actions/runs/41613860203) completed/success。基线 job `125842266009` 的 15 步、SDK 10.0.401 job `125842265806` 的 8 步均成功。三个 artifact ID、SHA-256 与源码 SHA 见 `ci-run-41613860203.json`。required-check/规则集读取此前 HTTP 403，本批没有重复探测；完整性仍标为“远端待验”。

## 待验边界

Playnite 已通过实际安装链启动且安装 DLL 身份已核实，但没有观察插件页面的实时布局；Render QA 是离屏验证。外部 UI Automation 客户端、实体键盘/IME、物理 DPI 与其它主题宿主仍待真实验收。没有执行真实存档恢复、媒体删除或云端写入。WPF 页面代码已完成，宿主视觉与输入验收仍待验收。
