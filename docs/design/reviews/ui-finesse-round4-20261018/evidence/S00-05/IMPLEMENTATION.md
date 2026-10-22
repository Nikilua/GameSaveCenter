# S00-05 上下文预览实现与定向验证

日期：2026-10-22

## 实现

- 新增不可变 `CommandDirectoryContextSnapshot`，在面板每次打开时捕获工作区、游戏稳定 ID/名称和任务稳定 ID/关联游戏信息。
- 目录项在 `CanExecute` 与执行转发前复核快照。游戏/任务选择或集合变化后，失效命令被禁用并给出重新选择原因；工作区搜索与游戏选择入口也会在工作区变化后失效。
- 命令浮层在结果列表上方显示当前选中命令的作用对象及不可用原因。预览容器复用动态主题资源，不接收焦点；结果列表仍保持原有虚拟化、方向键、Enter、Escape 与 IME 路由。
- 扩展行为测试，覆盖上下文预览、游戏选择变化、任务选择变化、任务对象移除，以及预览随结果选择更新和不可用原因可见。

## 定向验证

- SDK：8.0.423；Configuration：Release。
- 命令目录定向测试：21/21 通过，0 失败、0 跳过；包含 `CommandDirectoryBehaviorTests`、`CommandDirectoryKeyboardBehaviorTests` 与 `CommandDirectorySearchTests`。
- `python scripts/validate-source.py .`：通过。
- `validate_wpf_ui.py`：0 错误；仓库整体 26 条既有警告，改动文件无告警。
- 标准根命令 `GameSaveCenter-Run.cmd` 的完整编译、全量测试、打包、安装与 Playnite 启动验收尚待本次实现提交后执行。

## 验收边界

定向 STA/WPF 行为测试不代表真实 Playnite 窗口、物理键盘或真实 IME 候选验证。该宿主验收继续保持 pending。
