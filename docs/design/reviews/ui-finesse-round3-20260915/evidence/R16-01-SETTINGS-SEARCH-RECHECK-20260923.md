# R16-01 设置搜索定位定向复核

日期：2026-09-23

复核提交：`ae9aedbc`（`codex/ui-finesse-round2`，D 盘工作区）

## 结论

R16-01 的受控实现条件已满足，账本状态校正为“已满足，待环境验证”。本批没有新增生产代码，复用了 `a4e35578` 的设置字段索引、搜索分类切换、可见性恢复和原有控件/绑定，仅复测当前提交上的 STA WPF 行为与门禁。

## 实际复测

- `R16SettingsSearchBehaviorTests`：独立 testhost `1/1` 通过。输入“恢复巡检间隔”后切到自动化分类，匹配数值字段保持可见且可编辑，普通 Worker 字段隐藏；清空搜索后恢复搜索前分类和字段可见性，设置路径与 pending edit 不变。
- `SettingsValidationSourceTests`：独立 testhost `1/1` 通过；`SettingsValidationNavigationBehaviorTests`：独立 testhost `1/1`；`SettingsDraftLifecycleBehaviorTests`：独立 testhost `1/1`。原有验证错误定位和脏草稿焦点恢复链未被搜索状态吞掉。
- Playnite `net462` 定向构建实际完成，无新增错误；保留 `MediaCenterView.xaml.cs:706` 两条既有 `CS8602` warning。
- `python scripts/validate-source.py`、XAML 结构检查 `24/24`、`git diff --check` 通过。

## Testhost 边界与保留能力

- 将验证导航与草稿生命周期放在同一 WPF testhost 会触发既有 `System.Windows.Application` 多实例限制；拆为独立 testhost 后两项均 `1/1`，这条环境边界不写成产品失败或隐藏。
- 搜索继续复用设置页原有 TextBox、ComboBox、CheckBox、ToggleSwitch、分类滚动、保存/取消、验证错误和命令绑定；搜索文本不写配置对象，不重建设置 DTO 或全局导航。
- 测试使用合成设置、fake/隔离目录和受控 STA WPF；未读写真实存档、媒体、云端、用户诊断或系统剪贴板。未验真实 Playnite/package-host 的最终布局、浅/深主题呈现、UIA/读屏、IME、DPI/跨屏、presented frame、ETW 或宿主性能。
- Demo 原目录不可用，沿用已恢复生产基线，没有引入新的设计体系。

下一可执行任务：`R16-02 策略差异预览`。先核对策略/模板 DTO、继承值与显式覆盖值，再以只读差异预览和取消无写入为边界推进。
