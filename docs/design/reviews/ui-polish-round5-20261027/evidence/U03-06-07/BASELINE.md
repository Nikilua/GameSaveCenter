# U03-06/07 下拉、勾选与开关状态

## 去重和增量

源码提交：`5300a2b5c70f7736f1f46617667de73749b8d883`（`补充下拉与勾选控件回归`）。

- 复核生产 `GscWpfUiComboBoxTemplate`：选中内容和20 DIP箭头列分开；普通显示文本有省略号及完整文本提示；Popup使用现有最大下拉高度、自动竖向滚动和方向键内部导航。没有重写通用控件模板，也没有替换当前游戏选框。
- 复核生产 `GscCheckBox`：已显示勾选和不确定标记，长标签可换行；复用统一焦点样式，整个标签/间隔与框体落在同一个可激活控件上；Space及禁用语义保留。
- 本批只有一个生产XAML源码守护测试增量，防止 ComboBox 长选中项省略/提示和箭头列分离回退。U03-07 所需状态、双主题命中区和键盘行为已经由现有测试覆盖，不再复制生产样式或改变保存时机。
- 保持 DropDownClosed 提交契约及所有已有命令、Binding、安全语义不变；不改变游戏选框、滚动条、列表虚拟化或页面架构。

## 验证

使用本地 SDK `8.0.423`、Release；精确提交 `5300a2b5c70f7736f1f46617667de73749b8d883` 定向测试 `13/13` 通过、0 skip、0 fail：

| 验证 | 通过数 |
| --- | ---: |
| `AccessibilitySourceTests`（含新增断言） | 6/6 |
| `R05PopupBoundaryBehaviorTests`（通用下拉边缘与滚动视口） | 2/2 |
| `Q10CheckBoxHitBehaviorTests`（Light/Dark长标签命中、Space、禁用） | 4/4 |
| `ComboPopupClosesOutsideAndKeepsItsReadingSurfaceBounded` | 1/1 |

TRX及SHA-256见 [`tests-manifest.json`](tests-manifest.json)。源码检查通过；XAML结构检查26/26；WPF静态审查扫描37个XAML，0 error、26 warning、179 info。此提交只改C#测试，不改生产视图或资源；因此没有为相同生产画面重复生成Render QA。对应生产资源的 d1fc 精确SHA双主题 Render QA exit0 / 394 PNG / 0 `PROBLEM` 见 [U03-04/05报告](../U03-04-05/BASELINE.md)。

## 远端与未验边界

GitHub workflow `.github/workflows/windows-build.yml` 对精确提交 `5300a2b5c70f7736f1f46617667de73749b8d883` 的 run `40567683673`（#1328）已完成：baseline job `122413211080` 的15步与SDK10.0.401 job `122413210438` 的8步全部成功。扩展包 artifact `12768025071`（90,778,190 bytes，SHA-256 `2c5705e41535b272f01f11e42722c1d93fe9097ade808a31fb7be3731481bb8f`）；baseline诊断 `12767114031`（1,970,030 bytes，SHA-256 `7abaa0e3f460db384e6061e324a9107a5582f3e6a9e058cb5ac5b467c8b617f9`）；SDK10诊断 `12764309162`（943,542 bytes，SHA-256 `02cc2417f1a6d58d0a1f2542bbdc2aae20df750d121b7991ea6de1088a95a0f6`）。完整job/artifact身份见 [`ci-run-40567683673.json`](ci-run-40567683673.json) 与 [`CI清单`](../U02-05-06/ci-runs.json)。required-checks接口此前HTTP 403，分支保护状态仍“远端待验”。

行为测试通过不等同于真实Playnite宿主、物理输入、Narrator/真实UIA客户端或实体DPI验收。没有触发保存命令、真实存档恢复、媒体删除或云端写入。

## 本轮复核输出与磁盘快照

2026-11-09 20:32 +08:00 只读观测：C盘空闲 `3,200,520,192` bytes，D盘空闲 `20,197,179,392` bytes。本轮没有本地build、行为测试或Render QA；Source Validation的隐藏进程日志只创建于仓库 `.tmp/round5-u03-06-07-doc-check-20261109/`（2文件/361 bytes），已核对边界、无重解析点/引用/进程占用后用PowerShell `Remove-Item -LiteralPath -Recurse -Force`删除。清理时C盘前后 `3,197,964,288` bytes、D盘前后 `20,197,036,032` bytes，未释放有效空间。真实宿主/UIA/鼠标/DPI边界保持未验。
