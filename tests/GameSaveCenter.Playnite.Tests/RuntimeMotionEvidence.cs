using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace GameSaveCenter.Playnite.Tests;

/// <summary>
/// Optional, timestamped samples from real WPF CompositionTarget.Rendering callbacks.
/// Set GSC_MOTION_EVIDENCE_PATH to write; ordinary test runs stay side-effect free.
/// </summary>
internal sealed class RuntimeMotionEvidence : IDisposable
{
    private const string Header = "capture_id,phase,source,elapsed_ms,frame,opacity,x_dip,y_dip,visibility,is_visible,is_hit_test_visible";
    private readonly string? outputPath;
    private readonly string captureId;
    private readonly Stopwatch stopwatch = Stopwatch.StartNew();
    private readonly List<string> rows = new List<string>();
    private FrameworkElement? target;
    private string phase = "untracked";
    private int frame;
    private bool disposed;

    public RuntimeMotionEvidence(string scenario)
    {
        outputPath = Environment.GetEnvironmentVariable("GSC_MOTION_EVIDENCE_PATH");
        captureId = $"{scenario}-{DateTime.UtcNow:yyyyMMddTHHmmssfffZ}";
        if (!string.IsNullOrWhiteSpace(outputPath))
            CompositionTarget.Rendering += OnRendering;
        else
            outputPath = null;
    }

    public bool Enabled => outputPath != null;

    public void Track(string nextPhase, FrameworkElement element)
    {
        if (!Enabled)
            return;

        phase = nextPhase;
        target = element;
        Capture("state");
    }

    public void Stop()
        => target = null;

    private void OnRendering(object? sender, EventArgs e)
        => Capture("rendering");

    private void Capture(string source)
    {
        if (target == null || disposed)
            return;

        var offset = target.RenderTransform.Transform(new Point(0, 0));
        rows.Add(string.Join(",", new[]
        {
            Csv(captureId),
            Csv(phase),
            source,
            stopwatch.Elapsed.TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture),
            (++frame).ToString(CultureInfo.InvariantCulture),
            target.Opacity.ToString("F4", CultureInfo.InvariantCulture),
            offset.X.ToString("F3", CultureInfo.InvariantCulture),
            offset.Y.ToString("F3", CultureInfo.InvariantCulture),
            target.Visibility.ToString(),
            target.IsVisible ? "true" : "false",
            target.IsHitTestVisible ? "true" : "false"
        }));
    }

    private static string Csv(string value)
        => "\"" + value.Replace("\"", "\"\"") + "\"";

    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;
        if (outputPath == null || rows.Count == 0)
            return;

        var fullPath = Path.GetFullPath(outputPath);
        var parent = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(parent))
            Directory.CreateDirectory(parent);

        var writeHeader = !File.Exists(fullPath) || new FileInfo(fullPath).Length == 0;
        using (var writer = new StreamWriter(
            new FileStream(fullPath, FileMode.Append, FileAccess.Write, FileShare.Read),
            new UTF8Encoding(false)))
        {
            if (writeHeader)
                writer.WriteLine(Header);
            foreach (var row in rows)
                writer.WriteLine(row);
        }
    }
}
