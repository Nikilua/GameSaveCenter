# U09-05 focused animation test timing fix

The SDK 8 job in prior Windows run #1488 failed because a fixed 180 ms wait observed the compact task-inspector animation before it had settled. The test now waits for bounded observable states and makes the reversal interval deterministic with a test-local one-second motion resource. Production XAML, animation resources, commands, and behavior are unchanged.

With SDK 8.0.423 / Release, the current test source compiled with 0 warnings and 0 errors. The focused WPF behavior test passed 3 independent runs (1/1 each; 0 failures and 0 skips). Each run used its own WinSta0 private desktop and left the input desktop at `Default`. Sanitized TRX copies and hashes are recorded in `verification.json`.

Source Validation, `git diff --check`, and the WPF static review passed (398 XAML, 0 errors, 218 existing warnings, 1440 info). Render QA was not applicable because this is a test-only change.

This patch stabilizes the CI animation assertion; it does **not** identify or resolve the user's black-window report. The attached screenshot file timestamp is 2026-11-06 13:59:27 and carries no HWND/PID. A separate post-test desktop snapshot found no matching visible Playnite/GameSaveCenter window at that instant, but it was not a continuous event trace. U09-05 remains in progress. Full `scripts/build.cmd` tests, the frozen Chinese install/startup chain, and the new commit's SDK8/SDK10 jobs remain pending.