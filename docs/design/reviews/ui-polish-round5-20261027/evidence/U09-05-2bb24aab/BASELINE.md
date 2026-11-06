# U09-05 / U09-06 冻结门禁：2bb24aab

冻结源码为 `2bb24aabede7d98ecbed47c967df24418bcd8312`。独立 `scripts/build.cmd` 与中文根入口使用 SDK 8.0.423、Release；根入口映射为 `GameSaveCenter-一键构建安装运行.cmd` → `GameSaveCenter-Run.cmd` → `scripts/dev-install-run.ps1`。两个入口的外层退出码和各自 166 个诊断步骤均为 0，诊断记录确认源码工作树干净。独立构建于 `2026-11-06T15:39:31Z` 开始并于 `16:04:20Z` 完成；中文根链紧接着于 `16:04:25Z` 开始并于 `16:29:38Z` 完成。

每次运行各有 160 份 TRX、2030 个结果：1972 通过、0 失败、58 跳过。跳过原因按原始结果保留：57 个属于已撤销的“今日工作台”架构断言；1 个是默认 opt-in、会移动系统鼠标指针的测试，不计作通过。320 份原始 TRX 文件及逐文件大小、SHA-256 在 [`trx-manifest.json`](trx-manifest.json)，两次诊断摘要与测试目录在 [`scripts-build/diagnostics`](scripts-build/diagnostics) 与 [`chinese-root/diagnostics`](chinese-root/diagnostics)。独立 build 和中文根入口完整日志在 [`launch-logs`](launch-logs)；入口日志中的本机用户名已替换为 `[user-profile]`，中文根日志仅规范化行末空格；原始日志及脱敏副本哈希在 [`log-redaction-manifest.json`](log-redaction-manifest.json)。逐步骤成功控制台副本留在本机当前审计目录，原始诊断没有删除。

中文根链生成的 `GameSaveCenter-0.6.73.pext` 为 45,739,381 bytes，SHA-256 `2B1BBE74ADDAE42B7A86830C2CA5E12C0F28C32859724725D038442D4198445F`。Playnite 安装清单版本为 0.6.73，DLL 版本为 0.6.73.0；主扩展、Core、Contracts 和 Worker 下对应三个程序集共六个 DLL 的包内 SHA-256 均与安装文件相同，详见 [`verified-local.json`](verified-local.json) 与 [`install-identity.txt`](install-identity.txt)。PEXT 保存在本机最新构建审计目录，不复制进 Git。

构建、安装及 Playnite 启动都位于非活动桌面，启动时输入桌面仍为 `Default`。启动后的 CUA 前台清单只包含 ChatGPT 与 FlClash，没有构建控制台或 Playnite 窗口；这只是检查时点，不证明整个执行期间绝无瞬时闪现。官方 `Playnite.DesktopApp.exe --shutdown` 的关闭调用等待超时，后台宿主没有被强制结束。当前提交没有重新捕获插件页面；04b6457d 的真实宿主审计曾捕获嵌入首页和设置页，0 条高等级记录，详见 [`U09-motion-reentry/host-audit-04b6.json`](../U09-motion-reentry/host-audit-04b6.json)。外部 UIA、实体键盘/IME、多显示器物理 DPI/帧呈现仍未验收。没有执行真实存档恢复、媒体删除或云端写入。

精确提交的 GitHub [run #1479](https://github.com/Nikilua/GameSaveCenter/actions/runs/42244931836) completed/success。baseline job `127984550172` 与 SDK 10.0.401 job `127984549862` 均成功；三个 artifact 的 ID、大小、digest 及源码 SHA 见 [`ci-run-1479.json`](ci-run-1479.json)。required-check/ruleset 读取仍因此前的 403 保持“远端待验”，没有重复探测。
