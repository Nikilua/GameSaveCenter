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
| [#957](https://github.com/Nikilua/GameSaveCenter/actions/runs/38319864960) | `063550b60bd1855c712ea316469497b1713ad832` | `8.0.423` | Release build and earlier suites passed; R07 still observed `Collapsed/0×0` after grid and bound `SelectedTask` both identified `r07-resize-task`. The complete log does not report a failed production command; it shows the fixture had not opened the compact details drawer. | `36491ec7689e9483521124edb4af470d4255e771e40893fc8ac8ee2a5bc22576` |

The eight archives are `run-<id>-complete-logs.zip` in this directory. The earlier anonymous 403 was an authentication limitation, not a missing log: authenticated retrieval succeeded. The reported failures are test observations; they do not by themselves establish a production UI defect.

## Test correction and local evidence

The earlier failure was not solved by only replacing `IsVisible` or moving selection after load. #957 confirmed the selected grid object and bound `SelectedTask` matched while the detail stayed `Collapsed/0×0`. Production `OnTaskSelectionChanged` resets `taskInspectorOpen`; at a compact breakpoint the drawer remains closed until its “查看任务详情” button is invoked. The fixture now starts with no selected task, selects through the loaded production DataGrid, checks the closed drawer/open-button state, routes a Click through the production compact-details button, and checks the drawer plus close-button state before resizing. The five wide → narrow → short → wide → restored samples still measure detail geometry; picker, focus, nonblank Task grid, MaxHeight budget/restoration, and shell resize propagation checks remain.

This verifies a laid-out area in a controlled STA WPF window. It does not verify an unobscured/presented frame, physical DPI, cross-monitor behavior, or the real Playnite host.

- Local SDK: official .NET SDK `8.0.423`; this is separate from historical SDK `10.0.401` runs #919/#920.
- `scripts/build.ps1 -Configuration Release -SkipTests`: Release solution succeeded with `0` warnings / `0` errors; XAML `24/24`; Playnite target `net462`, tests `net472`.
- After the production compact-drawer interaction correction, `R07ResizeStressBehaviorTests`: `1/1` passed. TRX: [R07ResizeStressBehaviorTests-sdk8423-current-main.trx](tests/R07ResizeStressBehaviorTests-sdk8423-current-main.trx), SHA-256 `951dfbae93f8f3f9ff86a69653407afb2b34034a71326174f3ddd97a586296448`.
- Actual local fixture output confirms `compactDetailsAction=clicked`, the open button collapsed, the close button visible, and the detail area visible at `632.667×136` DIP before the resize sequence.
- Resize samples recorded detail area `360×476`, `632.667×136`, `632.667×136`, `360×476`, `360×476` DIP respectively. All were locally `Visibility=Visible`; picker remained visible, search retained focus, and shell size-change count was `7`.
- Test source SHA-256: `24e04d8cb7ac9c070d3c9d8910433afcff514689c66badcf9c464c87edf60852`.
- Built plugin DLL SHA-256: `09847087f2acd067d7810907d597eb1b1ee08bfa68998ff4a9479b8d49dcb7fa`; test DLL SHA-256: `b876988ad9f31614b3b5685bc94c197b5a9becaa56b18d869c6329f38d5650dd`.
- The successful TRX also includes a WPF `TextServicesHost` cleanup `InvalidComObjectException` after the passing test. Its cause is unknown; the process exited successfully.
- `validate-source.py` passed; WPF static review reported `0 errors / 29 warnings / 177 info` across the repository. No unrelated warning was changed.

Run #955 failed source validation because its commit omitted the required `0.6.73` current-facts marker; the marker was restored and source validation passed locally. Runs #956/#957 then reached Release build/test with SDK `8.0.423`; both exposed a compact drawer that the fixture had not opened. The corrected fixture passes locally, but the latest test change has not yet run in Actions; R07-08 remains open pending that result. `TextServicesHost.InvalidComObjectException` cleanup output appeared after both passing and failing tests; its cause remains unknown and it is not the reported R07 assertion. No real Playnite interaction, physical resize, OS UI Automation, or presented-frame verification was performed.
