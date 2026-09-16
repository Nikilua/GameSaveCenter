# R03-02 阅读层级校准

日期：2026-09-17  
状态：已满足（受控生产 XAML 与窄窗离屏验证；真实 Playnite 宿主最终呈现仍未验）  
代码提交：`e889b0480a5d05dc1ba23ffeae5aed19053ecd58`（`codex/ui-finesse-round2`，干净工作树）

## 任务边界

R03-02 要求按标题、正文、辅助、技术文本建立实际使用清单，消除无意义字号变体，保证正文不靠低透明度弱化，并用生产窄窗截图检查挤压。本阶段先核对最新分支已有的 `Themes/Typography.xaml`、`Themes/Redesign.xaml` 和各页面入口，没有引入新的设计体系；只把生产页面中可明确归类的 9.5/10.5/12.5 字面字号收敛到现有共享令牌。

## 实际使用清单与实现

| 层级 | 共享来源 | 当前生产入口/语义 | 说明 |
| --- | --- | --- | --- |
| 标题 | `GscTypographyPageTitle` / `GscTypographySectionTitle`；22/16 DIP；展示标题 28 DIP | 页面标题、分区标题和现有 `GscPageTitleStyle`/`GscSectionTitleStyle` | 保留 Demo-first 页面层级，不把标题改成正文字号 |
| 正文 | `GscBodyFontSize`；14 DIP；`GscTypographyBody`/`GscBodyStyle` | 壳层品牌、媒体卡片文件名、Overview 任务类型/活动游戏名/问题标题 | 主标识不再使用 12.5 或 10.5 微调 |
| 辅助 | `GscCaptionFontSize`；12 DIP；`GscTypographyCaption`/`GscCaptionStyle` | 版本、Worker/Ludusavi 状态、媒体时间/云端状态、任务上下文/详情、比较质量徽标 | 继续用语义颜色区分层级，不叠加局部低透明度 |
| 技术文本 | `GscTypographyCode` / `GscPathText`；代码字体与 Caption 令牌 | Save、Media、Maintenance、Trainer 的路径/目录/可复制技术值 | 保留单行省略、Tooltip/复制入口和现有路径样式 |

修改前生产 Views/Settings 扫描得到 `FontSize="12.5"` 5 处、`10.5` 9 处、`9.5` 2 处；修改后均为 0。对应映射为：主标识/文件名/主标题使用正文令牌，状态/时间/详情/比较徽标使用 Caption 令牌。生产源码直接扫描未发现 `TextBlock` 局部低透明度；剩余 `Opacity=0.22`/`0.9` 分别属于装饰 Rectangle/ambient Grid，不是正文弱化。

## 验证结果

1. `ProductionTypographyUsesSharedHierarchyWithoutMicroSizeDrift` 从真实生产 Views/Settings 逐行检查微字号负例，并核对壳层、媒体、Overview、Save 的正文/Caption 映射；最终 clean 隔离程序集定向 `10/10`，`0` 失败、`0` 跳过。
2. `scripts/build.ps1 -Configuration Release -OutputRoot .tmp\\r03-02-build-final` 对 `e889b04` 通过：XAML `24/24`；解决方案构建 `0 warning / 0 error`；Core `83/83`；Worker `311/311`；Playnite `547` 通过、`57` 跳过、`0` 失败，总计 `604`。
3. `scripts/render-qa.ps1 -Configuration Release -Output .tmp\\r03-02-render-initial` 绑定 `e889b0480a5d05dc1ba23ffeae5aed19053ecd58`，报告 `WorkingTreeClean=True`、`render-qa OK`。Light/Dark 的 1040×700 生产窄窗均完成 Overview、Save、Trainer、Media、Maintenance、Task、Settings 探针；Overview 工作区为 `744×460 DIP`，Save/Media/Maintenance/Task 的列表与页级滚动仍可达，表格可读行探针均通过。
4. 人工查看了 Light/Dark Overview 1040×700、Media/Save/Maintenance 生产窄窗代表图；中文、英文、数字及状态信息保持清晰，媒体卡片文件名和时间状态未产生正面裁切，操作区未因字号变化被推离首屏。截图是受控 WPF offscreen logical DIP，不当作真实屏幕呈现。
5. `python scripts/validate-source.py`：通过。

本阶段只改 Typography 入口和测试；没有改游戏选框、滚动条系统、命令/Binding、取消/错误语义、恢复保护、有限列表策略或 Playnite/net462 契约。验证使用合成数据与隔离输出，没有执行真实备份、恢复、媒体删除、云端写入或外部诊断发送。

## 未验边界与下一步

受控证据不等价真实 Playnite 最终 presented frame、用户安装字体差异、物理 DPI/跨屏、OS 输入/IME、屏幕阅读器、ETW 或宿主帧率/性能；57 条既有 UI 基线跳过仍按项目规则保留，不能被本项的窄窗离屏结果替代。下一可执行任务为 R03-03 双语基线。
