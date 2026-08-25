using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Effects;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Derives a readable local palette from the active Playnite theme. Playnite requires
    /// TextBrush/TextBrushDark, but community themes can expose very different background brushes,
    /// so GameSaveCenter validates contrast instead of assuming a simple light/dark pair.
    /// </summary>
    internal sealed class AdaptiveThemePalette
    {
        public bool IsDark { get; set; }
        public Color Background { get; set; }
        public Color PrimaryText { get; set; }
        public Color SecondaryText { get; set; }
        public Color MutedText { get; set; }
        public Color DisabledText { get; set; }
        public Color ControlFill { get; set; }
        public Color ControlStroke { get; set; }
        public Color Divider { get; set; }
        public Color SurfaceTop { get; set; }
        public Color SurfaceBottom { get; set; }
        public Color StrongSurfaceTop { get; set; }
        public Color StrongSurfaceBottom { get; set; }
        public Color SidebarTop { get; set; }
        public Color SidebarBottom { get; set; }
        public Color Backdrop { get; set; }
        public Color Highlight { get; set; }
        public Color Accent { get; set; }
        public Color AccentHover { get; set; }
        public Color AccentPressed { get; set; }
        public Color AccentTint { get; set; }
        public Color AccentTintStrong { get; set; }
        public Color AccentIconFill { get; set; }
        public Color OnAccentText { get; set; }
        public Color Info { get; set; }
        public Color Success { get; set; }
        public Color Warning { get; set; }
        public Color Error { get; set; }
        public Color InfoIconFill { get; set; }
        public Color SuccessIconFill { get; set; }
        public Color WarningIconFill { get; set; }
        public Color ErrorIconFill { get; set; }
        public double GlassStrength { get; set; }
    }

    internal static class AdaptiveThemePaletteFactory
    {
        private static readonly string[] BackgroundResourceKeys =
        {
            // Playnite Desktop's built-in theme uses this historical spelling. It is the
            // resource applied by MainWindowStyle/StandardWindowStyle and is the reliable
            // source when a plugin settings view is hosted in a separate dialog window.
            "WindowBackgourndBrush",
            "WindowBackgroundBrush",
            "MainWindowBackgroundBrush",
            "ControlBackgroundBrush",
            "BackgroundBrush"
        };

        private static readonly string[] AccentResourceKeys =
        {
            "HighlightGlyphBrush",
            "AccentBrush",
            "HoverBrush"
        };

        public static AdaptiveThemePalette Create(FrameworkElement host, bool glassEnabled, int strengthPercent, GameSaveCenterThemeMode themeMode = GameSaveCenterThemeMode.FollowPlaynite)
        {
            var forcedLight = themeMode == GameSaveCenterThemeMode.Light;
            var forcedDark = themeMode == GameSaveCenterThemeMode.Dark;
            var highContrast = SystemParameters.HighContrast;
            var hostText = highContrast ? null : ResolveResourceColor(host, "TextBrush");
            var hostInverseText = highContrast ? null : ResolveResourceColor(host, "TextBrushDark");
            // The UserControl itself is initially backed by GameSaveCenter's dark fallback
            // brush. Resolve Playnite's published theme resource first; otherwise FollowPlaynite
            // would always classify the page from that local fallback before it can see the host.
            var rawBackground = highContrast
                ? SystemColors.WindowColor
                : forcedLight
                    ? Color.FromRgb(243, 244, 248)
                    : forcedDark
                        ? Color.FromRgb(23, 24, 31)
                        : ResolveFirstResourceColor(host, BackgroundResourceKeys)
                            ?? ResolveHostBackground(host)
                            ?? InferBackgroundFromHostText(hostText, hostInverseText)
                            ?? Color.FromRgb(17, 19, 25);

            var text = highContrast ? SystemColors.WindowTextColor : forcedLight ? Colors.Black : forcedDark ? Colors.White : hostText;
            var inverseText = highContrast ? SystemColors.WindowTextColor : forcedLight ? Colors.White : forcedDark ? Colors.Black : hostInverseText;

            // If a theme uses a transparent/image background, infer a stable local surface from the
            // required text brushes. Otherwise preserve some of the theme's own color character.
            var initialDark = RelativeLuminance(rawBackground) < 0.48;
            var fallbackBackground = initialDark
                ? Color.FromRgb(15, 17, 22)
                : Color.FromRgb(246, 247, 250);

            var primaryText = ChooseBestText(rawBackground, text, inverseText, initialDark);
            if (ContrastRatio(primaryText, rawBackground) < 4.5)
            {
                rawBackground = fallbackBackground;
                primaryText = ChooseBestText(rawBackground, text, inverseText, initialDark);
            }

            var isDark = RelativeLuminance(rawBackground) < 0.5;
            // Keep the embedded page in AcrylicFork's neutral graphite family even when the
            // host theme exposes a vivid blue/black background. We still retain a small amount
            // of the host's light/dark character, but do not let it tint every reading surface.
            var stableBase = Blend(rawBackground, isDark ? Color.FromRgb(17, 19, 25) : Color.FromRgb(248, 249, 252), isDark ? 0.56 : 0.34);
            primaryText = ChooseBestText(stableBase, text, inverseText, isDark);
            if (ContrastRatio(primaryText, stableBase) < 7)
                primaryText = isDark ? Colors.White : Colors.Black;

            var strength = Math.Max(20, Math.Min(100, strengthPercent)) / 100.0;
            var controlFill = isDark
                ? Blend(stableBase, Colors.White, 0.075)
                : Blend(stableBase, Colors.Black, 0.035);
            var strongControl = isDark
                ? Blend(stableBase, Colors.White, 0.105)
                : Blend(stableBase, Colors.Black, 0.02);
            // Keep the standalone demo and the embedded Playnite view on the same blue
            // interaction language. AcrylicFork's reference accents are indigo #7C8CF8,
            // sky #4FA3F0, cyan #35B8C9, mint #4CC08A, violet #A07BF5, amber #E8973C,
            // and rose #E56E8C. Production keeps indigo as its deterministic fallback while
            // FollowPlaynite still honors a host accent when one is exposed.
            var fallbackAccent = isDark ? Color.FromRgb(124, 140, 248) : Color.FromRgb(91, 116, 230);
            var hostAccent = !forcedLight && !forcedDark && !highContrast
                ? ResolveFirstResourceColor(host, AccentResourceKeys)
                : null;
            // FollowPlaynite keeps an explicitly published host accent intact. The local
            // fallback is the quieter demo violet-blue used when the host has no accent.
            var requestedAccent = hostAccent ?? fallbackAccent;
            var accent = EnsureContrast(highContrast ? SystemColors.HighlightColor : requestedAccent, stableBase, isDark);
            var accentHover = Blend(accent, isDark ? Colors.White : Colors.Black, 0.1);
            var accentPressed = Blend(accent, Colors.Black, isDark ? 0.16 : 0.2);
            // Keep the host accent for actions and focus rings, but soften large selected
            // surfaces. Playnite themes often publish a saturated blue/purple accent; using
            // that color at 24–34% opacity on every card makes the page look like a blue admin
            // dashboard instead of the demo's restrained navy surface hierarchy.
            var tintAccent = hostAccent.HasValue
                ? Blend(accent, fallbackAccent, 0.42)
                : accent;
            var onAccentText = highContrast
                ? SystemColors.HighlightTextColor
                : ChooseBestText(accent, Colors.White, Colors.Black, RelativeLuminance(accent) < 0.5);
            var info = highContrast
                ? EnsureContrast(SystemColors.HighlightColor, stableBase, isDark)
                : Color.FromRgb(92, 170, 240);
            var success = highContrast
                ? EnsureContrast(SystemColors.HotTrackColor, stableBase, isDark)
                : Color.FromRgb(76, 219, 142);
            var warning = highContrast ? primaryText : Color.FromRgb(240, 178, 78);
            var error = highContrast ? primaryText : Color.FromRgb(242, 109, 126);

            // A larger strength should reveal more of the bounded ambient material instead
            // of making every card an opaque mask. Keep the low end stable and readable, then
            // open the surface gradually as the user moves toward 100%.
            var surfaceOpacity = glassEnabled
                ? 0.72 + (0.10 * (1 - strength))
                : 1;
            var strongTopOpacity = glassEnabled
                ? 0.80 + (0.08 * (1 - strength))
                : 1;
            var strongBottomOpacity = glassEnabled
                ? 0.68 + (0.10 * (1 - strength))
                : 1;
            var surfaceTop = glassEnabled
                ? WithAlpha(strongControl, surfaceOpacity)
                : WithAlpha(strongControl, 1);
            var surfaceBottom = glassEnabled
                ? WithAlpha(controlFill, surfaceOpacity)
                : WithAlpha(controlFill, 1);
            var strongTop = glassEnabled
                ? WithAlpha(Blend(strongControl, primaryText, isDark ? 0.018 : 0.006), strongTopOpacity)
                : WithAlpha(strongControl, 1);
            var strongBottom = glassEnabled
                ? WithAlpha(controlFill, strongBottomOpacity)
                : WithAlpha(controlFill, 1);

            return new AdaptiveThemePalette
            {
                IsDark = isDark,
                GlassStrength = strength,
                Background = stableBase,
                PrimaryText = primaryText,
                SecondaryText = WithAlpha(primaryText, 0.74),
                MutedText = WithAlpha(primaryText, 0.56),
                DisabledText = WithAlpha(primaryText, 0.38),
                ControlFill = WithAlpha(controlFill, glassEnabled
                    ? 0.78 + (0.10 * (1 - strength))
                    : 1),
                // UiLab uses a hairline rather than a bright outline. Keep the production
                // surfaces readable while avoiding the sharp blue/white frame seen in the
                // host screenshots at 125–150% DPI.
                ControlStroke = WithAlpha(primaryText, isDark ? 0.10 : 0.09),
                Divider = WithAlpha(primaryText, isDark ? 0.09 : 0.08),
                SurfaceTop = surfaceTop,
                SurfaceBottom = surfaceBottom,
                StrongSurfaceTop = strongTop,
                StrongSurfaceBottom = strongBottom,
                SidebarTop = glassEnabled ? WithAlpha(strongControl, 0.74 * strength) : WithAlpha(strongControl, 1),
                SidebarBottom = glassEnabled ? WithAlpha(stableBase, 0.64 * strength) : WithAlpha(stableBase, 1),
                Backdrop = WithAlpha(stableBase, glassEnabled ? 0.26 : 1),
                Highlight = WithAlpha(primaryText, isDark ? 0.075 : 0.24),
                Accent = accent,
                AccentHover = accentHover,
                AccentPressed = accentPressed,
                AccentTint = highContrast ? accent : WithAlpha(tintAccent, isDark ? 0.16 : 0.11),
                AccentTintStrong = highContrast ? accent : WithAlpha(tintAccent, isDark ? 0.24 : 0.16),
                AccentIconFill = highContrast ? accent : WithAlpha(tintAccent, isDark ? 0.16 : 0.11),
                OnAccentText = onAccentText,
                Info = info,
                Success = success,
                Warning = warning,
                Error = error,
                InfoIconFill = highContrast ? info : WithAlpha(info, 0.13),
                SuccessIconFill = highContrast ? success : WithAlpha(success, 0.13),
                WarningIconFill = highContrast ? warning : WithAlpha(warning, 0.13),
                ErrorIconFill = highContrast ? error : WithAlpha(error, 0.13)
            };
        }

        public static void ApplyAccentResources(ResourceDictionary resources, AdaptiveThemePalette palette)
        {
            // AcrylicReferenceControls.xaml still exposes the same short token names as the
            // standalone Demo (AccentBrush, AccentTintBrush, ...). The Demo writes these aliases
            // when its accent changes; production used to update only the Gsc-prefixed tokens.
            // In a Playnite host the unresolved aliases can come from the host theme and render
            // icon tiles, chips and primary buttons black/transparent. Keep the aliases local to
            // this view resource dictionary so we do not mutate Playnite's global palette.
            resources["AccentBrush"] = Brush(palette.Accent);
            resources["AccentHoverBrush"] = Brush(palette.AccentHover);
            resources["AccentPressedBrush"] = Brush(palette.AccentPressed);
            resources["AccentStrokeBrush"] = Brush(palette.AccentHover);
            resources["AccentTintBrush"] = Brush(palette.AccentTint);
            resources["AccentTintStrongBrush"] = Brush(palette.AccentTintStrong);
            resources["AccentWashBrush"] = Brush(WithAlpha(palette.Accent, palette.IsDark ? 0.16 : 0.12));
            resources["TextOnAccentBrush"] = Brush(palette.OnAccentText);
            resources["AccentBrushColor"] = palette.Accent;
            resources["AccentPressedColor"] = palette.AccentPressed;
            resources["AccentWashColor"] = WithAlpha(palette.Accent, palette.IsDark ? 0.17 : 0.13);

            resources["GscAccentBrush"] = Brush(palette.Accent);
            resources["GscAccentHoverBrush"] = Brush(palette.AccentHover);
            resources["GscAccentPressedBrush"] = Brush(palette.AccentPressed);
            resources["GscAccentTintBrush"] = Brush(palette.AccentTint);
            resources["GscAccentTintStrongBrush"] = Brush(palette.AccentTintStrong);
            resources["GscAccentIconFillBrush"] = Brush(palette.AccentIconFill);
            resources["GscOnAccentTextBrush"] = Brush(palette.OnAccentText);
            resources["GscSelectionTextBrush"] = Brush(SystemParameters.HighContrast ? SystemColors.HighlightTextColor : palette.PrimaryText);
            resources["GscPrimaryButtonBrush"] = Gradient(palette.Accent, palette.AccentPressed);
            resources["GscPrimaryButtonBorderBrush"] = Brush(palette.AccentHover);
            // The strength slider used to affect mostly surface opacity, so a value of 100
            // could actually hide the ambient light behind the reading surfaces. Keep the
            // pane-wide material tied to the same value while its alpha remains bounded.
            resources["GscInfoBrush"] = Brush(palette.Info);
            resources["GscSuccessBrush"] = Brush(palette.Success);
            resources["GscWarningBrush"] = Brush(palette.Warning);
            resources["GscErrorBrush"] = Brush(palette.Error);
            // These semantic surfaces are used by both the extracted workspaces and the
            // settings page. Keep them in the same palette as their status strokes instead
            // of leaving the static DesignTokens fallback active after a theme switch.
            resources["GscErrorTintBrush"] = Brush(SemanticTint(palette.Error, palette.IsDark ? 0.20 : 0.12));
            resources["GscRestoreInfoFillBrush"] = Brush(SemanticTint(palette.Info, palette.IsDark ? 0.20 : 0.11));
            resources["GscRestoreInfoStrokeBrush"] = Brush(SemanticTint(palette.Info, palette.IsDark ? 0.46 : 0.32));
            resources["GscSafetyFillBrush"] = Brush(SemanticTint(palette.Warning, palette.IsDark ? 0.20 : 0.12));
            resources["GscSafetyStrokeBrush"] = Brush(SemanticTint(palette.Warning, palette.IsDark ? 0.48 : 0.34));
            resources["GscMutedStatusBrush"] = Brush(SemanticTint(palette.PrimaryText, palette.IsDark ? 0.54 : 0.46));
            resources["GscInfoIconFillBrush"] = Brush(palette.InfoIconFill);
            resources["GscSuccessIconFillBrush"] = Brush(palette.SuccessIconFill);
            resources["GscWarningIconFillBrush"] = Brush(palette.WarningIconFill);
            resources["GscErrorIconFillBrush"] = Brush(palette.ErrorIconFill);
        }

        /// <summary>
        /// Keeps elevation effects local to an embedded plugin page and removes them entirely
        /// when transparent material is unavailable. An Effect with Opacity=0 still retains an
        /// effect visual, so accessibility and low-cost fallback paths must use a real null.
        /// </summary>
        public static void ApplyMaterialResources(ResourceDictionary resources, AdaptiveThemePalette palette, bool glassEnabled, bool motionEnabled)
        {
            resources["GscSurfaceEffect"] = CreateShadowEffect(glassEnabled, Colors.Black, 14, 2, palette.IsDark ? 0.34 : 0.24);
            resources["GscPrimaryButtonEffect"] = CreateShadowEffect(glassEnabled, palette.Accent, 12, 2, palette.IsDark ? 0.32 : 0.28);
            resources["GscSidebarEffect"] = CreateShadowEffect(glassEnabled, Colors.Black, 24, 3, palette.IsDark ? 0.42 : 0.30);
            resources["GscPopupEffect"] = CreateShadowEffect(glassEnabled, Colors.Black, 20, 5, palette.IsDark ? 0.46 : 0.38);
            resources["GscDialogEffect"] = CreateShadowEffect(glassEnabled, Colors.Black, 34, 8, palette.IsDark ? 0.52 : 0.44);
            resources["GscSliderThumbEffect"] = CreateShadowEffect(glassEnabled, Colors.Black, 6, 1, 0.26);
            resources["GscPopupAllowsTransparency"] = glassEnabled;
            resources["GscPopupAnimation"] = motionEnabled ? PopupAnimation.Fade : PopupAnimation.None;
            var glassStrength = Math.Max(0.2, Math.Min(1, palette.GlassStrength));
            resources["GscAmbientPageOpacity"] = glassEnabled
                ? (palette.IsDark ? 0.74 + (0.26 * glassStrength) : 0.84 + (0.16 * glassStrength))
                : 0d;
            resources["GscShellAmbientOpacity"] = glassEnabled
                ? 0.68 + (0.32 * glassStrength)
                : 0d;
            var gameBackgroundVisible = glassEnabled && !SystemParameters.HighContrast;
            resources["GscGameBackgroundOpacity"] = gameBackgroundVisible
                ? (palette.IsDark ? 0.48 : 0.40)
                : 0d;
            resources["GscGameBackgroundTintBrush"] = Brush(gameBackgroundVisible
                ? Color.FromArgb(palette.IsDark ? (byte)0x52 : (byte)0x66,
                    palette.Backdrop.R, palette.Backdrop.G, palette.Backdrop.B)
                : Colors.Transparent);
            resources["GscGameBackgroundEffect"] = CreateGameBackgroundBlurEffect(gameBackgroundVisible, glassStrength);
            // This is a wide, non-circular material wash. It adds a low-cost sense of depth
            // across large empty regions without a page-sized BlurEffect or spotlight tree.
            resources["GscAmbientWideWashBrush"] = CreateAmbientWideWashBrush(palette, glassEnabled);
        }

        /// <summary>
        /// WPF-UI resolves these Fluent token names through dynamic resources. Keep the overrides
        /// local to an embedded GameSaveCenter view so a Playnite theme (or another extension)
        /// is never mutated, while WPF-UI controls still share the same palette as native controls.
        /// </summary>
        public static void ApplyWpfUiResources(ResourceDictionary resources, AdaptiveThemePalette palette)
        {
            var secondaryFill = palette.IsDark
                ? Blend(palette.ControlFill, Colors.White, 0.045)
                : Blend(palette.ControlFill, Colors.Black, 0.025);
            var tertiaryFill = palette.IsDark
                ? Blend(palette.ControlFill, Colors.White, 0.085)
                : Blend(palette.ControlFill, Colors.Black, 0.055);

            resources["AccentFillColorDefaultBrush"] = Brush(palette.Accent);
            resources["AccentFillColorSecondaryBrush"] = Brush(palette.AccentHover);
            resources["AccentFillColorTertiaryBrush"] = Brush(palette.AccentPressed);
            resources["AccentFillColorDisabledBrush"] = Brush(WithAlpha(palette.Accent, 0.38));
            resources["TextOnAccentFillColorPrimaryBrush"] = Brush(palette.OnAccentText);
            resources["TextOnAccentFillColorSelectedTextBrush"] = Brush(palette.OnAccentText);
            resources["TextFillColorPrimaryBrush"] = Brush(palette.PrimaryText);
            resources["TextFillColorSecondaryBrush"] = Brush(palette.SecondaryText);
            resources["TextFillColorTertiaryBrush"] = Brush(palette.MutedText);
            resources["TextFillColorDisabledBrush"] = Brush(palette.DisabledText);
            resources["ControlFillColorDefaultBrush"] = Brush(palette.ControlFill);
            resources["ControlFillColorSecondaryBrush"] = Brush(secondaryFill);
            resources["ControlFillColorTertiaryBrush"] = Brush(tertiaryFill);
            resources["ControlFillColorInputActiveBrush"] = Brush(tertiaryFill);
            resources["GscControlFocusFillBrush"] = Brush(tertiaryFill);
            resources["ControlFillColorDisabledBrush"] = Brush(WithAlpha(palette.ControlFill, 0.5));
            resources["ControlSolidFillColorDefaultBrush"] = Brush(palette.Accent);
            resources["ControlStrokeColorDefaultBrush"] = Brush(palette.ControlStroke);
            resources["ControlStrokeColorSecondaryBrush"] = Brush(palette.Divider);
            resources["CardBackgroundFillColorDefaultBrush"] = Brush(palette.StrongSurfaceTop);
            resources["CardStrokeColorDefaultBrush"] = Brush(palette.ControlStroke);
            resources["FocusStrokeColorOuterBrush"] = Brush(palette.Accent);
            resources["FocusStrokeColorInnerBrush"] = Brush(palette.OnAccentText);
        }

        /// <summary>
        /// Applies every runtime theme resource used by the Dashboard shell and its extracted
        /// workspace views.  Workspace UserControls own local dictionaries for their styles, so
        /// copying only the shell resources would leave their DynamicResource lookups on the
        /// static DesignTokens fallback after a theme switch.
        /// </summary>
        public static void ApplyRuntimeThemeResources(ResourceDictionary resources, AdaptiveThemePalette palette, bool glassEnabled, bool motionEnabled)
        {
            ApplyAccentResources(resources, palette);
            ApplyMaterialResources(resources, palette, glassEnabled, motionEnabled);
            ApplyWpfUiResources(resources, palette);
            resources["GscPrimaryTextBrush"] = Brush(palette.PrimaryText);
            resources["GscSecondaryTextBrush"] = Brush(palette.SecondaryText);
            resources["GscMutedTextBrush"] = Brush(palette.MutedText);
            resources["GscDisabledTextBrush"] = Brush(palette.DisabledText);
            resources["GscControlFillBrush"] = Brush(palette.ControlFill);
            resources["GscSegmentFillBrush"] = Brush(Color.FromArgb(
                palette.IsDark ? (byte)89 : (byte)38,
                palette.PrimaryText.R,
                palette.PrimaryText.G,
                palette.PrimaryText.B));
            resources["GscSegmentItemFillBrush"] = Brush(Color.FromArgb(
                palette.IsDark ? (byte)242 : (byte)240,
                palette.StrongSurfaceTop.R,
                palette.StrongSurfaceTop.G,
                palette.StrongSurfaceTop.B));
            resources["GscSegmentItemStrokeBrush"] = Brush(Color.FromArgb(
                palette.IsDark ? (byte)28 : (byte)20,
                palette.PrimaryText.R,
                palette.PrimaryText.G,
                palette.PrimaryText.B));
            resources["GscProgressTrackBrush"] = Brush(palette.IsDark
                ? Color.FromRgb(45, 50, 62)
                : Color.FromRgb(214, 220, 232));
            resources["GscProgressFillBrush"] = Brush(palette.Accent);
            resources["GscControlStrokeBrush"] = Brush(palette.ControlStroke);
            resources["GscDividerBrush"] = Brush(palette.Divider);
            resources["GscTableDividerBrush"] = Brush(Color.FromArgb(
                palette.IsDark ? (byte)16 : (byte)13, palette.PrimaryText.R, palette.PrimaryText.G, palette.PrimaryText.B));
            resources["GscPopupBrush"] = Brush(Color.FromArgb(
                250, palette.StrongSurfaceTop.R, palette.StrongSurfaceTop.G, palette.StrongSurfaceTop.B));
            resources["GscGlassFillBrush"] = Gradient(palette.SurfaceTop, palette.SurfaceBottom);
            resources["GscGlassStrongBrush"] = Gradient(palette.StrongSurfaceTop, palette.StrongSurfaceBottom);
            resources["GscSidebarBrush"] = Gradient(palette.SidebarTop, palette.SidebarBottom);
            resources["GscGlassStrokeBrush"] = Brush(palette.ControlStroke);
            resources["GscGlassHighlightBrush"] = Brush(glassEnabled ? palette.Highlight : Colors.Transparent);
            resources["GscBackdropBrush"] = Brush(palette.Backdrop);
            // The Demo owns a single, readable header band inside each table frame.  A nearly
            // transparent header is indistinguishable from the rows in Playnite and makes the
            // column labels look like they are floating over the data, especially in light
            // themes.  Reuse the opaque strong surface so every DataGrid gets the same band
            // without adding a second outer rectangle or changing the project's scrollbar.
            resources["GscTableHeaderBrush"] = Brush(palette.StrongSurfaceTop);
            resources["GscTableRowBrush"] = Brush(Color.FromArgb(
                SystemParameters.HighContrast ? (byte)0 : palette.IsDark ? (byte)10 : (byte)6,
                palette.PrimaryText.R, palette.PrimaryText.G, palette.PrimaryText.B));
            resources["GscTableAlternateRowBrush"] = Brush(Color.FromArgb(
                SystemParameters.HighContrast ? (byte)0 : palette.IsDark ? (byte)18 : (byte)12,
                palette.PrimaryText.R, palette.PrimaryText.G, palette.PrimaryText.B));
            resources["GscRowHoverBrush"] = Brush(Color.FromArgb(
                palette.IsDark ? (byte)12 : (byte)8, palette.PrimaryText.R, palette.PrimaryText.G, palette.PrimaryText.B));
            resources["GscRowHoverStrongBrush"] = Brush(Color.FromArgb(
                SystemParameters.HighContrast ? (byte)0 : palette.IsDark ? (byte)32 : (byte)18,
                palette.PrimaryText.R, palette.PrimaryText.G, palette.PrimaryText.B));
            resources["GscScrollTrackBrush"] = Brush(Color.FromArgb(
                palette.IsDark ? (byte)28 : (byte)20, palette.PrimaryText.R, palette.PrimaryText.G, palette.PrimaryText.B));
            resources["GscScrollThumbBrush"] = Brush(Color.FromArgb(
                palette.IsDark ? (byte)88 : (byte)68, palette.PrimaryText.R, palette.PrimaryText.G, palette.PrimaryText.B));
            // Scrollbar hover must follow the active Playnite accent (blue, purple, or a
            // custom high-contrast highlight) instead of retaining the old purple fallback.
            resources["GscScrollThumbHoverBrush"] = Brush(WithAlpha(palette.AccentHover, palette.IsDark ? 0.78 : 0.66));
            resources["GscOverlayBrush"] = Brush(Color.FromArgb(
                palette.IsDark ? (byte)138 : (byte)72, 0, 0, 0));
            resources["GscPickerScrimBrush"] = Brush(Color.FromArgb(
                palette.IsDark ? (byte)54 : (byte)34, 0, 0, 0));
            WpfUiThemeScope.Apply(resources, palette.IsDark);
            ApplyDemoCoreResources(resources, palette.IsDark);
            // The shell sidebar is one continuous translucent material surface. Keep this
            // separate from GscSidebarBrush, which is also used by standalone rounded cards
            // in the settings view; the shell brush can therefore carry a broad navigation wash
            // without reintroducing a transparent edge or a bright vertical seam.
            resources["GscSidebarMaterialBrush"] = CreateSidebarMaterialBrush(palette, glassEnabled);
            if (!glassEnabled)
                resources["GscGlassHighlightBrush"] = Brush(Colors.Transparent);
        }

        /// <summary>
        /// Applies the selected game's sampled colors to the shared reading surfaces. The actual
        /// artwork is already blurred by the shell; these resources only add a controlled,
        /// translucent color field to cards, table frames and floating containers. Keeping this
        /// at the resource level makes every page follow the same material without attaching an
        /// expensive BlurEffect to each control (which would blur its own text as well).
        /// </summary>
        public static void ApplyGameBackgroundGlassResources(
            ResourceDictionary resources,
            AdaptiveThemePalette palette,
            Brush? ambientBrush,
            bool hasGameMaterial,
            bool glassEnabled)
        {
            if (!glassEnabled || !hasGameMaterial || ambientBrush == null || SystemParameters.HighContrast)
            {
                // ApplyDemoCoreResources is the single source of truth for the neutral fallback
                // values. This also restores the fallback immediately when a game has no image.
                ApplyDemoCoreResources(resources, palette.IsDark);
                return;
            }

            var strength = Math.Max(0.2, Math.Min(1, palette.GlassStrength));
            var fillOpacity = palette.IsDark
                ? 0.80 - (0.12 * strength)
                : 0.95 - (0.08 * strength);
            var strongOpacity = palette.IsDark
                ? 0.90 - (0.10 * strength)
                : 0.98 - (0.06 * strength);
            var fillTint = palette.IsDark ? 0.20 : 0.14;
            var strongTint = palette.IsDark ? 0.16 : 0.11;

            resources["GscGlassFillBrush"] = CreateGameGlassBrush(
                ambientBrush, palette.SurfaceTop, palette.SurfaceBottom, fillTint, fillOpacity);
            resources["GscGlassStrongBrush"] = CreateGameGlassBrush(
                ambientBrush, palette.StrongSurfaceTop, palette.StrongSurfaceBottom, strongTint, strongOpacity);
            resources["GscTableHeaderBrush"] = CreateGameGlassBrush(
                ambientBrush, palette.StrongSurfaceTop, palette.StrongSurfaceBottom, strongTint * 0.72, strongOpacity + 0.02);
            resources["GscPopupBrush"] = CreateGameGlassBrush(
                ambientBrush, palette.StrongSurfaceTop, palette.StrongSurfaceBottom, strongTint * 0.82, Math.Min(0.98, strongOpacity + 0.04));
            resources["CardBackgroundFillColorDefaultBrush"] = resources["GscGlassStrongBrush"];
        }

        /// <summary>
        /// Settings is a standalone window/page and intentionally does not follow the selected
        /// game's artwork. It still needs a material hierarchy of its own so the ambient theme
        /// wash is visible through the shell, rail, form card and content surface.
        /// </summary>
        public static void ApplySettingsMaterialResources(
            ResourceDictionary resources,
            AdaptiveThemePalette palette,
            bool glassEnabled)
        {
            resources["GscSettingsAmbientBrush"] = CreateSettingsAmbientBrush(
                palette, glassEnabled && !SystemParameters.HighContrast);
            resources["GscSettingsAmbientEffect"] = CreateSettingsAmbientBlurEffect(
                glassEnabled && !SystemParameters.HighContrast, palette.GlassStrength);

            if (!glassEnabled || SystemParameters.HighContrast)
            {
                resources["GscSettingsShellBrush"] = Brush(Opaque(palette.Background));
                resources["GscSettingsPanelBrush"] = Brush(Opaque(palette.StrongSurfaceTop));
                resources["GscSettingsCardBrush"] = Brush(Opaque(palette.StrongSurfaceBottom));
                resources["GscSettingsContentBrush"] = Brush(Opaque(palette.Background));
                return;
            }

            var strength = Math.Max(0.2, Math.Min(1, palette.GlassStrength));
            var shellTop = Blend(Opaque(palette.SurfaceTop), Opaque(palette.StrongSurfaceTop), 0.42);
            var shellBottom = Blend(Opaque(palette.SurfaceBottom), Opaque(palette.StrongSurfaceBottom), 0.36);
            var panelTop = Blend(shellTop, Opaque(palette.StrongSurfaceTop), 0.34);
            var panelBottom = Blend(shellBottom, Opaque(palette.StrongSurfaceBottom), 0.30);
            var cardTop = Blend(panelTop, Colors.White, palette.IsDark ? 0.025 : 0.018);
            var cardBottom = Blend(panelBottom, Colors.Black, palette.IsDark ? 0.018 : 0.012);

            resources["GscSettingsShellBrush"] = CreateSettingsSurfaceBrush(
                shellTop, shellBottom,
                palette.IsDark ? 0.58 - (0.04 * strength) : 0.78 - (0.04 * strength));
            resources["GscSettingsPanelBrush"] = CreateSettingsSurfaceBrush(
                panelTop, panelBottom,
                palette.IsDark ? 0.68 - (0.05 * strength) : 0.84 - (0.04 * strength));
            resources["GscSettingsCardBrush"] = CreateSettingsSurfaceBrush(
                cardTop, cardBottom,
                palette.IsDark ? 0.74 - (0.06 * strength) : 0.88 - (0.04 * strength));
            resources["GscSettingsContentBrush"] = CreateSettingsSurfaceBrush(
                shellTop, shellBottom,
                palette.IsDark ? 0.24 - (0.04 * strength) : 0.35 - (0.05 * strength));
        }

        /// <summary>
        /// Keeps the migrated page surfaces on the Demo's light/dark color relationships.
        /// Playnite may still provide the accent used for focus and primary actions, but its
        /// background/text brushes must not recolor the page cards, tables or status surfaces.
        /// High contrast intentionally stays on the adaptive opaque path above.
        /// </summary>
        public static void ApplyDemoCoreResources(ResourceDictionary resources, bool isDark)
        {
            if (SystemParameters.HighContrast)
                return;

            var canvasStart = isDark ? Color.FromRgb(29, 32, 39) : Color.FromRgb(239, 239, 244);
            var canvasMiddle = isDark ? Color.FromRgb(23, 26, 32) : Color.FromRgb(233, 234, 240);
            var canvasEnd = isDark ? Color.FromRgb(19, 21, 25) : Color.FromRgb(228, 230, 237);
            var card = isDark ? Color.FromArgb(0xEE, 0x26, 0x2B, 0x36) : Color.FromArgb(0xF5, 0xFF, 0xFF, 0xFF);
            var cardStroke = isDark ? Color.FromArgb(0x17, 0xFF, 0xFF, 0xFF) : Color.FromArgb(0x12, 0x00, 0x00, 0x00);
            var cardHoverStroke = isDark ? Color.FromArgb(0x2E, 0xFF, 0xFF, 0xFF) : Color.FromArgb(0x24, 0x00, 0x00, 0x00);
            var header = isDark ? Color.FromArgb(0xA0, 0x20, 0x24, 0x30) : Color.FromArgb(0x8C, 0xF2, 0xF4, 0xFA);
            var sidebar = isDark ? Color.FromArgb(0x8C, 0x1B, 0x1F, 0x2A) : Color.FromArgb(0xA9, 0xF4, 0xF6, 0xFB);
            var field = isDark ? Color.FromArgb(0x66, 0x13, 0x16, 0x20) : Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF);
            var fieldStroke = isDark ? Color.FromArgb(0x1C, 0xFF, 0xFF, 0xFF) : Color.FromArgb(0x17, 0x00, 0x00, 0x00);
            var floating = isDark ? Color.FromArgb(0xF0, 0x26, 0x2C, 0x3A) : Color.FromArgb(0xF2, 0xFF, 0xFF, 0xFF);
            var primaryText = isDark ? Color.FromRgb(0xF2, 0xF4, 0xF8) : Color.FromArgb(0xF2, 0x1B, 0x1F, 0x27);
            var secondaryText = isDark ? Color.FromRgb(0xB9, 0xC0, 0xCC) : Color.FromArgb(0xF2, 0x4E, 0x56, 0x66);
            var tertiaryText = isDark ? Color.FromRgb(0x82, 0x8A, 0x99) : Color.FromArgb(0xF2, 0x8A, 0x92, 0xA1);
            var divider = isDark ? Color.FromArgb(0x12, 0xFF, 0xFF, 0xFF) : Color.FromArgb(0x12, 0x00, 0x00, 0x00);
            var tableHeader = isDark ? Color.FromArgb(0x0D, 0xFF, 0xFF, 0xFF) : Color.FromArgb(0x0A, 0x00, 0x00, 0x00);
            var rowHover = isDark ? Color.FromArgb(0x0C, 0xFF, 0xFF, 0xFF) : Color.FromArgb(0x08, 0x00, 0x00, 0x00);
            var scrollThumb = isDark ? Color.FromArgb(0x38, 0xFF, 0xFF, 0xFF) : Color.FromArgb(0x33, 0x00, 0x00, 0x00);
            var scrollThumbHover = isDark ? Color.FromArgb(0x52, 0xFF, 0xFF, 0xFF) : Color.FromArgb(0x52, 0x00, 0x00, 0x00);
            var scrim = isDark ? Color.FromArgb(0x66, 0x0D, 0x0F, 0x14) : Color.FromArgb(0x47, 0x10, 0x14, 0x18);
            var success = isDark ? Color.FromRgb(0x4C, 0xDB, 0x8E) : Color.FromRgb(0x12, 0x8A, 0x4C);
            var successFill = isDark ? Color.FromArgb(0x23, 0x4C, 0xDB, 0x8E) : Color.FromArgb(0x1F, 0x12, 0x8A, 0x4C);
            var warning = isDark ? Color.FromRgb(0xF0, 0xB2, 0x4E) : Color.FromRgb(0x9A, 0x6A, 0x14);
            var warningFill = isDark ? Color.FromArgb(0x26, 0xF0, 0xB2, 0x4E) : Color.FromArgb(0x22, 0x9A, 0x6A, 0x14);
            var error = isDark ? Color.FromRgb(0xF2, 0x6D, 0x7E) : Color.FromRgb(0xC7, 0x41, 0x52);
            var errorFill = isDark ? Color.FromArgb(0x26, 0xF2, 0x6D, 0x7E) : Color.FromArgb(0x20, 0xC7, 0x41, 0x52);
            var info = isDark ? Color.FromRgb(0x5C, 0xAA, 0xF0) : Color.FromRgb(0x25, 0x6F, 0xBD);
            var infoFill = isDark ? Color.FromArgb(0x22, 0x5C, 0xAA, 0xF0) : Color.FromArgb(0x1F, 0x25, 0x6F, 0xBD);
            var neutral = isDark ? Color.FromRgb(0xB9, 0xC0, 0xCC) : Color.FromRgb(0x4E, 0x56, 0x66);

            resources["GscBackdropBrush"] = CanvasGradient(canvasStart, canvasMiddle, canvasEnd);
            resources["GscGlassFillBrush"] = Brush(header);
            resources["GscGlassStrongBrush"] = Brush(card);
            resources["GscGlassStrokeBrush"] = Brush(cardStroke);
            resources["GscGlassHighlightBrush"] = Brush(cardHoverStroke);
            resources["GscSidebarBrush"] = Brush(sidebar);
            resources["GscPrimaryTextBrush"] = Brush(primaryText);
            resources["GscSecondaryTextBrush"] = Brush(secondaryText);
            resources["GscMutedTextBrush"] = Brush(tertiaryText);
            resources["GscDisabledTextBrush"] = Brush(tertiaryText);
            resources["GscControlFillBrush"] = Brush(field);
            resources["GscControlFocusFillBrush"] = Brush(isDark ? Color.FromArgb(0x8C, 0x1A, 0x1E, 0x2B) : Color.FromArgb(0xF9, 0xFF, 0xFF, 0xFF));
            resources["GscControlStrokeBrush"] = Brush(fieldStroke);
            resources["GscDividerBrush"] = Brush(divider);
            resources["GscTableDividerBrush"] = Brush(divider);
            resources["GscPopupBrush"] = Brush(floating);
            resources["GscTableHeaderBrush"] = Brush(tableHeader);
            resources["GscTableRowBrush"] = Brush(isDark
                ? Color.FromArgb(0x0A, 0xFF, 0xFF, 0xFF)
                : Color.FromArgb(0x06, 0x00, 0x00, 0x00));
            resources["GscTableAlternateRowBrush"] = Brush(isDark
                ? Color.FromArgb(0x12, 0xFF, 0xFF, 0xFF)
                : Color.FromArgb(0x0C, 0x00, 0x00, 0x00));
            resources["GscRowHoverBrush"] = Brush(rowHover);
            resources["GscRowHoverStrongBrush"] = Brush(rowHover);
            resources["GscScrollTrackBrush"] = Brush(Colors.Transparent);
            resources["GscScrollThumbBrush"] = Brush(scrollThumb);
            resources["GscScrollThumbHoverBrush"] = Brush(scrollThumbHover);
            resources["GscOverlayBrush"] = Brush(scrim);
            resources["GscPickerScrimBrush"] = Brush(scrim);
            resources["GscSelectionTextBrush"] = Brush(primaryText);
            resources["GscProgressTrackBrush"] = Brush(isDark ? Color.FromArgb(0x59, 0xFF, 0xFF, 0xFF) : Color.FromArgb(0x26, 0x00, 0x00, 0x00));
            resources["GscSegmentFillBrush"] = Brush(isDark ? Color.FromArgb(0x59, 0xFF, 0xFF, 0xFF) : Color.FromArgb(0x26, 0x00, 0x00, 0x00));
            resources["GscSegmentItemFillBrush"] = Brush(isDark ? Color.FromArgb(0xF2, 0x37, 0x3D, 0x4C) : Color.FromArgb(0xF0, 0xFF, 0xFF, 0xFF));
            resources["GscSegmentItemStrokeBrush"] = Brush(isDark ? Color.FromArgb(0x1C, 0xFF, 0xFF, 0xFF) : Color.FromArgb(0x14, 0x00, 0x00, 0x00));
            resources["GscInfoBrush"] = Brush(info);
            resources["GscSuccessBrush"] = Brush(success);
            resources["GscWarningBrush"] = Brush(warning);
            resources["GscErrorBrush"] = Brush(error);
            resources["GscInfoIconFillBrush"] = Brush(infoFill);
            resources["GscRestoreInfoFillBrush"] = Brush(infoFill);
            resources["GscRestoreInfoStrokeBrush"] = Brush(info);
            resources["GscSuccessIconFillBrush"] = Brush(successFill);
            resources["GscWarningIconFillBrush"] = Brush(warningFill);
            resources["GscSafetyFillBrush"] = Brush(warningFill);
            resources["GscSafetyStrokeBrush"] = Brush(warning);
            resources["GscErrorIconFillBrush"] = Brush(errorFill);
            resources["GscErrorTintBrush"] = Brush(errorFill);
            resources["GscMutedStatusBrush"] = Brush(neutral);

            resources["TextFillColorPrimaryBrush"] = Brush(primaryText);
            resources["TextFillColorSecondaryBrush"] = Brush(secondaryText);
            resources["TextFillColorTertiaryBrush"] = Brush(tertiaryText);
            resources["TextFillColorDisabledBrush"] = Brush(tertiaryText);
            resources["ControlFillColorDefaultBrush"] = Brush(field);
            resources["ControlFillColorSecondaryBrush"] = Brush(isDark ? Color.FromArgb(0xD9, 0x20, 0x25, 0x31) : Color.FromArgb(0x66, 0xF2, 0xF4, 0xF9));
            resources["ControlFillColorTertiaryBrush"] = Brush(isDark ? Color.FromArgb(0x8C, 0x1A, 0x1E, 0x2B) : Color.FromArgb(0xF9, 0xFF, 0xFF, 0xFF));
            resources["ControlFillColorInputActiveBrush"] = Brush(isDark ? Color.FromArgb(0x8C, 0x1A, 0x1E, 0x2B) : Color.FromArgb(0xF9, 0xFF, 0xFF, 0xFF));
            resources["ControlStrokeColorDefaultBrush"] = Brush(fieldStroke);
            resources["ControlStrokeColorSecondaryBrush"] = Brush(divider);
            resources["CardBackgroundFillColorDefaultBrush"] = Brush(card);
            resources["CardStrokeColorDefaultBrush"] = Brush(cardStroke);
        }

        private static LinearGradientBrush CreateGameGlassBrush(
            Brush ambientBrush,
            Color surfaceTop,
            Color surfaceBottom,
            double tint,
            double opacity)
        {
            var result = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };

            if (ambientBrush is GradientBrush gradient && gradient.GradientStops.Count > 0)
            {
                foreach (var stop in gradient.GradientStops)
                {
                    var offset = Math.Max(0, Math.Min(1, stop.Offset));
                    var surface = Blend(Opaque(surfaceTop), Opaque(surfaceBottom), offset);
                    var gameTint = Blend(surface, Opaque(stop.Color), tint);
                    result.GradientStops.Add(new GradientStop(WithAlpha(gameTint, opacity), offset));
                }
            }
            else
            {
                var fallback = ExtractUsableColor(ambientBrush) ?? Opaque(surfaceTop);
                result.GradientStops.Add(new GradientStop(
                    WithAlpha(Blend(Opaque(surfaceTop), fallback, tint), opacity), 0));
                result.GradientStops.Add(new GradientStop(
                    WithAlpha(Blend(Opaque(surfaceBottom), fallback, tint), opacity), 1));
            }

            result.Freeze();
            return result;
        }

        private static LinearGradientBrush CreateSettingsSurfaceBrush(
            Color top,
            Color bottom,
            double opacity)
        {
            var result = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            result.GradientStops.Add(new GradientStop(WithAlpha(Opaque(top), opacity), 0));
            result.GradientStops.Add(new GradientStop(WithAlpha(Opaque(bottom), opacity), 1));
            result.Freeze();
            return result;
        }

        private static LinearGradientBrush CreateSettingsAmbientBrush(
            AdaptiveThemePalette palette,
            bool enabled)
        {
            if (!enabled)
                return Gradient(Colors.Transparent, Colors.Transparent);

            var strength = Math.Max(0.2, Math.Min(1, palette.GlassStrength));
            var baseOpacity = palette.IsDark
                ? 0.24 + (0.20 * strength)
                : 0.10 + (0.14 * strength);
            var accent = Opaque(palette.Accent);
            var info = Opaque(palette.Info);
            var success = Opaque(palette.Success);
            var neutral = Blend(Opaque(palette.SurfaceTop), Opaque(palette.SurfaceBottom), 0.5);
            var brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            brush.GradientStops.Add(new GradientStop(WithAlpha(accent, baseOpacity), 0));
            brush.GradientStops.Add(new GradientStop(WithAlpha(info, baseOpacity * 0.84), 0.24));
            brush.GradientStops.Add(new GradientStop(
                WithAlpha(Blend(info, neutral, 0.42), baseOpacity * 0.70), 0.48));
            brush.GradientStops.Add(new GradientStop(
                WithAlpha(Blend(info, success, 0.46), baseOpacity * 0.56), 0.70));
            brush.GradientStops.Add(new GradientStop(
                WithAlpha(Blend(success, neutral, 0.48), baseOpacity * 0.42), 1));
            brush.Freeze();
            return brush;
        }

        private static LinearGradientBrush CanvasGradient(Color start, Color middle, Color end)
        {
            var brush = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 1) };
            brush.GradientStops.Add(new GradientStop(start, 0));
            brush.GradientStops.Add(new GradientStop(middle, 0.55));
            brush.GradientStops.Add(new GradientStop(end, 1));
            brush.Freeze();
            return brush;
        }

        public static SolidColorBrush Brush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        public static LinearGradientBrush Gradient(Color top, Color bottom)
        {
            var brush = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 1) };
            brush.GradientStops.Add(new GradientStop(top, 0));
            brush.GradientStops.Add(new GradientStop(bottom, 1));
            brush.Freeze();
            return brush;
        }

        private static DropShadowEffect? CreateShadowEffect(bool enabled, Color color, double blurRadius, double shadowDepth, double opacity)
        {
            if (!enabled) return null;
            var effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = blurRadius,
                ShadowDepth = shadowDepth,
                Opacity = opacity
            };
            effect.Freeze();
            return effect;
        }

        private static BlurEffect? CreateGameBackgroundBlurEffect(bool enabled, double glassStrength)
        {
            if (!enabled) return null;

            // The effect is restricted to the static game image layer. A 16–34 DIP range keeps
            // the selected artwork recognizable at low strength while making the default 78%
            // setting visibly diffuse instead of merely translucent.
            var effect = new BlurEffect
            {
                Radius = 12 + (22 * Math.Max(0.2, Math.Min(1, glassStrength))),
                RenderingBias = RenderingBias.Performance
            };
            effect.Freeze();
            return effect;
        }

        private static BlurEffect? CreateSettingsAmbientBlurEffect(bool enabled, double glassStrength)
        {
            if (!enabled) return null;

            var effect = new BlurEffect
            {
                Radius = 16 + (14 * Math.Max(0.2, Math.Min(1, glassStrength))),
                RenderingBias = RenderingBias.Performance
            };
            effect.Freeze();
            return effect;
        }

        private static LinearGradientBrush CreateSidebarMaterialBrush(AdaptiveThemePalette palette, bool glassEnabled)
        {
            if (!glassEnabled || SystemParameters.HighContrast)
                return Gradient(palette.SidebarTop, palette.SidebarBottom);

            // Use a diagonal, full-width material wash instead of a right-edge fade. The
            // sidebar gets a visible glass character across its whole area while its final
            // stop remains a normal surface, so the page boundary cannot become a light pillar.
            var sidebarBase = Blend(Opaque(palette.SidebarTop), Opaque(palette.SidebarBottom), 0.42);
            var sidebarTint = palette.IsDark
                ? Blend(sidebarBase, Colors.White, 0.065)
                : Blend(sidebarBase, Colors.Black, 0.028);
            var accentTint = Blend(sidebarTint, palette.Accent, palette.IsDark ? 0.16 : 0.055);
            var infoTint = Blend(sidebarTint, palette.Info, palette.IsDark ? 0.075 : 0.032);
            var neutralLift = palette.IsDark
                ? Blend(sidebarTint, Colors.White, 0.035)
                : Blend(sidebarTint, Colors.Black, 0.018);
            var brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            // Keep the navigation readable, but leave enough of the blurred backdrop visible
            // to make the rail a glass surface instead of a second opaque window.
            brush.GradientStops.Add(new GradientStop(WithAlpha(accentTint, palette.IsDark ? 0.72 : 0.78), 0));
            brush.GradientStops.Add(new GradientStop(WithAlpha(Blend(accentTint, sidebarTint, 0.48), palette.IsDark ? 0.68 : 0.74), 0.28));
            brush.GradientStops.Add(new GradientStop(WithAlpha(Blend(sidebarTint, infoTint, 0.40), palette.IsDark ? 0.64 : 0.70), 0.64));
            brush.GradientStops.Add(new GradientStop(WithAlpha(neutralLift, palette.IsDark ? 0.60 : 0.67), 1));
            brush.Freeze();
            return brush;
        }

        private static LinearGradientBrush CreateAmbientWideWashBrush(AdaptiveThemePalette palette, bool glassEnabled)
        {
            if (!glassEnabled || SystemParameters.HighContrast)
                return Gradient(Colors.Transparent, Colors.Transparent);

            var glassStrength = Math.Max(0.2, Math.Min(1, palette.GlassStrength));
            var accentWash = Opaque(palette.Accent);
            var infoWash = Opaque(palette.Info);
            var neutralWash = Blend(Opaque(palette.SurfaceTop), Opaque(palette.SurfaceBottom), 0.5);
            var tealWash = Blend(infoWash, Opaque(palette.Success), 0.52);
            var successWash = Opaque(palette.Success);
            var baseOpacity = palette.IsDark
                ? 0.10 + (0.20 * glassStrength)
                : 0.045 + (0.11 * glassStrength);
            var brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            // A long, multi-stop diagonal field creates a pane-wide color transition rather
            // than separate spotlights. Alpha remains bounded because this layer spans a page.
            brush.GradientStops.Add(new GradientStop(WithAlpha(accentWash, baseOpacity), 0));
            brush.GradientStops.Add(new GradientStop(WithAlpha(infoWash, baseOpacity * 0.82), 0.22));
            brush.GradientStops.Add(new GradientStop(WithAlpha(Blend(infoWash, neutralWash, 0.46), baseOpacity * 0.64), 0.43));
            brush.GradientStops.Add(new GradientStop(WithAlpha(tealWash, baseOpacity * 0.58), 0.65));
            brush.GradientStops.Add(new GradientStop(WithAlpha(successWash, baseOpacity * 0.46), 0.84));
            brush.GradientStops.Add(new GradientStop(WithAlpha(Blend(successWash, neutralWash, 0.52), baseOpacity * 0.32), 1));
            brush.Freeze();
            return brush;
        }

        private static Color ChooseBestText(Color background, Color? first, Color? second, bool darkBackground)
        {
            var candidates = new List<Color>();
            if (first.HasValue) candidates.Add(Opaque(first.Value));
            if (second.HasValue) candidates.Add(Opaque(second.Value));
            candidates.Add(darkBackground ? Colors.White : Colors.Black);
            var best = candidates[0];
            var bestRatio = ContrastRatio(best, background);
            for (var i = 1; i < candidates.Count; i++)
            {
                var ratio = ContrastRatio(candidates[i], background);
                if (ratio <= bestRatio) continue;
                best = candidates[i];
                bestRatio = ratio;
            }
            return best;
        }

        private static Color? ResolveHostBackground(FrameworkElement host)
        {
            DependencyObject? current = host;
            while (current != null)
            {
                var brush = current switch
                {
                    Border border => border.Background,
                    Panel panel => panel.Background,
                    Control control => control.Background,
                    _ => null
                };
                var color = ExtractUsableColor(brush);
                if (color.HasValue) return color;
                current = VisualTreeHelper.GetParent(current);
            }

            var window = Window.GetWindow(host);
            return ExtractUsableColor(window?.Background);
        }

        private static Color? ResolveFirstResourceColor(FrameworkElement host, IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                var color = ResolveResourceColor(host, key);
                if (color.HasValue) return color;
            }
            return null;
        }

        private static Color? ResolveResourceColor(FrameworkElement host, string key)
        {
            var color = ExtractUsableColor(host.TryFindResource(key) as Brush);
            if (color.HasValue) return color;

            // Settings can be presented by Playnite in a separate Window whose resource
            // dictionary is not part of the plugin UserControl's logical tree. Check the
            // actual owner and application scopes explicitly instead of relying on only one
            // WPF resource lookup path.
            var window = Window.GetWindow(host);
            color = ExtractUsableColor(window?.TryFindResource(key) as Brush);
            if (color.HasValue) return color;

            return ExtractUsableColor(Application.Current?.TryFindResource(key) as Brush);
        }

        private static Color? InferBackgroundFromHostText(Color? text, Color? inverseText)
        {
            if (text.HasValue)
            {
                var textLuminance = RelativeLuminance(text.Value);
                if (textLuminance <= 0.45) return Color.FromRgb(243, 244, 248);
                if (textLuminance >= 0.60) return Color.FromRgb(17, 19, 25);
            }

            if (inverseText.HasValue)
            {
                var inverseLuminance = RelativeLuminance(inverseText.Value);
                if (inverseLuminance >= 0.60) return Color.FromRgb(243, 244, 248);
                if (inverseLuminance <= 0.45) return Color.FromRgb(17, 19, 25);
            }

            return null;
        }

        private static Color? ExtractUsableColor(Brush? brush)
        {
            if (brush == null || brush.Opacity <= 0.08) return null;
            if (brush is SolidColorBrush solid && solid.Color.A > 24)
                return Opaque(solid.Color);

            if (brush is GradientBrush gradient && gradient.GradientStops.Count > 0)
            {
                double red = 0;
                double green = 0;
                double blue = 0;
                double totalWeight = 0;
                foreach (var stop in gradient.GradientStops)
                {
                    var weight = Math.Max(0.01, stop.Color.A / 255d);
                    red += stop.Color.R * weight;
                    green += stop.Color.G * weight;
                    blue += stop.Color.B * weight;
                    totalWeight += weight;
                }
                if (totalWeight > 0)
                    return Color.FromRgb((byte)(red / totalWeight), (byte)(green / totalWeight), (byte)(blue / totalWeight));
            }

            return null;
        }

        private static Color Blend(Color source, Color target, double amount)
        {
            amount = Math.Max(0, Math.Min(1, amount));
            return Color.FromRgb(
                (byte)Math.Round(source.R + (target.R - source.R) * amount),
                (byte)Math.Round(source.G + (target.G - source.G) * amount),
                (byte)Math.Round(source.B + (target.B - source.B) * amount));
        }

        private static Color Opaque(Color color) => Color.FromRgb(color.R, color.G, color.B);

        private static Color EnsureContrast(Color candidate, Color background, bool darkBackground)
        {
            candidate = Opaque(candidate);
            var target = darkBackground ? Colors.White : Colors.Black;
            for (var attempt = 0; attempt < 6 && ContrastRatio(candidate, background) < 3; attempt++)
                candidate = Blend(candidate, target, 0.18);
            return candidate;
        }

        private static Color WithAlpha(Color color, double opacity)
        {
            var alpha = (byte)Math.Round(Math.Max(0, Math.Min(1, opacity)) * 255);
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        private static Color SemanticTint(Color color, double opacity)
        {
            // High-contrast themes do not reliably render translucent fills. Use the solid
            // semantic color there so the associated border/status remains visible.
            return SystemParameters.HighContrast ? color : WithAlpha(color, opacity);
        }

        private static double ContrastRatio(Color first, Color second)
        {
            var firstLuminance = RelativeLuminance(first);
            var secondLuminance = RelativeLuminance(second);
            var lighter = Math.Max(firstLuminance, secondLuminance);
            var darker = Math.Min(firstLuminance, secondLuminance);
            return (lighter + 0.05) / (darker + 0.05);
        }

        private static double RelativeLuminance(Color color)
        {
            double Convert(byte channel)
            {
                var value = channel / 255.0;
                return value <= 0.03928 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
            }

            return 0.2126 * Convert(color.R) + 0.7152 * Convert(color.G) + 0.0722 * Convert(color.B);
        }
    }
}
