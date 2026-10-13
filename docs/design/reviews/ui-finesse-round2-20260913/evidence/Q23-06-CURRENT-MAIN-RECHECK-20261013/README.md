# Q23-06 目录编辑器当前主线复核

日期：2026-10-13。复核基线为 main `c9f9179d9b8d38e506dbb1402ae7030eafebea6c`；证据是在该提交的工作树上生成，工作树含本阶段改动。

Q23-06 的离屏视觉和受控行为条件通过；最终状态仍为“未完成”，因为真实 Playnite 宿主、物理 DPI 与真实文件对话框/剪贴板路径未验。

## 现有能力与改动

- 复用 `SettingsPathEditorService.Probe`、现有字段 Binding、全量设置校验和浏览/校验/打开/复制命令。路径探测只读；打开仍要求当前字段有效，不回退父目录。
- 受控视觉探针暴露了共享 ComboBox 模板的真实问题：选中项虽有 `DisplayName`，嵌套模板却依赖内部 `SelectionBoxItemTemplate`，导致路径选项显示 DTO 类型名。现在内部 `SettingsPathEditorOption` 的 `Key`/`DisplayName` 为 public getter，供 WPF `SelectedValuePath`/`DisplayMemberPath` 使用；共享模板使用公开 `ItemTemplate` API，并在设置 `DisplayMemberPath` 时从 `ComboBox.Text` 显示选中标签。自定义 `ItemTemplate` 仍走 ContentPresenter。
- RenderHarness 的 `settingspathprobe` 用生产 Settings 视图/资源、合成沙箱和占位 Worker 文件。占位文件从未执行。唯一长路径是沙箱内随机命名、未创建的子目录；探针没有调用 Browse/Open/Copy/Save。
- Demo 设计源目录在当前工作区不可用；本轮沿用恢复后的生产 Settings/共享资源基线，没有另换一套设计体系。

## 验证

- 本机 SDK `8.0.423`；插件目标 `net462`、RenderHarness/测试目标 `net472`。RenderHarness Release 构建 `0 warning / 0 error`。
- `settingspathprobe` 在浅/深主题、`1040×700` 与 `720×640 DIP`、有效目录/172 字符不存在路径下共 `8/8` 通过。它核对真实 ComboBox 选中键与可见标签、完整路径文本和输入框尾部横向滚动、有效/缺失状态说明、状态文本没有被裁切、四个路径动作的位置在路径状态变化时不漂移。短窗中动作自然换行，`BringIntoView` 后浏览/校验/打开/复制都完整位于滚动视口内。
- 探针结束时复核合成 `keep.txt` 与占位 Worker 的内容/时间戳未变、沙箱文件清单未变、长路径目录仍不存在。没有修改用户目录。
- Playnite 定向测试 TRX：`209 total`、`170 passed`、`0 failed`；控制台列出 `39 skipped`（既有测试跳过条件）。覆盖 `SettingsPathValidationTests`、`R16SettingsPathEditorBehaviorTests`、`R16SettingsPathEditorSourceTests`、`SettingsValidationSourceTests`、`WpfUiResourceDictionaryTests` 与 `UiFinesseRound2ControlSourceTests`。
- `scripts/validate-source.py` 通过；XAML 结构校验 `24/24`；WPF 静态审查扫描 36 个 XAML，`0 errors / 29 warnings / 177 info`。
- 完整 `render-qa` 通过，报告中 `0 PROBLEM` 行。Settings 五个分类在 Light/Dark 下覆盖 `1040×700`、`1100×720`、`1366×768`、`2560×1440`。报告标记 `OffscreenRenderHarness`、`DpiScale=1.00`、`WorkingTreeClean=False`；这些不是 Playnite 宿主或物理屏幕呈现结果。

## 证据文件

- [`settings-path-probe-report.txt`](settings-path-probe-report.txt)：8 组实际测量、路径状态、动作几何与沙箱负例。
- [`render-qa-report.txt`](render-qa-report.txt)：当前共享生产资源的全量离屏 Render QA 报告。
- [`Q23-06-focused-tests.trx`](Q23-06-focused-tests.trx)：Playnite 定向测试结果。
- 当前截图：浅色 `1040×700` 有效卡片与长路径错误卡片/字段；深色 `1040×700` 长路径错误卡片；深色 `720×640` 长路径错误卡片/字段。

关键身份 SHA-256：

| 文件 | SHA-256 |
| --- | --- |
| `SettingsPathEditorOption.cs` | `A427924C8845181E65A11A66E4F01F44EC159AFCE3125EED19065C6C21125965` |
| `WpfUiProduction.xaml` | `6D6EE5777D0B50D0DEC21EDF4DB5EE7F1632532992D24B8E2B8F108037181211` |
| `RenderHarness/Program.cs` | `03AB37796995C6D790857CD717C71897BAF382553B315868C192A127BA866945` |
| `Q23-06-focused-tests.trx` | `3635F4A9586EC7D11AAD6447CE624E3CBBFBE277E9829BE2345B2EDA199EF27F` |
| `settings-path-probe-report.txt` | `21AB1030975556CE8D618884C8E271DB97ADF8AF31363674030A3F01AA1E2FF3` |
| `render-qa-report.txt` | `DF12DAFA52BE2CE08BC7A822169E3C5D52FAF0B0C42F8634FF3AAC06F22A37D5` |

## 未验证边界与账本

- 没有在真实 Playnite 内加载 Settings Owner，没有验证 OS UI Automation/Narrator、IME、真实 ACL 拒绝/网络共享、物理 DPI/跨屏或 presented frame。受控离屏结果不能替代这些环境验收。
- 未触发文件夹选择器、Explorer、剪贴板、Open 或 Playnite Save；这些命令的交互语义继续由现有服务/测试覆盖，本轮只验证其布局与可达性。
- Round2 账本仅把 Q23-06 视觉列改为通过；宿主列仍“外部阻塞”，最终仍“未完成”。Round3 `R16-05` 已有的路径服务/命令能力不重复实现或重新计数。
