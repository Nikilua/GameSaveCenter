## WPF / XAML UI changes

For any task involving WPF, XAML, Playnite UI, theming, layout, controls, animation, dialog, toast, DataGrid, ScrollBar, TabControl, responsive sizing, DPI, accessibility, or visual regression:

1. Use the `wpf-apple-desktop-ui` skill.
2. Inspect the real repository and existing shared resources before editing.
3. If present, read:
   - `docs/design/APPLE_WPF_IMPLEMENTATION_PROMPT.md`
   - `docs/design/UI_CHANGE_GATE.md`
4. Fix shared styles/templates globally instead of patching a single screenshot.
5. Preserve commands, bindings, business behavior, virtualization, keyboard access, UI Automation, and Playnite compatibility.
6. Validate all similar controls and all theme modes.
7. Never claim Windows/Playnite rendering was verified unless it was actually run.
