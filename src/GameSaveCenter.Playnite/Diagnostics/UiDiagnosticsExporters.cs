using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace GameSaveCenter.Playnite.Diagnostics
{
    /// <summary>
    /// Developer-only exporters used by the real Playnite host fidelity audit. They emit
    /// resolved effective values (not raw XAML) and deliberately omit text content so the
    /// output can be shared without leaking user paths or account data.
    /// </summary>
    internal static class UiDiagnosticsExporters
    {
        public static void WriteJson(object value, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            File.WriteAllText(path, JsonConvert.SerializeObject(value, Formatting.Indented));
        }

        public static List<UiResourceRecord> BuildResourceSnapshot(ResourceDictionary dictionary, string scopeName)
        {
            var records = new List<UiResourceRecord>();
            var keys = new[]
            {
                "GscUiFontFamily", "GscCodeFontFamily",
                "GscBackgroundBrush", "GscSurfaceBrush", "GscSurfaceRaisedBrush",
                "GscGlassBrush", "GscGlassStrongBrush", "GscGlassFillBrush",
                "GscGlassStrongBrush", "GscBackdropBrush", "GscControlFillBrush", "GscControlStrokeBrush",
                "GscTableHeaderBrush", "GscTableDividerBrush",
                "GscPrimaryTextBrush", "GscSecondaryTextBrush", "GscMutedTextBrush",
                "GscAccentBrush", "GscAccentTintBrush", "GscAccentTintStrongBrush",
                "GscPrimaryButtonBrush", "GscPrimaryButtonBorderBrush",
                "GscSurfaceEffect", "GscDialogEffect"
            };

            foreach (var key in keys.Distinct())
            {
                if (!dictionary.Contains(key))
                    continue;
                var value = dictionary[key];
                records.Add(new UiResourceRecord
                {
                    Key = key,
                    ValueType = value?.GetType().Name ?? "null",
                    Scope = scopeName,
                    IsLocalToScope = true,
                    BrushSummary = SummarizeBrush(value as Brush),
                    EffectType = value is Effect effect ? effect.GetType().Name : string.Empty
                });
            }

            return records;
        }

        public static List<UiStyleFingerprint> BuildStyleFingerprints(DependencyObject root, int maxNodes = 4000)
        {
            var fingerprints = new List<UiStyleFingerprint>();
            var counts = new Dictionary<Type, int>();
            Traverse(root, 0, maxNodes, fingerprints, counts);
            return fingerprints;
        }

        public static List<UiVisualNode> BuildVisualTree(Visual root, int maxNodes = 3000)
        {
            var nodes = new List<UiVisualNode>();
            TraverseVisual(root, 0, maxNodes, nodes);
            return nodes;
        }

        public static void SavePng(Visual visual, string path)
        {
            if (!(visual is FrameworkElement element) || element.ActualWidth <= 0 || element.ActualHeight <= 0)
                throw new InvalidOperationException($"Cannot render audit PNG for {path}: empty size.");
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            var dpi = VisualTreeHelper.GetDpi(visual);
            var bitmap = new RenderTargetBitmap(
                (int)Math.Ceiling(element.ActualWidth),
                (int)Math.Ceiling(element.ActualHeight),
                dpi.PixelsPerInchX,
                dpi.PixelsPerInchY,
                PixelFormats.Pbgra32);
            bitmap.Render(visual);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = File.Create(path);
            encoder.Save(stream);
        }

        private static void Traverse(
            DependencyObject current,
            int depth,
            int maxNodes,
            List<UiStyleFingerprint> fingerprints,
            Dictionary<Type, int> counts)
        {
            if (fingerprints.Count >= maxNodes)
                return;

            if (current is FrameworkElement element
                && element.Visibility == Visibility.Visible
                && element.ActualWidth > 0
                && element.ActualHeight > 0)
            {
                var type = element.GetType();
                var limit = LimitFor(type);
                if (limit > 0)
                {
                    counts.TryGetValue(type, out var seen);
                    if (seen < limit)
                    {
                        counts[type] = seen + 1;
                        fingerprints.Add(BuildFingerprint(element));
                    }
                }
            }

            var childCount = VisualTreeHelper.GetChildrenCount(current);
            for (var i = 0; i < childCount; i++)
                Traverse(VisualTreeHelper.GetChild(current, i), depth + 1, maxNodes, fingerprints, counts);
        }

        private static int LimitFor(Type type)
        {
            if (type == typeof(TextBlock)) return 6;
            if (typeof(ButtonBase).IsAssignableFrom(type)) return 6;
            if (type == typeof(TextBox)) return 4;
            if (type == typeof(ComboBox)) return 4;
            if (type == typeof(TabItem)) return 4;
            if (type == typeof(DataGrid)) return 3;
            if (type == typeof(DataGridColumnHeader)) return 4;
            if (type == typeof(DataGridRow)) return 3;
            if (type == typeof(DataGridCell)) return 3;
            if (typeof(ScrollBar).IsAssignableFrom(type)) return 3;
            if (type == typeof(Border)) return 8;
            return 0;
        }

        private static UiStyleFingerprint BuildFingerprint(FrameworkElement element)
        {
            var control = element as Control;
            var border = element as Border;
            var text = element as TextBlock;
            return new UiStyleFingerprint
            {
                Type = element.GetType().Name,
                Name = element.Name,
                AutomationId = System.Windows.Automation.AutomationProperties.GetAutomationId(element),
                FontFamily = text?.FontFamily?.Source ?? control?.FontFamily?.Source ?? string.Empty,
                FontSize = text?.FontSize ?? control?.FontSize ?? 0,
                FontWeight = text?.FontWeight ?? control?.FontWeight ?? FontWeights.Normal,
                ForegroundArgb = SummarizeBrush(control?.Foreground ?? text?.Foreground),
                BackgroundArgb = SummarizeBrush(border?.Background ?? control?.Background),
                BorderBrushArgb = SummarizeBrush(border?.BorderBrush ?? control?.BorderBrush),
                BorderThickness = (border?.BorderThickness ?? control?.BorderThickness)?.ToString() ?? string.Empty,
                Padding = (border?.Padding ?? control?.Padding)?.ToString() ?? string.Empty,
                Margin = element.Margin.ToString(),
                Opacity = element.Opacity,
                EffectType = element.Effect?.GetType().Name ?? "None",
                MinWidth = element.MinWidth,
                MinHeight = element.MinHeight,
                ActualWidth = element.ActualWidth,
                ActualHeight = element.ActualHeight,
                TextFormattingMode = TextOptions.GetTextFormattingMode(element).ToString(),
                TextRenderingMode = TextOptions.GetTextRenderingMode(element).ToString(),
                TextHintingMode = TextOptions.GetTextHintingMode(element).ToString(),
                ClearTypeHint = RenderOptions.GetClearTypeHint(element).ToString(),
                SnapsToDevicePixels = element.SnapsToDevicePixels,
                UseLayoutRounding = element.UseLayoutRounding
            };
        }

        private static void TraverseVisual(Visual current, int depth, int maxNodes, List<UiVisualNode> nodes)
        {
            if (nodes.Count >= maxNodes)
                return;
            if (current is FrameworkElement element
                && element.Visibility == Visibility.Visible
                && element.ActualWidth > 0
                && element.ActualHeight > 0)
            {
                nodes.Add(new UiVisualNode
                {
                    Type = element.GetType().Name,
                    Name = element.Name,
                    Depth = depth,
                    ActualWidth = Math.Round(element.ActualWidth, 2),
                    ActualHeight = Math.Round(element.ActualHeight, 2)
                });
            }

            var childCount = VisualTreeHelper.GetChildrenCount(current);
            for (var i = 0; i < childCount; i++)
            {
                if (VisualTreeHelper.GetChild(current, i) is Visual child)
                    TraverseVisual(child, depth + 1, maxNodes, nodes);
            }
        }

        private static string SummarizeBrush(Brush? brush)
        {
            if (brush == null)
                return string.Empty;
            if (brush is SolidColorBrush solid)
                return solid.Color.ToString();
            if (brush is GradientBrush gradient && gradient.GradientStops.Count > 0)
            {
                var stops = gradient.GradientStops
                    .Select(stop => $"{stop.Offset:0.00}:{stop.Color}")
                    .ToArray();
                return gradient.GetType().Name + "[" + string.Join(";", stops) + "]";
            }
            return brush.GetType().Name;
        }
    }

    internal sealed class UiResourceRecord
    {
        public string Key { get; set; } = string.Empty;
        public string ValueType { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public bool IsLocalToScope { get; set; }
        public string BrushSummary { get; set; } = string.Empty;
        public string EffectType { get; set; } = string.Empty;
    }

    internal sealed class UiStyleFingerprint
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AutomationId { get; set; } = string.Empty;
        public string FontFamily { get; set; } = string.Empty;
        public double FontSize { get; set; }
        public FontWeight FontWeight { get; set; }
        public string ForegroundArgb { get; set; } = string.Empty;
        public string BackgroundArgb { get; set; } = string.Empty;
        public string BorderBrushArgb { get; set; } = string.Empty;
        public string BorderThickness { get; set; } = string.Empty;
        public string Padding { get; set; } = string.Empty;
        public string Margin { get; set; } = string.Empty;
        public double Opacity { get; set; }
        public string EffectType { get; set; } = "None";
        public double MinWidth { get; set; }
        public double MinHeight { get; set; }
        public double ActualWidth { get; set; }
        public double ActualHeight { get; set; }
        public string TextFormattingMode { get; set; } = string.Empty;
        public string TextRenderingMode { get; set; } = string.Empty;
        public string TextHintingMode { get; set; } = string.Empty;
        public string ClearTypeHint { get; set; } = string.Empty;
        public bool SnapsToDevicePixels { get; set; }
        public bool UseLayoutRounding { get; set; }
    }

    internal sealed class UiVisualNode
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Depth { get; set; }
        public double ActualWidth { get; set; }
        public double ActualHeight { get; set; }
    }
}
