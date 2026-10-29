from pathlib import Path
import json,difflib,hashlib
p=Path(__file__).resolve().parent;patches=[]
for side in ('before','after'):
 root=p/side/'tests/GameSaveCenter.RenderHarness';f=root/'Program.cs';original=f.read_text(encoding='utf-8-sig');t=original
 start=t.index('            void LayoutCurrentPage(');end=t.index('            void MeasureAction(',start);body=t[start:end]
 old='                ApplyThemePalette(shell, themeMode, glassEnabled: true, motionEnabled: true);'
 assert body.count(old)==1
 body=body.replace(old,'                var partBytes = U09Performance.Allocated;\n                var partTicks = Stopwatch.GetTimestamp();\n'+old)
 old='                pageHost.Content = page;'
 assert body.count(old)==1
 body=body.replace(old,'                U09Performance.RecordPart("theme", page.GetType().Name, themeMode.ToString(), partBytes, partTicks);\n                partBytes = U09Performance.Allocated;\n                partTicks = Stopwatch.GetTimestamp();\n'+old)
 old='                window.UpdateLayout();\n            }'
 assert body.count(old)==1
 body=body.replace(old,'                window.UpdateLayout();\n                U09Performance.RecordPart("layout", page.GetType().Name, themeMode.ToString(), partBytes, partTicks);\n            }')
 t=t[:start]+body+t[end:];f.write_text(t,encoding='utf-8',newline='\n')
 probe=root/'U09Performance.cs';oldprobe=probe.read_text(encoding='utf-8');v=oldprobe
 v=v.replace(' private static bool Settling;',' private static bool Settling;\n private static readonly StringBuilder PartRows=new StringBuilder("part,page,theme,duration_ms,allocated_bytes\\n");\n public static long Allocated => AppDomain.MonitoringIsEnabled ? AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize : 0;\n public static void RecordPart(string part,string page,string theme,long bytes,long ticks){if(!Clock.IsRunning||Settling)return;PartRows.AppendLine(part+","+page+","+theme+","+((Stopwatch.GetTimestamp()-ticks)*1000d/Stopwatch.Frequency).ToString("F6",CultureInfo.InvariantCulture)+","+(Allocated-bytes));}')
 v=v.replace('  File.WriteAllText(Path.Combine(output,"page-cycles.csv"),CycleRows.ToString());','  File.WriteAllText(Path.Combine(output,"parts.csv"),PartRows.ToString());\n  File.WriteAllText(Path.Combine(output,"page-cycles.csv"),CycleRows.ToString());')
 probe.write_text(v,encoding='utf-8',newline='\n');patches.append({'side':side,'programDiff':'\n'.join(difflib.unified_diff(original.splitlines(),t.splitlines())),'probeDiff':'\n'.join(difflib.unified_diff(oldprobe.splitlines(),v.splitlines())),'probeSha256':hashlib.sha256(probe.read_bytes()).hexdigest()})
(p/'parts-patch.json').write_text(json.dumps(patches,ensure_ascii=False,indent=2)+'\n',encoding='utf-8',newline='\n')
print('Prepared separate theme-apply/layout costs; production source unchanged')
