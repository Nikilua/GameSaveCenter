# CLOSE-SDK-01：SDK与编译器基线（2026-10-07）

本阶段统一`global.json`、CI安装入口及构建预检为稳定SDK **8.0.423**、精确匹配`rollForward=disable`、C# **12.0**。目标框架、NuGet依赖、业务行为和0.6.73版本不变。完整Release/包/当前runner门禁尚未全部通过，CLOSE-SDK-01与CLOSE-CI-01保持`IN_PROGRESS`。

## 选择依据与契约

- SDK8.0.423是此前本地完整编译与Core/Worker验证使用的实际版本；[官方发布元数据](https://builds.dotnet.microsoft.com/dotnet/release-metadata/8.0/releases.json)确认其2026-07-14发行，runtime8.0.29。选定行及Windows x64下载地址/SHA512保存在[sdk-official-metadata.json](sdk-official-metadata.json)，本阶段未安装SDK或升级运行时。
- [.NET SDK选择规则](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json)说明`disable`要求精确版本；原`latestMajor`允许预装较新SDK改变编译器。本阶段CI使用[setup-dotnet v4的global-json-file](https://github.com/actions/setup-dotnet/blob/v4/README.md)，仅以仓库声明安装。
- [C#版本配置文档](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version)说明`latest`随安装编译器变化。本阶段显式12.0，实际MSBuild属性见验证日志。暂不新增自动SDK10兼容通道；未来升级须显式修改SDK/语言契约并通过完整门禁。历史SDK10结果仍单独保留。
- build与独立WPF runner都验证原生`dotnet --version`和声明相等；缺失或不匹配会给所需版本、安装指引、实际版本和退出码，不接受其他大版本替代。

## 先核对前阶段远端结果

| 运行 | 实际身份与结果 |
| --- | --- |
| [37565955357](https://github.com/Nikilua/GameSaveCenter/actions/runs/37565955357) | 源码48b78b3beb0465083f420b1df23cd4c973a06ea8；SDK10.0.401；Core125、Worker357、source470通过/18原有skip；侧栏22/22；WPF前23类通过，第24类Q14失败 |
| [37566483862](https://github.com/Nikilua/GameSaveCenter/actions/runs/37566483862) | 文档提交c7f2a3689293c35e1ad98faff05e821b12d22070；SDK10.0.401；同一Q14错误态高度断言失败；独立摘要和相关TRX保留 |

两次终态均失败，失败时仍成功上传`GameSaveCenter-build-diagnostics`，已实际下载核对。[第一运行完整诊断](ci-sdk10-37565955357/summary.json)包含各成功/失败类console、TRX及DLL SHA/MVID；[第二运行摘要](ci-sdk10-37566483862/summary.json)和相关类证据单独保留，避免重复整套大日志。

默认偏好测试的真实runner输出为`highContrast=False; clientAreaAnimation=False; expected=False`。前阶段改正后的侧栏22项在SDK10实际通过，支持测试前提修复；旧失败运行当时的具体偏好未记录，不能补造。Q14的失败是验证错误态搜索框36→37 DIP，严格容差0.75；COM收尾输出不是该断言的原因。

## 本阶段本地验证

- PowerShell5.1与7均通过SDK自测。真实缺失旧SDK1.0.100时精确匹配失败，原生退出码`-2147450735`；同机同一可丢弃目录改回原`latestMajor`后选择8.0.423并返回0，构成可区分的回归对照。额外合成10.0.401返回0输入被预检拒绝。本地只装SDK8，不能据此宣称实际双SDK矩阵通过。
- [PS5证据](sdk-selftest-ps5/summary.json)、[PS7证据](sdk-selftest-ps7/summary.json)保存原生选择日志、负例、旧策略对照及可理解错误。`runType=sdk-baseline-tests`下的预期非零步骤是自测负例，不是成功构建。
- 统一基线后的Release编译0 warning/0 error、XAML24/24，Core125/125、Worker357/357、source470 passed/18原有skip/0 failed，侧栏22/22。WPF前23类通过、第24类Q14仍在错误态36→37 DIP失败，后89类未执行；完整build脚本终态exit1。完整本地[摘要](local-sdk8-full/summary.json)、[Q14 TRX](local-sdk8-full/tests/wpf.Q14ToolbarAlignmentBehaviorTests/wpf.Q14ToolbarAlignmentBehaviorTests.trx)保留失败，固定SDK没有使布局转绿。
- SDK阶段验证遇到PATH中两个`git.exe`匹配：原采集器拼接两路径导致启动失败，未丢失为成功。已修复原生命令解析为首个PATH匹配。两个不同原生程序以同名夹具验证首路径退出7、次路径会返回不同错误；PS5.1/7均通过，PATH进程环境在finally恢复。此额外问题与Q14布局分别记账。
- 隔离STA探针确认错误边框1→2 DIP导致搜索框36→37 DIP；只在合成窗口把参与测量的边框恢复1 DIP，控件恢复36 DIP且Validation.HasError仍true。[测量报告](textbox-measure-diagnostic.json)。这是共享模板根因定位，尚无生产UI修复，不是Playnite/DPI验收。
- 独立调用同DLL的既有私有几何夹具，Light/Dark654 DIP七个单行控件都残留20 DIP margin；两个主题620/660正例通过，Light620→660→620往返也通过。[六场景报告](preset-boundary-diagnostic.json)。这是绕过前面错误态断言以单独观察后续问题，完整Q14仍按原入口执行并记录失败，没有放宽断言。
- `git fsck --full`本阶段实际退出0，仅既有dangling对象；源码、PowerShell AST、MSBuild语言属性与diff结果在验证目录归档。构建源身份是c7f2a368加明确dirty修改，不能当成下一提交的clean DLL。
- 全量build开始后只精炼采集/SDK自测/runner脚本与workflow，编译的C#/XAML/项目配置输入未变；[最终快照对照](source-snapshot-comparison.json)列出全部变更路径。构建起始与最终[验证快照](verification/summary.json)分别保留，不能把早期脚本hash当最终交付源码。

## 后续与边界

先完成Q14共享错误态测量和654 DIP单行边距修复，再跑所有113类、完整CI渲染/包门禁以及精确8项freshness补证。固定SDK不能解释或关闭Q14；baseline未改，本地旧d57a613f包未安装。没有修改系统动画偏好、运行真实Playnite或操作真实存档、媒体、云端。ENV-001、用户Q06/R08原失败和同候选发布矩阵继续开放。
