from pathlib import Path
import hashlib,json,difflib,zipfile
p=Path(__file__).resolve().parent;patches=[]
for side in ('before','after'):
 root=p/side;f=root/'tests/GameSaveCenter.RenderHarness/Program.cs';raw=f.read_bytes();old=raw.decode('utf-8-sig').replace('\r\n','\n');new=old
 assert old.count('public static class Program')==1
 new=new.replace('public static class Program','public static partial class Program',1)
 entry='    public static int Main(string[] args)\n    {'
 assert new.count(entry)==1
 new=new.replace(entry,entry+'\n        if (args.Length > 0 && args[0] == "u04pairprobe")\n            return RunU04PairEntry(Path.GetFullPath(args[1]));',1)
 f.write_text(new,encoding='utf-8',newline='\n')
 probe=f.parent/'U04PairProbe.cs';probe.write_bytes((p/probe.name).read_bytes())
 with zipfile.ZipFile(p/(side+'.zip')) as z:
  src=[n for n in z.namelist() if n.startswith('src/') and not n.endswith('/')];assert all(z.read(n)==(root/n).read_bytes() for n in src)
 patches.append({'side':side,'programOriginalSha256':hashlib.sha256(raw).hexdigest(),'programInstrumentedSha256':hashlib.sha256(f.read_bytes()).hexdigest(),'programDiff':'\n'.join(difflib.unified_diff(old.splitlines(),new.splitlines())),'probeSha256':hashlib.sha256(probe.read_bytes()).hexdigest(),'productionFilesUnchanged':len(src)})
(p/'probe-instrumentation.json').write_text(json.dumps(patches,ensure_ascii=False,indent=2)+'\n',encoding='utf-8',newline='\n')
print('Historical harness instrumentation prepared; no compile/capture yet; current tracked source untouched')
