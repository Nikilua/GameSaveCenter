# L42 真实宿主滚动复核边界（2026-09-09）

## 结论

本轮没有取得新的真实嵌入 Dashboard，也没有取得新的 `MediaInboxGrid`/`TaskGrid` replay JSON。因此 L42 不能作为视频问题 A/B 的通过证据，不能据此修改生产模板、`ScrollUnit`、Margin 或虚拟化策略。

当前仍以 L39 的真实 Playnite/FusionX 端点回放作为有效宿主证据，以宿主人工视频回归作为 B 类问题的未完成门禁。

## L41 受限环境结果

- 使用当前提交 `398a6f095718e2827c3e8bd2a19bbbb5525c1f16` 和隔离数据目录启动 Playnite。
- `playnite.log` 只记录到 `Application started`，随后 Playnite 进程退出；没有 `summary.json`、插件扩展日志或表格 replay。
- 隔离目录的 `cef.log` 记录 CEF Mojo channel `拒绝访问 (0x5)`。这说明该次受限执行环境没有提供可靠的宿主窗口，不是表格滚动根因证据。

## L42 提升权限结果

- 在同一隔离数据副本上以提升权限重跑，构建无警告/错误；Core `76/76`、Worker `311/311`、Playnite `431/488`（57 skip）。
- Playnite 进程确实启动并保持响应，但经过 60 秒 UI Automation 查找，`MainWindowHandle=0`，无法定位 `GameSaveCenter` 侧栏项；随后 90 秒等待仍没有 `summary.json`。
- 输出目录只保留 `runner-metadata.json`，没有 `scroll-replay`、`metadata` 或嵌入 Dashboard 截图；因此不存在可用于表格诊断的 L42 运行数据。
- 本轮由审计脚本启动的 Playnite PID 已在核验路径为 `D:\software\Playnite\Playnite.DesktopApp.exe` 后停止；用户 FusionX 文件、Playnite 全局样式和用户数据目录未修改。

## 当前可用的表格诊断结果

L39 真实嵌入回放仍显示：

- 两表负责行滚动的对象均为 `ScrollViewer`，`IScrollInfo` 为 `ScrollContentPresenter|DataGridRowsPresenter`，`CanContentScroll=True`、`ScrollUnit=Item`。
- `MediaInboxGrid`：400 条，底部 `verticalOffset=394/scrollableHeight=394`；实际 Presenter 为 `0,36,604×279.33 DIP`，水平条为 `0,315.33,604×12 DIP`；第 399 行位于 `256..300 DIP`，cell/visual/text 为 `5/5/5`，`lastRowComplete=true`。
- `TaskGrid`：50 条，底部 `verticalOffset=40/scrollableHeight=40`；实际 Presenter 为 `0,36,637.33×456 DIP`，水平条为 `0,492,637.33×12 DIP`；第 49 行位于 `432..476 DIP`，cell/visual/text 为 `6/6/6`，`lastRowComplete=true`。
- L39 的 47 个样本/表、底部 21 个样本中，空正文、大块首行间隙、末端水平条覆盖、选中内容缺失和尾项不完整均为 0。

这些数据支持“程序化端点下当前内容视口避开水平条且尾项完整”的结论，但不覆盖物理滑块拖动、滚轮/PageUp/PageDown/Ctrl+End、DPI/主题矩阵或原视频录屏；视频 B 类问题仍为宿主人工待验收。

## 下一步边界

只有取得可绑定的真实 Playnite UI 窗口并按视频动作采集到诊断时，才能按 `Items.Count/CollectionChanged/请求代际/实际滚动器/Presenter DIP/首末行/cell visual-text-clip/锚点` 区分集合变化、偏移越界、容器错位、单元格回收和 Reset 顺序。当前证据不足以选择其中任何一个作为视频根因。
