# U01-05 / U01-06 辅助文字与图标去重、实现和验证

## 去重结论与改动

- U01-05 原有 `GscTypographyCaption` 已是 12 DIP、完整不透明度，并使用动态 `GscMutedTextBrush`；它的浅/深主题实际层叠背景对比已有回归检查。本批补足了共享 WPF-UI 按钮的禁用原因：普通禁用态此前把整块 `ButtonChrome` 降到 0.72 透明，语义禁用文字色只在高对比模式生效。
- 本批把普通与高对比禁用按钮统一为不额外降低整体透明度，使用 `GscDisabledTextBrush`、`GscControlFillBrush` 和 `GscControlStrokeBrush`。运行时禁用文字采用与说明文字相同的可读对比算法；高对比仍解析为系统 `GrayText`。静态设计令牌与 Demo 核心资源同步提高禁用文字可读性。命令、绑定、`IsEnabled`、键盘焦点规则和状态行为未改变。
- U01-06 已由生产 `GscActionIcon` 16 DIP 与 `GscLineIcon` 24 DIP 资源、光学布局/语义和 125/150/175/200% DPI 探针覆盖；本批未另造占位或改动图标系统。

## 验证

- SDK 8.0.423 定向组合测试：26 passed、0 failed、0 skipped；涵盖辅助文字/禁用按钮层叠对比、Light/Dark/跟随主题共享按钮模板状态、高对比恢复、图标语义/对齐及 100–200% DPI。各轮TRX与哈希见 [`tests/tests-manifest.json`](tests/tests-manifest.json)。
- `python scripts/validate-source.py` 通过。同步更新其 WPF-UI守卫，使其断言语义禁用 Brush 与控制底色，而不是旧的 0.72 Chrome 透明度。
- `python .codex/skills/wpf-apple-desktop-ui/scripts/validate_wpf_ui.py .`：31 个 XAML、0 errors、26 warnings、179 info；warning/info 为既有仓库检查项。
- `scripts/render-qa.ps1 -Configuration Release`：RenderHarness 编译 0 warning / 0 error；双主题离屏渲染 `render-qa OK`，报告中 0 个 `PROBLEM`。报告存档于 [`render-qa-report.txt`](render-qa-report.txt)，SHA-256 为 `9F46EE7098FC61540EFF44D64D1D975EDA9FB0C7E3B414A7023ED591D6F4D086`。生成时 HEAD 是 `50e8002d34fcc6775ebe59f306d26c93563d5e97`，工作树含本批尚未提交变更；产品源码随后随实现提交 `813adb09284f3400cd07a758d3eca4d23c34155d` 固化。
- 源码、行为测试、静态审查、渲染及磁盘清理记录汇总于 `verification.json` 和 `cleanup.json`。

## 未验边界

真实 Playnite 宿主、用户实际主题/DPI 与读屏器/UI Automation 未在本批运行；离屏 RenderHarness 不证明这些宿主事实。提交 `813adb09284f3400cd07a758d3eca4d23c34155d` 已推送；Actions push run 仍远端待验：当前GitHub集成的 `fetch_commit_workflow_runs` 只筛PR触发run，返回空；combined status也没有status项；本机无`gh` CLI，公开Actions API端点无法由浏览工具读取。需要用具备Actions读取权限的API/CLI查询该SHA在`windows-build.yml`的SDK8和SDK10 job、run/job/artifact原始身份，再更新进度。中文根入口完整构建/测试/发布/打包/安装身份/启动链与独立 `scripts/build.cmd` 留在整轮最终候选门禁。不触碰真实存档恢复、媒体删除或云端写入。
