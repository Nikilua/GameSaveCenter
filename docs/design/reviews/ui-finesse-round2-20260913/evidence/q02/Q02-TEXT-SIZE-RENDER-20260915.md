# Q02-07 当前生产正文尺寸视觉证据

采集日期：2026-09-15（Asia/Shanghai）。提交：`3811673`；来源为 clean-tree `RenderHarness audit`，`DpiScale=1.00`，窄窗口为 1040×700 逻辑 DIP。该证据与源码门禁合并使用，不替代真实 Playnite 宿主、物理 DPI 或最终 GlyphRun。

## 窄窗生产画面

- [Overview](text-size-20260915/overview-1040x700.png)：首页标题、云端状态、六项指标和操作按钮保持清晰层级。
- [Save 历史版本](text-size-20260915/save-history-1040x700.png)：表头、时间、文件数、大小、设备、备注和锁定状态均保持可读。
- [Media 当前游戏媒体](text-size-20260915/media-current-1040x700.png)：媒体文件名、时间、上传/失败状态和底部操作保持可读。
- [Maintenance 保留策略](text-size-20260915/maintenance-retention-1040x700.png)：策略说明、统计数字、单位和重新计算入口未因窄窗降级为不可读小字。
- [Trainer 已绑定工具](text-size-20260915/trainer-tools-1040x700.png)：工具名、来源/版本、导入状态和确认操作保持可读。
- [Task Center](text-size-20260915/task-center-1040x700.png)：搜索、筛选、任务列、状态、进度和详情入口均可读。

## 门禁与边界

- 本次审计汇总为 `HIGH=0`、`MEDIUM=0`、`Fidelity=0`、失败路由 `0`；源码门禁继续确认生产 Views/Settings 没有显式 `FontSize="10"`/`FontSize="11"`，109 处统一到 `GscCaptionFontSize`。
- 本组截图签收 Q02-07 的离屏视觉列。Settings 专页本轮审计路由没有加载有效内容，因此 Settings 视觉不由空白截图冒充通过；真实宿主小窗口、125/150/175/200% DPI、字体安装差异、IME 和 GlyphRun 仍保持边界。

