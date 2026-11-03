# U09-05 / U09-08 冻结候选门禁：de6f7f82

## 源码身份与本地完整链

冻结提交：`de6f7f82038c59c5fbdbb9b366c3e280354ffc5f`，tree `6377a8a71c2ca5a0a07533190b43d11a451c01c7`；`src/`、`tests/`、`scripts/`树分别为 `b5d4721c8df7acf643ccbea1d8597cffa1bba163`、`fd800e3c82f5f8a27bd15547f613cf22fb277768`、`efbde3774901e75ad253a2d77cd90f83796e2931`。构建开始时工作树干净。实际中文入口 `GameSaveCenter-一键构建安装运行.cmd` 转交 `GameSaveCenter-Run.cmd`，后者执行 `scripts/dev-install-run.ps1 -Configuration Release`；未创建用户口述的其他入口。

在该精确候选上运行中文入口完整 Release 编译、测试、Worker 发布、打包、安装身份核验及 Playnite 启动链，最终退出码为 0。SDK `8.0.423`、MSBuild `17.11.48`，8个项目编译，XAML结构检查26个文件，0 warning、0 error。158份TRX共2026项：1968通过、0失败、0 errors/timeouts/aborts、58 skip，155个WPF隔离类。58项为既有57条撤销的“今日工作台”架构断言及1条默认关闭的真实系统指针用例；skip不计通过且未扩大。Core 157/157、Worker 401/401，Playnite WPF隔离组853/893通过（40 skip）。完整摘要SHA-256为 `AA568BA46900DE559F63A8B72E7EF9723DB9DCBD6F4B63160B084A40112A1C11`，诊断目录 `artifacts/build-diagnostics/20261102T234431737Z-92dfb8a2/`。

独立入口 `scripts/build.cmd` 按其真实编译/测试职责在提交 `eebef00978265119e5ed54d96d0053aba4ea147c` 运行并退出0；该候选与 de6 的 `src/`、`tests/`、`scripts/` 树相同，差异仅为提交后的文档。它记录同样2026项、1968通过、0失败、58 skip、155个WPF隔离类，不能代替中文完整安装启动链。较长临时根首次运行的Worker压力用例曾在第17周期原子写入失败，原因未确认；改用短根后在同SDK、同源码和断言下复跑成功，没有修改源码、断言或skip。两次诊断/TRX均保留，见 `independent-buildcmd.json`。

## 包和已安装身份

本地ZIP与PEXT各45,737,961 bytes，内容相同，SHA-256 `4C03AF7D78FCA82BD23D27D37934B44BEFF1598EAC57EF5CC57A942C19A7E243`；安装清单及插件版本均为 `0.6.73`。安装目录中6个插件/Worker DLL 的 `AssemblyInformationalVersion` 均为 `0.6.73+de6f7f82038c59c5fbdbb9b366c3e280354ffc5f`，逐文件SHA与安装报告、包暂存目录相符。安装报告SHA-256 `089143FDC70117857FA241C68B8F264CFF8BD8845511A426D0DD3C446944CF6D`。Playnite进程启动，日志确认加载 `GameSaveCenter` 0.6.73；这只证明插件版本已加载，不代表看过插件页面。

最近活动与共享/兼容选择装饰保留蓝色圆角框、焦点圈和选中色，同时让装饰层不参与行内容测量；R10最近活动/共享列表三态几何类3/3通过、R23生产资源状态类12/12通过，均0 skip。图标及行容器在选中时不应被推向右下或裁切。测试TRX哈希、安装文件身份及本地细节见 `root-gates.json`。

## 精确提交 GitHub Actions

run [41547554497（#1441）](https://github.com/Nikilua/GameSaveCenter/actions/runs/41547554497) 由 `windows-build.yml` 对精确提交 `de6f7f82…` 的 push 触发，2026-11-03 00:48:41 UTC completed/success。适用的两个job及全部步骤成功：SDK基线 job `125626741394`（含编译/测试、Render QA、WPF静态审查、Playnite扩展包与artifact上传，15步）；SDK `10.0.401` job `125626741153`（编译/测试复核，8步）。预览包 artifact `13194211820` digest `sha256:15d7b9fe96e8b367797f8f0c11d4c5cf136a5652053ae0d60c6b62c17f13d282`；基线诊断 `13194191830` digest `sha256:69765a85d9d530e3e8cc7efb3f02babf0b55eb03494f5fa78902c20205b7a26c`；SDK10诊断 `13192160788` digest `sha256:1c91bdb7526809a821dee931517ab42d1685bc93a45fbb5fc8c47d9f52cecc16`。Artifact大小、到期时间与提交绑定见 [`ci-run-41547554497.json`](ci-run-41547554497.json)。required-check/规则集读取此前返回403；本轮未重试同一拒绝。U09-06保留“远端待验”，下一步由有读取权限的维护者核对required-check清单。

## 仍待人工/真实宿主验收

曾启动Playnite并看到插件版本加载，但计算机控制运行时初始化因缺失路径错误连续失败两次，没有向UI发送输入，也没有观察真实插件页面。因此不声称页面截图、Playnite视觉、外部UIA、实体键盘/IME、物理DPI/跨屏已验；这些相关80项仍为“代码完成待验收”。U09-04的受控TaskGrid滚动p95比较没有显示回退，但U00原始逐帧/累计分配及真实宿主呈现数据仍缺，保持进行中。真实存档恢复、媒体删除和云端写入不在本轮范围。
