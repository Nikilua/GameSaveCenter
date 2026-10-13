# Q23-08 底部动作与设置迁移：当前 main 复核（2026-10-13）

## 结论

Q23-08 要求统一既有保存、取消、恢复默认、导入和导出入口，并保证短窗可达且危险/迁移操作保留确认。已复核现有能力并补上当前行为与双主题短窗证据。Playnite 拥有 Settings 保存/取消按钮，插件 `UserControl` 不重复绘制或接管它们；恢复默认、导入/导出沿用现有目录、预览 DTO 和命令流程。

Round2 台账：视觉条件 **通过**，宿主 **外部阻塞**，最终 **未完成**。

## 复用能力与本轮改动

- 恢复单字段、单分类和全部默认都继续使用 `SettingsResetCatalog`；现有 Yes/No 警告确认保留。恢复只改当前设置草稿，路径、云端目标和设备身份继续受保护，Playnite Cancel 可撤销草稿。
- 导入继续先生成兼容性预览，再显示将覆盖字段并请求确认，只有 Yes 才调用 `ApplyPortableJson`。导出/导入仍用原生文件选择框与现有错误报告流程。
- XAML 仅为既有“恢复全部默认”按钮增加测试定位名。代码隐藏把现有重置/导入确认统一到同一个提示方法；生产时仍调用相同的拥有者窗口、Yes/No、Warning `MessageBox`。内部测试 delegate 只用于驱动真实确认分支，避免自动测试打开原生模态框。
- 没有新增保存/取消按钮、重写设置 DTO/传输服务或改变确认标题与业务顺序。

## 本机行为与构建

源码提交哈希及最终测试身份由本阶段证据同步记录。本机使用隔离 SDK `8.0.423`（`D:\GameSaveCenterTools\dotnet-sdk-8.0.423\dotnet.exe`），VSTest `17.11.1`；Playnite 插件目标 `net462`，Playnite 测试目标 `net472`。测试/程序集构建绑定当前仓库身份；不替代 CI SDK10 job。

- Playnite 测试项目 Release build：`0 warning / 0 error`。
- `Q23SettingsActionConfirmationBehaviorTests`：`1/1`。通过实际 Reset All 按钮 Click 和生产确认方法，验证 No 后 fingerprint/编辑值不变；Yes 只重置安全字段；工具/存档/媒体路径与云端目标保留；随后 `CancelEdit` 恢复原草稿。导入确认 No 保持 fingerprint 不变，Yes 返回确认通过；正向 payload 语义仍由现有 `ApplyPortableJson` 行为覆盖。原生导入文件对话框未被调用。
- 关联筛选（确认行为、`R16SettingsDefaults`、`R16SettingsImportPreview`、`SettingsValidationSourceTests`）：`10/10`，`0` failed、`0` skipped。TRX：[确认/重置/导入回归](tests/Q23-08-related-regression.trx)、[独立取消回归](tests/Q23-08-cancel-regression.trx)、[单项确认行为](tests/Q23SettingsActionConfirmationBehaviorTests.trx)。
- `SettingsDraftLifecycleBehaviorTests` 独立进程：`1/1`。
- `scripts/check-xaml.ps1`：24 个 XAML 文件通过；`py -3 scripts/validate-source.py` 通过；`python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .` 报 `0 errors / 29 warnings / 177 info`；`git diff --check` 通过。
- 一次提交后重跑曾使用错误的 `GSC_BUILD_COMMIT` 值，源码身份门禁 fail-closed 阻止 3 个源码测试，另外 7 个测试通过。按实际 HEAD 重建后，当前关联 TRX 为 `10/10`；这次拦截是命令身份输入错误，不是产品测试失败，失败 TRX 已由当前成功 TRX 替换。
- 当前 Playnite 测试程序集 SHA-256：`255B3B610CDB4F591219DB5DAB2CA9BAC67D13716DCC21811F9B5B612702DE6D`；Playnite 插件程序集 SHA-256：`6998451E88AD86AFDC473D3D09250068B5DB1E056DFF7945FDC0211D0E67BD81`。

源文件 SHA-256：XAML `GameSaveCenterSettingsView.xaml` `DADBA15AE9D1FD426F36FA839F4A90AA53FC5ADBF213238932046B61F7D60556`；view code-behind `GameSaveCenterSettingsView.xaml.cs` `D0944FE1A76BF33796FE308FA72322836916FC4A650B6055EA4A857FE37CE28C`；行为测试 `Q23SettingsActionConfirmationBehaviorTests.cs` `26B6875925C3011093FFF825585C6ED2ADEE8FFD172B39A811CA1B0A13AB1A80`；RenderHarness `Program.cs` `6E109F4541FCF55C9D11CBFCCFA4AD731297F4E4ED091707477F4BFBAFAB9F27`。

## 当前身份离屏视觉证据

RenderHarness 使用合成设置。为让既有路径验证保持有效，探针在证据目录下短暂创建一个空 Worker 标记文件，验证只检查文件是否存在、不启动它，截图完成后删除；设置的 Saves/Media 只使用合成的缺失子路径，不创建目录。路径根通过进程级环境变量映射到隔离目录，没有使用用户配置或真实存档/媒体路径。Light/Dark × `1040×700`/`720×640 DIP` 四种场景均通过。运行报告：[settings-actions-probe-report.txt](settings-actions-probe-report.txt)。每种场景有重置默认折叠、展开和迁移页三张图：

| 主题/尺寸 | 默认折叠 | 展开重置 | 迁移页 |
| --- | --- | --- | --- |
| Light `1040×700 DIP` | [图](settings-actions-light-1040x700-reset-collapsed.png) | [图](settings-actions-light-1040x700-reset-expanded.png) | [图](settings-actions-light-1040x700-migration.png) |
| Light `720×640 DIP` | [图](settings-actions-light-720x640-reset-collapsed.png) | [图](settings-actions-light-720x640-reset-expanded.png) | [图](settings-actions-light-720x640-migration.png) |
| Dark `1040×700 DIP` | [图](settings-actions-dark-1040x700-reset-collapsed.png) | [图](settings-actions-dark-1040x700-reset-expanded.png) | [图](settings-actions-dark-1040x700-migration.png) |
| Dark `720×640 DIP` | [图](settings-actions-dark-720x640-reset-collapsed.png) | [图](settings-actions-dark-720x640-reset-expanded.png) | [图](settings-actions-dark-720x640-migration.png) |

探针实际检查短窗初始折叠；展开后恢复全部、单字段选择与动作可见；四个设置分类中的“恢复本类默认”动作经页面滚动后完整落在 viewport；迁移页导入和导出动作均可滚动到并完整显示。离屏报告记录迁移 viewport：宽场景 `751.33×412.67 DIP`，窄场景 `431.33×352.67 DIP`。视觉探针没有点击任何重置/导入/导出/保存/取消按钮，也没有打开文件对话框。

当前 evidence SHA-256：

| 文件 | SHA-256 |
| --- | --- |
| `settings-actions-probe-report.txt` | `EC13589FA7D720F6A51FC0DB19A8743792028D59A689FC9F0428D5B24C1AFB08` |
| `settings-actions-dark-1040x700-migration.png` | `365A329C9623F3C55E8250794235D9BF05852668D6A2F4ABF3587C1F695D97D2` |
| `settings-actions-dark-1040x700-reset-collapsed.png` | `0CE87A0D92C04C6B47131A66A44716A3FB9A9A60973D7158166D5F30E5481374` |
| `settings-actions-dark-1040x700-reset-expanded.png` | `48F2319B4A4DD8FF8A8952BC874C41D18483AD1E92F07369199A9A83678772AF` |
| `settings-actions-dark-720x640-migration.png` | `D5E885748A61B9A4FC18C24D7EDE4BD227A4C68FA97780088F12FD283074ED50` |
| `settings-actions-dark-720x640-reset-collapsed.png` | `DCBC764349D42DEABBDC59B9CD774C22D17C489E69CC6F244E3CF2231EA67908` |
| `settings-actions-dark-720x640-reset-expanded.png` | `171C83BBB71C85534EBAC54F07B288F59133FB535275451492195A002EC212B2` |
| `settings-actions-light-1040x700-migration.png` | `84CF4424191DCCED7DBD656200DE45FBAE6340DE96BE4FFEB298016E73EAD67E` |
| `settings-actions-light-1040x700-reset-collapsed.png` | `8C7C0D3AC4AB64E1289F61D0441232C57969341E31C95823CA336C7415E43205` |
| `settings-actions-light-1040x700-reset-expanded.png` | `F7B7D31EDA6A13EC2F1089021DED34EB23761E8FFC37D15FBA99256311AB8321` |
| `settings-actions-light-720x640-migration.png` | `57ACD85B13002ECE8BDB0B5B7EE7CFEA7FFC9957E8EF2B18E14F98B09EF47E23` |
| `settings-actions-light-720x640-reset-collapsed.png` | `E46DBDBDF95DAE6AEDE2E3126DB7AB85663A2ACC9B9A9B08B6D2B3725CD2A0AD` |
| `settings-actions-light-720x640-reset-expanded.png` | `BD88C8DE2821E48F1BE74E3C0276D213AA1AACC32B8CDB5ECF36A9A4968B77B5` |
| `tests/Q23-08-cancel-regression.trx` | 更新代码身份复核后补录 |
| `tests/Q23-08-related-regression.trx` | 更新代码身份复核后补录 |
| `tests/Q23SettingsActionConfirmationBehaviorTests.trx` | 更新代码身份复核后补录 |

## 未验证边界与 CI 状态

- 没有运行真实 Playnite Settings Owner；宿主底部 Save/Cancel 的真实呈现与提交/回滚序列尚未现场复核。
- 没有实际打开原生文件选择框、读写导入/导出文件、调用剪贴板或操作用户配置；合成窗口与逻辑 DIP 不代表物理 DPI、OS UIA/IME 或最终显示帧。
- Demo 原始目录在本机不可用，本轮沿用恢复后的生产基线；没有据此另换设计体系。
- 用户提供的 `GameSaveCenter-sdk10-build-diagnostics.zip` 对应 #967 的 SDK10 job：SDK `10.0.401`，`summary.json` outcome `passed`，构建与测试步骤均 exit 0，117 个 TRX 为 `1566 passed / 57 NotExecuted / 0 failed`。#967 workflow 红叉来自独立 SDK8 job 的源码结构检查失败；这个 ZIP 不包含该 SDK8 job 日志。用户说明手头目前只到 #967。之后 #970、#971 的失败步骤只有 exit code 1，匿名日志/API 不可访问（API 403，job 页面要求登录），也没有可用 artifact；未取得完整日志前不推测根因或修复。当前 Q23-08 代码推送后，仍须检查新 Actions 至明确终态，并分别记录 SDK8 与 SDK10 job。

真实宿主和系统级证据未取得前，Q23-08 保持最终“未完成”。
