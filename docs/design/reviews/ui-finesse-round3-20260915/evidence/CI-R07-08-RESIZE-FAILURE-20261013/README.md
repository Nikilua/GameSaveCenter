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

The five archives are `run-<id>-complete-logs.zip` in this directory. The earlier anonymous 403 was an authentication limitation, not a missing log: authenticated retrieval succeeded. The reported failing assertion is the `IsVisible` test predicate; why it was false on those runners is still unknown and is not asserted as a production UI defect.

## Test correction and local evidence

The prior baseline check depended on effective `UIElement.IsVisible`. The revised fixture checks the detail ScrollViewer's local `Visibility` and positive arranged width/height, then records positive detail geometry at every wide → narrow → short → wide → restored sample. Existing checks remain for picker visibility, search focus, nonblank Task grid, finite compact MaxHeight, restoration of wide MaxHeight, and resize propagation through the production shell.

This verifies a laid-out area in a controlled STA WPF window. It does not verify an unobscured/presented frame, physical DPI, cross-monitor behavior, or the real Playnite host.

- Local SDK: official .NET SDK `8.0.423`; this is separate from historical SDK `10.0.401` runs #919/#920.
- `scripts/build.ps1 -Configuration Release -SkipTests`: Release solution succeeded with `0` warnings / `0` errors; XAML `24/24`; Playnite target `net462`, tests `net472`.
- `R07ResizeStressBehaviorTests`: `1/1` passed. TRX: [R07ResizeStressBehaviorTests-sdk8423-current-main.trx](tests/R07ResizeStressBehaviorTests-sdk8423-current-main.trx), SHA-256 `39ee7ed99ef1e4b24148b7f596bfcafc3ee4262a80f940e270bbea2c1c9329f5`.
- Resize samples recorded detail area `360×476`, `632.667×136`, `632.667×136`, `360×476`, `360×476` DIP respectively. All were locally `Visibility=Visible`; picker remained visible, search retained focus, and shell size-change count was `7`.
- Test source SHA-256: `7dfbbfbbcd1d949dbcea302fbd1fa650178cf0e430b7d52804535194958eb1d1`.
- The successful TRX also includes a WPF `TextServicesHost` cleanup `InvalidComObjectException` after the passing test. Its cause is unknown; the process exited successfully.
- `validate-source.py` passed; WPF static review reported `0 errors / 29 warnings / 177 info` across the repository. No unrelated warning was changed.

The R07 correction was pushed as `55d3df19`. Its run #955 ended in failure before building or testing because this document update had removed the required `0.6.73` current-facts marker. The marker is restored in the working tree and source validation is being rerun; the next Actions run must reach build/test before R07 can be evaluated remotely. No real Playnite interaction, physical resize, OS UI Automation, or presented-frame verification was performed.
