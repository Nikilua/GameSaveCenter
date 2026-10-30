using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace GameSaveCenter.RenderHarness;

internal static class U09Performance
{
    private readonly struct Frame
    {
        public Frame(long tick, bool settle)
        {
            Tick = tick;
            Settle = settle;
        }

        public long Tick { get; }
        public bool Settle { get; }
    }

    private static readonly List<Frame> Frames = new(50000);
    private static readonly StringBuilder CycleRows = new("page,theme,cycle,duration_ms,allocated_bytes\n");
    private static readonly StringBuilder PartRows = new("part,page,theme,duration_ms,allocated_bytes,start_seconds,end_seconds,gc0_delta,gc1_delta,gc2_delta\n");
    private static readonly StringBuilder ActionRows = new("action,page,theme,duration_ms,allocated_bytes\n");
    private static readonly Stopwatch Clock = new();
    private static long ClockStartTimestamp;
    private static long StartBytes;
    private static long ActiveBytes;
    private static long ActiveTicks;
    private static bool Settling;
    private static int Cycles;
    private static int Actions;

    public static long Allocated => Clock.IsRunning ? AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize : 0;

    public static void Begin()
    {
        AppDomain.MonitoringIsEnabled = true;
        Frames.Clear();
        CycleRows.Clear();
        CycleRows.AppendLine("page,theme,cycle,duration_ms,allocated_bytes");
        PartRows.Clear();
        PartRows.AppendLine("part,page,theme,duration_ms,allocated_bytes,start_seconds,end_seconds,gc0_delta,gc1_delta,gc2_delta");
        ActionRows.Clear();
        ActionRows.AppendLine("action,page,theme,duration_ms,allocated_bytes");
        Settling = false;
        Clock.Restart();
        ClockStartTimestamp = Stopwatch.GetTimestamp();
        CompositionTarget.Rendering += OnRendering;
        StartBytes = AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize;
    }

    public static void RecordCycle(string page, string theme, int cycle, double durationMilliseconds, long allocatedBytes)
    {
        CycleRows.AppendLine(string.Join(
            ",",
            page,
            theme,
            cycle.ToString(CultureInfo.InvariantCulture),
            durationMilliseconds.ToString("F6", CultureInfo.InvariantCulture),
            allocatedBytes.ToString(CultureInfo.InvariantCulture)));
    }

    public static void RecordPart(
        string part,
        string page,
        string theme,
        long bytesBefore,
        long ticksBefore,
        int gc0Before,
        int gc1Before,
        int gc2Before)
    {
        if (!Clock.IsRunning)
            return;

        var endTicks = Stopwatch.GetTimestamp();
        var allocatedBytes = AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize - bytesBefore;
        var durationMilliseconds = (endTicks - ticksBefore) * 1000d / Stopwatch.Frequency;
        var startSeconds = (ticksBefore - ClockStartTimestamp) / (double)Stopwatch.Frequency;
        var endSeconds = (endTicks - ClockStartTimestamp) / (double)Stopwatch.Frequency;
        PartRows.AppendLine(string.Join(
            ",",
            part,
            page,
            theme,
            durationMilliseconds.ToString("F6", CultureInfo.InvariantCulture),
            allocatedBytes.ToString(CultureInfo.InvariantCulture),
            startSeconds.ToString("F6", CultureInfo.InvariantCulture),
            endSeconds.ToString("F6", CultureInfo.InvariantCulture),
            (GC.CollectionCount(0) - gc0Before).ToString(CultureInfo.InvariantCulture),
            (GC.CollectionCount(1) - gc1Before).ToString(CultureInfo.InvariantCulture),
            (GC.CollectionCount(2) - gc2Before).ToString(CultureInfo.InvariantCulture)));
    }

    public static void RecordAction(string action, string page, string theme, double durationMilliseconds, long allocatedBytes)
    {
        ActionRows.AppendLine(string.Join(
            ",",
            action,
            page,
            theme,
            durationMilliseconds.ToString("F6", CultureInfo.InvariantCulture),
            allocatedBytes.ToString(CultureInfo.InvariantCulture)));
    }

    public static void Stop(int cycles, int actions)
    {
        ActiveBytes = AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize - StartBytes;
        ActiveTicks = Clock.ElapsedTicks;
        Cycles = cycles;
        Actions = actions;
        Settling = true;
    }

    public static void End(string outputRoot)
    {
        CompositionTarget.Rendering -= OnRendering;
        var totalBytes = AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize - StartBytes;
        Clock.Stop();

        var frameRows = new StringBuilder("seconds,gap_ms,phase\n");
        var activeGaps = new List<double>();
        var settleGaps = new List<double>();
        for (var i = 0; i < Frames.Count; i++)
        {
            var seconds = Frames[i].Tick / (double)Stopwatch.Frequency;
            var gap = i == 0 ? 0 : (Frames[i].Tick - Frames[i - 1].Tick) * 1000d / Stopwatch.Frequency;
            var phase = i == 0
                ? "first"
                : Frames[i].Settle != Frames[i - 1].Settle
                    ? "boundary"
                    : Frames[i].Settle ? "settle" : "active";
            frameRows.AppendLine(string.Join(
                ",",
                seconds.ToString("F6", CultureInfo.InvariantCulture),
                gap.ToString("F6", CultureInfo.InvariantCulture),
                phase));
            if (phase == "active")
                activeGaps.Add(gap);
            else if (phase == "settle")
                settleGaps.Add(gap);
        }

        File.WriteAllText(Path.Combine(outputRoot, "performance-frames.csv"), frameRows.ToString());
        File.WriteAllText(Path.Combine(outputRoot, "page-cycles.csv"), CycleRows.ToString());
        File.WriteAllText(Path.Combine(outputRoot, "parts.csv"), PartRows.ToString());
        File.WriteAllText(Path.Combine(outputRoot, "actions.csv"), ActionRows.ToString());

        var report = new StringBuilder("Fixed-cycle supplementary WPF diagnostic; not original historical measurements or presented/VSync frames\n");
        report.AppendLine($"Cycles={Cycles}; CompletedActions={Actions}; FixedCycleTarget=70; no forced GC");
        report.AppendLine($"ActiveSeconds={ActiveTicks / (double)Stopwatch.Frequency:F6}; TotalSeconds={Clock.Elapsed.TotalSeconds:F6}");
        report.AppendLine("ManagedAllocationCounter=AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize; native/other-AppDomain allocations excluded");
        report.AppendLine("Per-operation timestamps use Stopwatch.GetTimestamp relative to Begin; GC columns are per-operation GC.CollectionCount deltas.");
        report.AppendLine($"ActiveAllocatedBytes={ActiveBytes}; TotalAllocatedBytes={totalBytes}; ActiveBytesPerCycle={(Cycles == 0 ? double.NaN : ActiveBytes / (double)Cycles):F3}; ActiveBytesPerCompletedAction={(Actions == 0 ? double.NaN : ActiveBytes / (double)Actions):F3}");
        AppendFrameSummary(report, "active", activeGaps);
        AppendFrameSummary(report, "settle", settleGaps);
        File.WriteAllText(Path.Combine(outputRoot, "performance-summary.txt"), report.ToString());
    }

    private static void OnRendering(object? sender, EventArgs args)
    {
        Frames.Add(new Frame(Clock.ElapsedTicks, Settling));
    }

    private static void AppendFrameSummary(StringBuilder report, string phase, List<double> values)
    {
        values.Sort();
        double Percentile(double percentile) => values.Count == 0 ? double.NaN : values[(int)Math.Ceiling(percentile * values.Count) - 1];
        report.AppendLine($"Phase={phase}; Samples={values.Count}; P50Ms={Percentile(0.50):F3}; P95Ms={Percentile(0.95):F3}; MaxMs={(values.Count == 0 ? double.NaN : values[values.Count - 1]):F3}; Over16_7Ms={values.Count(value => value > 16.7)}; Over33_3Ms={values.Count(value => value > 33.3)}");
    }
}