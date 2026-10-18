# S00-02 命令搜索面板

状态：代码完成待验收。实现提交 `29dc09b28cf04fd6cf02e4a51c89c60c44717cd5`，Demo 对齐修正提交 `a58dd58227f067b350278d625a56f551faa775b4`。

## 实现

- 在 Shell 标题栏加入可见的命令搜索入口。面板使用与游戏选择器相同的 Shell 内 overlay 和浮层卡片样式，不创建脱离宿主窗口的 WPF Popup。
- 空查询显示目录前八项；输入后按中文/英文名称、别名或分类进行不区分大小写的本地过滤。
- 每条结果显示分类、作用范围、可用状态；不可用项保留禁用原因，并通过自动化名称/帮助文本暴露状态。命令仍调用 S00-01 登记的真实 ICommand。
- 结果列表启用虚拟化。可在面板关闭按钮或遮罩关闭；无 IME 组合输入时 Esc 关闭并返回触发器，组合期间 Esc 交给输入法。

## 受控验证

环境为 Windows、本机 .NET SDK `9.0.302`、VSTest `17.14.1`。按提交 `a58dd582` 身份以 Release 构建 `GameSaveCenter.Playnite.Tests.csproj`，产出 Playnite `net462` 与测试 `net472` 程序集；构建成功且无警告。

定向用例覆盖命令过滤/限制、别名和分类匹配、200 项延迟、状态通知、in-host 虚拟化标记、S00-01 命令行为、R10 任务导航/键盘帮助、R21 焦点视觉和自动化名称：`21/21` 通过，0 失败、0 跳过。TRX 中 200 项别名搜索匹配 200 项，耗时 `0.554 ms`。

| 证据 | SHA-256 |
| --- | --- |
| [定向测试 TRX](tests/S00-02-command-search.trx) | `6A11FDCA6F6021F2B1AAE2A4EBEDCE6CF54D533F59A9845D55B747BDC3C8F0C2` |
| Playnite `net462` 程序集 | `BF99CAB774D51734AE02E338301A1913BB7CB272B68499E619EC65CE65502BA5` |
| 测试 `net472` 程序集 | `C856EA8F57A3989CABF0B7EC2B145685CDEE1BDEF99C54EC59B53FE0B98FA6C5` |

`python scripts/validate-source.py .` 与 `git diff --check` 通过。WPF 静态审查扫描 35 个 XAML 文件，0 errors、28 warnings、177 info；Shell 搜索列表对应的提示位于第 373 行附近，列表实际放在独立 Grid 的星号行内，外层 overlay 和卡片均限制最大高度，因此有界。该静态提示不构成布局错误。

## 尚未完成的 UI 门禁

按仓库要求执行了 `GameSaveCenter-Run.cmd`。脚本要求从 `global.json` 选择精确 SDK `8.0.423`；本机 `dotnet --list-sdks` 只有 `9.0.302`，脚本以退出码 1 在 SDK 检查阶段停止，没有完成其真实构建、安装和版本核验。流程输出出现“Playnite 已退出”，但没有验证插件启动、主题呈现或实际版本。没有替代 SDK 检查、没有伪造宿主或屏幕证据。

因此该项仍为“代码完成待验收”，尚未推送到 `origin/main`；远端 UI 门禁通过后再完成推送并准入依赖项 S00-03。受控用例不证明真实 Playnite、浅/深主题、UI Automation/读屏、实际 IME 候选行为或 DPI 呈现。当前 checkout 未包含门禁所指的 `AcrylicFork/Design` 演示源，视觉实现沿用生产 Shell 已共享的浮层、令牌和按钮资源。
