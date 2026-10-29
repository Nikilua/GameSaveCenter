from pathlib import Path
import json,hashlib,difflib
p=Path(__file__).resolve().parent
changes=[]
for side in ('before','after'):
 root=p/side/'tests/GameSaveCenter.RenderHarness'
 f=root/'Program.cs';original=f.read_text(encoding='utf-8-sig');text=original
 old='            void RunActionCycle()\n            {\n                var cycleStopwatch = Stopwatch.StartNew();'
 new=old+'\n                var allocationStart = AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize;'
 assert text.count(old)==1
 text=text.replace(old,new)
 old='                    var durationMs = cycleStopwatch.Elapsed.TotalMilliseconds;'
 new=old+'\n                    U09Performance.RecordCycle(currentPage, currentTheme, cycle, durationMs, AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize - allocationStart);'
 assert text.count(old)==1
 text=text.replace(old,new);f.write_text(text,encoding='utf-8',newline='\n')
 probe=root/'U09Performance.cs';raw=probe.read_bytes();t=raw.decode('utf-8')
 t=t.replace(' private static bool Settling;', ' private static bool Settling;\n private static readonly StringBuilder CycleRows = new StringBuilder("page,theme,cycle,duration_ms,allocated_bytes\\n");\n public static void RecordCycle(string page,string theme,int cycle,double duration,long bytes){CycleRows.AppendLine(page+","+theme+","+cycle+","+duration.ToString("F6",CultureInfo.InvariantCulture)+","+bytes);}')
 t=t.replace('  CompositionTarget.Rendering-=Render;', '  File.WriteAllText(Path.Combine(output,"page-cycles.csv"),CycleRows.ToString());\n  CompositionTarget.Rendering-=Render;')
 probe.write_text(t,encoding='utf-8',newline='\n')
 changes.append({'side':side,'programDiff':'\n'.join(difflib.unified_diff(original.splitlines(),text.splitlines())),'probeOriginalSha256':hashlib.sha256(raw).hexdigest(),'probeSha256':hashlib.sha256(probe.read_bytes()).hexdigest()})
(p/'page-diagnostic-patch.json').write_text(json.dumps(changes,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print('Prepared only diagnostic per-cycle allocations; production source unchanged')
