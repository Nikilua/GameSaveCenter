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
 private struct Frame { public long Tick; public bool Settle; }
 private static readonly List<Frame> Frames=new List<Frame>(50000);
 private static readonly Stopwatch Clock=new Stopwatch();
 private static long StartBytes,ActiveBytes,ActiveTick;
 private static bool Settling;
 private static int Cycles,Actions;
 public static void Begin(){AppDomain.MonitoringIsEnabled=true;Frames.Clear();Settling=false;Clock.Restart();CompositionTarget.Rendering+=Render;StartBytes=AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize;}
 private static void Render(object? sender,EventArgs args){Frames.Add(new Frame{Tick=Clock.ElapsedTicks,Settle=Settling});}
 public static void Stop(int cycles,int actions){ActiveBytes=AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize-StartBytes;ActiveTick=Clock.ElapsedTicks;Cycles=cycles;Actions=actions;Settling=true;}
 public static void End(string output){
  CompositionTarget.Rendering-=Render;var totalBytes=AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize-StartBytes;Clock.Stop();
  var rows=new StringBuilder("seconds,gap_ms,phase\n");var active=new List<double>();var settle=new List<double>();
  for(int i=0;i<Frames.Count;i++){double seconds=Frames[i].Tick/(double)Stopwatch.Frequency;double gap=i==0?0:(Frames[i].Tick-Frames[i-1].Tick)*1000d/Stopwatch.Frequency;string phase=i==0?"first":Frames[i].Settle!=Frames[i-1].Settle?"boundary":Frames[i].Settle?"settle":"active";rows.AppendLine(seconds.ToString("F6",CultureInfo.InvariantCulture)+","+gap.ToString("F6",CultureInfo.InvariantCulture)+","+phase);if(phase=="active")active.Add(gap);else if(phase=="settle")settle.Add(gap);}
  File.WriteAllText(Path.Combine(output,"performance-frames.csv"),rows.ToString());
  var report=new StringBuilder("Reconstructed fixed-cycle supplementary WPF diagnostic; not original historical measurements or presented/VSync frames\n");
  report.AppendLine("Cycles="+Cycles+"; CompletedActions="+Actions+"; FixedCycleTarget=70; ActionFailures remain governed by original endurance probe");
  report.AppendLine("ActiveSeconds="+(ActiveTick/(double)Stopwatch.Frequency).ToString("F6",CultureInfo.InvariantCulture)+"; TotalSeconds="+Clock.Elapsed.TotalSeconds.ToString("F6",CultureInfo.InvariantCulture));
  report.AppendLine("ManagedAllocationCounter=AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize; native/other-AppDomain allocations excluded; no forced GC");
  report.AppendLine("ActiveAllocatedBytes="+ActiveBytes+"; TotalAllocatedBytes="+totalBytes+"; ActiveBytesPerCycle="+(ActiveBytes/(double)Cycles).ToString("F3",CultureInfo.InvariantCulture)+"; ActiveBytesPerCompletedAction="+(ActiveBytes/(double)Actions).ToString("F3",CultureInfo.InvariantCulture));
  Summary(report,"active",active);Summary(report,"settle",settle);File.WriteAllText(Path.Combine(output,"performance-summary.txt"),report.ToString());
 }
 private static void Summary(StringBuilder report,string phase,List<double> values){values.Sort();double P(double p)=>values.Count==0?double.NaN:values[(int)Math.Ceiling(p*values.Count)-1];report.AppendLine("Phase="+phase+"; Samples="+values.Count+"; P50Ms="+P(.5).ToString("F3",CultureInfo.InvariantCulture)+"; P95Ms="+P(.95).ToString("F3",CultureInfo.InvariantCulture)+"; MaxMs="+(values.Count==0?double.NaN:values[values.Count-1]).ToString("F3",CultureInfo.InvariantCulture)+"; Over16_7Ms="+values.Count(x=>x>16.7)+"; Over33_3Ms="+values.Count(x=>x>33.3));}
}
