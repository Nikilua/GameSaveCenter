# U09-05 hidden WPF test verification — 2026-11-07

Source commit: 0feff18501a5ec971e3d7fb2ba212e8d0d28e0ac (隐藏WPF测试窗口防止构建闪屏).

The WPF test runner now places each WPF test class on a private, non-input desktop in WinSta0. The focused Q10 animation test passed 1/1 with the input desktop remaining Default. The independent scripts/build.cmd full run exited 0: SDK 8.0.423 / Release, 2030 cases, 1962 passed, 0 failed, 68 existing skips, and all 157 WPF test classes isolated. Its continuous Default desktop window monitor recorded zero show events.

The Chinese root entry completed compilation, tests, packaging, installation identity checks, and Playnite startup. Its log reaches the explicit [OK] success branch; the branch exits 0 and the Chinese wrapper forwards that code. The diagnostic summary is passed (2030 / 1972 passed / 58 skipped / 0 failed). ZIP and PEXT hashes match; all six installed assembly identities include the exact source SHA and their hashes are recorded in local-results.json.

The outer PowerShell log-capture wrapper did not write its post-command exit record because the started Playnite/Worker retained redirected handles. The official --shutdown request timed out; the process was not force-killed. The input desktop monitor was stopped after the root output and startup completed and had recorded zero show events. This harness issue is not being reported as a successful wrapper exit observation.

GitHub windows-build.yml run 42326547974 (#1486), tied to the exact source SHA, completed successfully for both SDK 8.0.423 and SDK 10.0.401. Job and artifact IDs/digests are in local-results.json.

The user's screenshot contains no HWND/PID. These results show that this exact controlled test/build path produced no Default desktop show events; they do not identify the older screenshot window or claim universal absence of a flash in every manual invocation. Real Playnite page rendering, external UIA, physical keyboard/IME, and multi-monitor DPI remain unverified. No real save restore, media deletion, or cloud write was performed.
