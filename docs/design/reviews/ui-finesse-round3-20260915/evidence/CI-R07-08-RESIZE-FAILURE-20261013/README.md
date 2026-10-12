# R07-08 resize CI failure and current-main recheck

Date: 2026-10-13. The source correction is test-only; no production XAML, view model, command, DTO, or layout logic changed.

## CI runs and archived logs

Complete GitHub Actions job logs were retrieved with the configured repository credential and archived here. Each ZIP hash is SHA-256.

| Run | Commit | SDK from job log | Result and useful finding | Archived log SHA-256 |
| --- | --- | --- | --- | --- |
| [#919](https://github.com/Nikilua/GameSaveCenter/actions/runs/36675309812) | `a6b6dfc9430662e3cc5b7fe00862bce95ac5ebb5` | `10.0.401` | Failure in two sidebar transition tests: expected an active transition, observed none. This is an older source/workflow state. | `3b0b35b0653f1bfb424bbf31a8f987b2670ed94cfdd8b29321154437275c4480` |
| [#920](https://github.com/Nikilua/GameSaveCenter/actions/runs/36677346317) | `77de7f450431d267026ce2169aa30525b18bf528` | `10.0.401` | Same two sidebar transition assertions failed. This confirms the user-designated checkpoint is failed, not still running. Later source commits changed the sidebar tests; current-main SDK 8 results do not replace an SDK 10 run. | `5c387ee6cb853ae351da283c3ec76a27f59ccc5dedac695f37131b0d83864692` |
| [#952](https://github.com/Nikilua/GameSaveCenter/actions/runs/38309632982) | `4572118f3c671981c3d6f93a66820910bc3b94b2` | `8.0.423` | Failed in `R07ResizeStressBehaviorTests.ResizeSequenceKeepsOpenTaskDetailsAndPickerFocusReachable` at the initial effective `IsVisible` assertion. Earlier Core and Worker tests and Release build passed. | `cbda9bd0a0fb1410905e92fa67dd14599b0ecd942e12a026c4ea7a37d3a75737` |
| [#953](https://github.com/Nikilua/GameSaveCenter/actions/runs/38312234186) | `5762c874ef9c1ee2e92692a61c71d17cc9c0f371` | `8.0.423` | Same R07-08 initial `IsVisible` assertion failure. | `23fe03fd5af89daf0f3c0e06f8f0716aba8e77cb3391c971d383288c022caa83` |
| [#954](https://github.com/Nikilua/GameSaveCenter/actions/runs/38313448857) | `a2f8883cae3230506c681cd2ab0e1c8660a9f07f` | `8.0.423` | Same R07-08 initial `IsVisible` assertion failure. The complete log contains only the old generic failure message, so the actual `IsVisible`, ancestor, and geometry values were not recorded. | `cab266ad735e3e145b4a31c4aa50a641f2fc4dd4371eda30af29972a7ae82807` |
| [#955](https://github.com/Nikilua/GameSaveCenter/actions/runs/38317008652) | `55d3df19fb5bd3fabf8582c36c9755580ba6d3a8` | `8.0.423` | Failed before build/test at source validation: `CURRENT_STATE.md` omitted required marker `0.6.73`. The correction commit includes this CI ZIP; R07 tests did not run in #955. | `c022325198c2c1f4245b7462fc0e5111ac7502fd0832dbfa70d1c25f52acd3e3` |
| [#956](https://github.com/Nikilua/GameSaveCenter/actions/runs/38317512412) | `494e7a6e8145541fedeaf616bf7ad5e28ccf896a` | `8.0.423` | Build and earlier test groups passed; R07-08 failed at the initial detail-area check with `visibility=Collapsed`, `isVisible=False`, `actual=0x0`, while TaskView and Window were visible. The test did not yet report grid/view-model selection identity. | `7fd0c14a71f738fc9a415473c26d230c5338e6d065b954b95e3993a53d4d381e` |

The seven archives are `run-<id>-complete-logs.zip` in this directory. The earlier anonymous 403 was an authentication limitation, not a missing log: authenticated retrieval succeeded. The reported failures are test observations; they do not by themselves establish a production UI defect.

## Test correction and local evidence

The prior baseline check depended on effective `UIElement.IsVisible`. #956 showed the detail itself was `Collapsed` with `0×0` layout, so simply replacing that property check was insufficient. The fixture now attaches the production Task page to the shell/window first, then selects and scrolls the synthetic task into view through the loaded DataGrid. It asserts that both `TaskGrid.SelectedItem` and the bound `SelectedTask` reference the same object before checking the detail area's local Visibility and positive layout size. The five wide → narrow → short → wide → restored samples continue to measure detail geometry; picker, focus, nonblank Task grid, MaxHeight budget/restoration, and shell resize propagation checks remain.

This verifies a laid-out area in a controlled STA WPF window. It does not verify an unobscured/presented frame, physical DPI, cross-monitor behavior, or the real Playnite host.

- Local SDK: official .NET SDK `8.0.423`; this is separate from historical SDK `10.0.401` runs #919/#920.
- `scripts/build.ps1 -Configuration Release -SkipTests`: Release solution succeeded with `0` warnings / `0` errors; XAML `24/24`; Playnite target `net462`, tests `net472`.
- After the loaded-grid selection correction, `R07ResizeStressBehaviorTests`: `1/1` passed. TRX: [R07ResizeStressBehaviorTests-sdk8423-current-main.trx](tests/R07ResizeStressBehaviorTests-sdk8423-current-main.trx), SHA-256 `c924eb10916994468833b80f892b156ea2241e312e06bef9cfd07b212b1e1c14`.
- Resize samples recorded detail area `360×476`, `632.667×136`, `632.667×136`, `360×476`, `360×476` DIP respectively. All were locally `Visibility=Visible`; picker remained visible, search retained focus, and shell size-change count was `7`.
- Test source SHA-256: `e71337f5cf3977881a90f30e314d72582115a644ed034f62fdbd8a500fae1810`.
- Built plugin DLL SHA-256: `dda2d4c249f3ffd30af3fe576a76788cccba540343a8cd7267f095cdca3d8d22`; test DLL SHA-256: `f163b0c3e2bff37830c07da4b9cd1c4f703721c524ac45b085f1a0006db9f552`.
- The successful TRX also includes a WPF `TextServicesHost` cleanup `InvalidComObjectException` after the passing test. Its cause is unknown; the process exited successfully.
- `validate-source.py` passed; WPF static review reported `0 errors / 29 warnings / 177 info` across the repository. No unrelated warning was changed.

Run #955 failed source validation because its commit omitted the required `0.6.73` current-facts marker; that marker was restored and source validation passed locally. Run #956 then reached Release build/test with SDK `8.0.423` but exposed the collapsed detail fixture state described above. The selection-after-load correction passes locally and awaits a new Actions run; R07-08 is not yet closed. TestRun cleanup also printed `TextServicesHost.InvalidComObjectException` after the failing assertion in #956; this is cleanup output, not the reported test assertion, and its cause remains unknown. No real Playnite interaction, physical resize, OS UI Automation, or presented-frame verification was performed.
