# U05-03/04 详情对象与弹层退出复核

日期：2026-10-28。基线 HEAD：`ac633623`，生产实现继承 `237e993f`；本批只修改行为测试，无生产资源或布局差异。

- U05-03：生产 TaskCenterView 的选择变化立即归一化旧详情过渡。新增同一 dispatcher turn、尚未 UpdateLayout 的断言：标题不残留 A，SelectedTask 与查看关联游戏按钮的绑定及实际 OnClick 命令目标均为 B。使用合成 ICommand 哨兵，不导航真实游戏。Light/Dark × 1120/1400 DIP 共四种条件。
- U05-04：复用 DialogLifecycleStateMachine、DialogOverlayMotion 与触发器回焦路径；新增退出期间确认点击及重叠请求负例，原取消值不变、新请求安全取消、标题不被覆盖、完成后回原触发按钮。菜单沿用共享 GscPopupAnimation（运行时随动效偏好 Fade/None），对话框 Normal、任务详情 Fast。游戏选框属于保留例外。本批最初快照尚未补齐菜单实际开闭与抽屉回焦组合；后续 `U05-04-help` 已补充说明弹层开闭、焦点进入、Escape 与回焦覆盖，当前以追补证据为准。

SDK8.0.423 / Release 本地定向最终 **9 passed / 0 failed / 0 skipped**，构建退出0。覆盖 R06DetailsBudgetBehaviorTests、R08DialogOverlayBehaviorTests 与 DashboardConfirmationKeepsTabFocusInsideAndRestoresItsTriggerAfterClose。TRX见 tests/，哈希见 tests/hashes.json。此次构建基于 ac633623 加本批未提交测试差异，尚不是新提交精确身份结果。

首轮4项失败来自夹具只 RaiseEvent 而未调用 ButtonBase.OnClick，因此未执行命令；改为真实按钮点击方法后保留目标断言。第二轮8通过/1失败是源码守卫缺 GSC_BUILD_COMMIT；补齐完整HEAD后9/9。两次失败TRX均保留，不删断言、不增加skip。初次默认权限还原会话无测试结果，父进程47852后来已不存在，不计通过。

Source validation通过；WPF静态37 XAML、0 errors、26 warnings、179 info（现有基线）；XAML检查结果见 xaml-validation.txt。生产画面前后相同，视觉证据继承相同生产代码的 [U05-01/02双主题394 PNG Render QA](../U05-01-02/BASELINE.md)，本批未重新生成截图。真实Playnite、系统键盘/IME、UIA客户端和物理DPI未验。

当前源码提交的CI待提交后查询；不能由父提交绿色推断本批绿色。最终中文根入口完整构建/测试/发布/打包/安装身份/启动与独立 scripts/build.cmd 留在整轮最终候选门禁。

本阶段临时输出仅 `.tmp/round5-u05-03-04` 的四份原始TRX，保留为当前失败/成功诊断；持久证据为脱敏副本。开始记录D盘空闲16,163,041,280字节，归档前C盘3,333,009,408、D盘16,162,111,488字节。本批无旧包/截图副本，也无已确认可删的自有目录；删除清单为空。用户/其他阶段TRX和TestResults未动。

提交后复核：实现/测试提交18d73836e58946eb85c8db1c50a85519d14e1b3d已推送；冻结跟踪文件后SDK8.0.423 Release精确身份9/9、0 skip、退出0，见tests/u05-03-04-exact-18d73836.trx。最初查询的run40638353451现已终态success：baseline job122650472010、SDK10 job122650471976均成功；artifact IDs、大小、digest见[终态记录](ci-run-40638353451-final.json)。旧 `ci-run-40638353451.json` 保留为当时的运行中快照，不代表当前状态。U05-04后续提交498dd3a4、bca60704、5ee7be00的双SDK终态也成功，见[追补CI汇总](../U05-04-help/ci-runs-final-20261109.json)。required-checks既有403不重试。

2026-11-09 盘点：`.tmp/round5-u05-03-04` 当前不存在；本次没有删除该目录内容或认领空间回收。文档Source Validation、7个变更Markdown文件的1012条本地链接检查及 `git diff --check` 通过，清理清单见[文档检查记录](doc-check-20261109.json)。原始TRX/归档证据仍保留，用户及其它任务的TRX、TestResults未动。真实Playnite及外部UIA/IME/DPI仍待验；后续整合源码SHA `25bcbf79` 的run #1423双SDK成功，详情见追补CI汇总中的链接。
