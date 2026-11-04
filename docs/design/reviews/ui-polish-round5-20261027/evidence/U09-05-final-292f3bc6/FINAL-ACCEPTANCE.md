# Round5 U09-05 / U09-08 本机最终验收

## 冻结候选与入口

候选源码为 `292f3bc63a8ab08c57505d50ac5ba68b6c0d431a`，构建时隔离 checkout 的跟踪树干净。SDK 为仓库严格基线 `.NET 8.0.423`，配置为 Release。

- 独立 `scripts/build.cmd`：exit 0，诊断 run `20261104T030507692Z-10c2f177`。
- 中文入口 `GameSaveCenter-一键构建安装运行.cmd` → `GameSaveCenter-Run.cmd` → `scripts/dev-install-run.ps1`：**根入口 exit 0**，安装运行 ID `20261104T060044161Z-375720ee`，构建诊断 run `20261104T060046800Z-ba7cae6d`。入口文件未改动；此次运行覆盖中文包装器实际转交和英文根入口。

首次普通沙箱运行在安装复制阶段被 sandbox 拒绝写入用户扩展目录，退出 1；同一 SHA 按用户已有安装授权重跑后，根入口 exit 0。首次失败日志和诊断仍保存在隔离候选 `artifacts/` 中。独立 build 与最终根入口的测试跳过数不同：前者有 10 项 Named Pipe 用例受环境权限限制，完整授权入口通过了这些用例；没有删断言或扩大 skip。

## 测试

两条入口各有 160 份 TRX，计数与 TRX 报告一致：

| 入口 | 用例总数 | 通过 | 失败/错误/超时/中止 | 跳过 |
| --- | ---: | ---: | ---: | ---: |
| `scripts/build.cmd` | 2,030 | 1,962 | 0 | 68 |
| 中文完整入口 | 2,030 | 1,972 | 0 | 58 |

完整入口的 58 个跳过项均保留且不计通过：57 项属于已撤销“今日工作台”架构断言，1 项是默认不移动系统鼠标指针的 opt-in 用例。Core 157/157、Worker 401/401、Playnite source 575 项（557 通过、18 跳过）、157 个 WPF 隔离组（897 项，857 通过、40 跳过）均无失败。

独立入口的 68 个跳过项为同一组 57 项架构断言、10 项本地 Named Pipe 权限限制和 1 项鼠标 opt-in；完整授权入口中那 10 项通过。全部 TRX、摘要、SDK、源码身份和本机日志保留在隔离候选的 `artifacts/build-diagnostics/20261104T030507692Z-10c2f177/`、`artifacts/build-diagnostics/20261104T060046800Z-ba7cae6d/`。

## 包与安装身份

ZIP 与 PEXT 各 `45,739,194` bytes，SHA-256 均为 `91F4C4FCAB4CF88DC9C3D5E2921AF9A4055A550F36E9B450639B7EDE6D29BD1E`。

Playnite 扩展安装版本为 `0.6.73`，DLL 文件版本 `0.6.73.0`。插件程序集身份为 `0.6.73+292f3bc63a8ab08c57505d50ac5ba68b6c0d431a`、SHA-256 `D066E97586A4FA55B4851127ACB70A3E5C001CD011653188D6AFF47BE03D80E3`；Worker 程序集身份相同、SHA-256 `C8A5C7B86D0CAA619BC1361A72DA9D1B93C39BFFD26225A5F154C699B517CA1B`。安装报告来自本机 `artifacts/last-dev-install.txt`。

Playnite `10.56.0.23531` 进程 PID `50956` 从 `D:/software/Playnite/Playnite.DesktopApp.exe` 启动并保持响应。Playnite 的扩展日志记录 `0.6.73.0` 从该安装目录加载，观察到 3 个 Playnite 游戏。窗口标题 `Playnite` 已在非活动隐藏桌面上枚举到；没有切换桌面或捕获实机截图，因此不宣称实际插件页面已人工视觉验收。该进程仍运行；收尾辅助脚本的 WM_CLOSE 未能使其退出，未强杀，也没有启动第二个实例。此结果不改变中文根入口本身的 exit 0。

## CI、范围与未验边界

精确源码 push run [#1454](https://github.com/Nikilua/GameSaveCenter/actions/runs/41742480073) 已 completed/success：SDK 8.0.423 job `126287170658` 与 SDK 10.0.401 job `126287170119` 均成功。required-check / ruleset 名单此前读取为 403，按规则不重复试探，U09-06 仍为“远端待验”。

U09-05 标准入口验收完成；U09-08 整轮状态复核完成。80 项状态为 **7 项已满足、71 项代码完成待验收、U09-04 进行中、U09-06 远端待验**。U09-04 仍缺 U00 同条件真实帧时/分配数据；本次没有检查插件页面截图、外部 UIA、实体键盘/IME、物理 DPI 或跨屏表现。真实存档恢复、媒体删除与云端写入均未执行。包含截图对应选框修复的 U06-02 / U08-08 仍是“代码完成待验收”，不以构建成功替代宿主视觉确认。

## 用户盘符请求

已核实并解除 `R:` 的 SUBST 映射：它只指向 C: 下的 Codex 可视化目录。映射解除后原目录仍存在，未删除目录或文件；C:、D: 盘符映射未改动，`subst` 当前无映射。
