# U05-03/04 详情对象与弹层退出复核

日期：2026-10-28。基线 HEAD：`ac633623`，生产实现继承 `237e993f`；本批只修改行为测试，无生产资源或布局差异。

- U05-03：生产 TaskCenterView 的选择变化立即归一化旧详情过渡。新增同一 dispatcher turn、尚未 UpdateLayout 的断言：标题不残留 A，SelectedTask 与查看关联游戏按钮的绑定及实际 OnClick 命令目标均为 B。使用合成 ICommand 哨兵，不导航真实游戏。Light/Dark × 1120/1400 DIP 共四种条件。
- U05-04：复用 DialogLifecycleStateMachine、DialogOverlayMotion 与触发器回焦路径；新增退出期间确认点击及重叠请求负例，原取消值不变、新请求安全取消、标题不被覆盖、完成后回原触发按钮。菜单沿用共享 GscPopupAnimation（运行时随动效偏好 Fade/None），对话框 Normal、任务详情 Fast。游戏选框属于保留例外。本批尚未补齐菜单实际开闭与所有抽屉回焦的组合证据，因此 U05-04 保持实施中。

SDK8.0.423 / Release 本地定向最终 **9 passed / 0 failed / 0 skipped**，构建退出0。覆盖 R06DetailsBudgetBehaviorTests、R08DialogOverlayBehaviorTests 与 DashboardConfirmationKeepsTabFocusInsideAndRestoresItsTriggerAfterClose。TRX见 tests/，哈希见 tests/hashes.json。此次构建基于 ac633623 加本批未提交测试差异，尚不是新提交精确身份结果。

首轮4项失败来自夹具只 RaiseEvent 而未调用 ButtonBase.OnClick，因此未执行命令；改为真实按钮点击方法后保留目标断言。第二轮8通过/1失败是源码守卫缺 GSC_BUILD_COMMIT；补齐完整HEAD后9/9。两次失败TRX均保留，不删断言、不增加skip。初次默认权限还原会话无测试结果，父进程47852后来已不存在，不计通过。

Source validation通过；WPF静态37 XAML、0 errors、26 warnings、179 info（现有基线）；XAML检查结果见 xaml-validation.txt。生产画面前后相同，视觉证据继承相同生产代码的 [U05-01/02双主题394 PNG Render QA](../U05-01-02/BASELINE.md)，本批未重新生成截图。真实Playnite、系统键盘/IME、UIA客户端和物理DPI未验。

当前源码提交的CI待提交后查询；不能由父提交绿色推断本批绿色。最终中文根入口完整构建/测试/发布/打包/安装身份/启动与独立 scripts/build.cmd 留在整轮最终候选门禁。

本阶段临时输出仅 `.tmp/round5-u05-03-04` 的三份原始TRX，保留为当前失败/成功诊断；持久证据为脱敏副本。开始记录D盘空闲16,163,041,280字节，归档前C盘3,333,009,408、D盘16,162,111,488字节。本批无旧包/截图副本，也无已确认可删的自有目录；删除清单为空。用户/其他阶段TRX和TestResults未动。
