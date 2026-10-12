# R04-02 当前 main 错误摘要导航复核

日期：2026-10-12
状态：已满足（插件可控导航与验证行为）；真实 Playnite 宿主和屏幕阅读器朗读仍待验证。

## 当前身份与验证

- 本轮基于 main HEAD 32c0e1e21d1fbfe0b8e46d765b4d2bd3161c46ed 的 Playnite net472 Release 测试程序集复跑。该测试程序集在当前 HEAD 下由前一 R04-01 复核构建；当前源码和测试目录相对完整源码基线 4d166f017dcba8fbb07980ec21b690267b9ff377 未改动。
- SettingsValidationSourceTests、SettingsPathValidationTests、SettingsValidationNavigationBehaviorTests 合计 5 passed / 0 failed / 0 skipped。脱敏 TRX：[SettingsValidation-current-main.trx](tests/SettingsValidation-current-main.trx)，SHA-256：5F6FEFB5D90D8F41AA0B788125A3D46572370954A21A3A5D6127EF458EC07151。
- 使用隔离 SDK 8.0.423 构建当前 net472 测试程序集并运行定向行为类。程序集身份与 [R04-01 当前 main 复核](../R04-01-CURRENT-MAIN-RECHECK-20261012/README.md) 中相同：测试 DLL SHA-256 062626771582C0657630AC573C16B0D9BBD5397FDA81FF27F78422F912CE0E61，MVID e65c3013-b5c0-462f-8e5b-6431911a4a5c；插件 DLL SHA-256 576C173A76880CE3B0C9D8922B64B843BE76A8A12C1EAABB3C3A9A8DC6D5937A，MVID 2ced2b54-667b-47c1-9478-e799309ec9d9。两者 ProductVersion 均为 0.6.73+32c0e1e21d1fbfe0b8e46d765b4d2bd3161c46ed。

## 直接行为证据

- SettingsValidationNavigationBehaviorTests 的 STA Window 装载生产 GameSaveCenterSettingsView 与合成设置/fake Worker 文件，将 SettingsScroller 限制到 180 DIP 后点击“恢复可用性巡检间隔”错误 Hyperlink。实际断言自动化分类选中、链接 Automation Name 含目标原因、目标字段可见并取得键盘焦点、HelpText 含原因、ScrollViewer 的 VerticalOffset 大于 0。
- SettingsPathValidationTests 继续覆盖路径指向普通文件时拒绝、可用父路径下的缺失子目录不自动创建，以及关闭恢复巡检后遗留的非法间隔/有效期不再使 VerifySettings 失败。
- SettingsValidationSourceTests 只作为结构检查补充，检查摘要、Hyperlink、焦点/BringIntoView 和保存校验入口；没有把字符串断言计作焦点、滚动或读屏行为证据。核心导航和禁用字段负例由上面的行为/模型测试直接执行。

## 验收边界

- 已复核的生产设置页仍复用现有 VerifySettings、字段错误模板、页头摘要、SettingsSectionTabs 与 SettingsScroller；错误摘要可定位到可修复字段并把具体原因放进 Automation Name/HelpText。
- 禁用恢复巡检后的负例直接验证 VerifySettings 接受旧无效值，避免此项继续阻止验证；本轮没有执行 Playnite Settings API 的真实保存或 Worker 应用过程，因此不声称持久化/宿主保存闭环已验证。
- 使用合成设置、fake Worker 文件、隔离临时目录和 STA WPF Window，没有读取/修改真实存档、删除真实媒体或写用户云端。真实 Playnite 嵌入、Windows 屏幕阅读器朗读、物理 DPI/跨屏、最终 presented frame、ETW 和宿主性能仍未验。

原始实现说明见 [R04-02-VALIDATION-NAVIGATION-20260917.md](../R04-02-VALIDATION-NAVIGATION-20260917.md)。当前 main 复核没有更改实现或放宽断言。下一项依账本核查 R04-03 未保存离开保护。
