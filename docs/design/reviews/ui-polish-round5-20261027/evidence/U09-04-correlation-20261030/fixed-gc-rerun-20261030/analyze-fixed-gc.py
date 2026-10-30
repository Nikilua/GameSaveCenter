from pathlib import Path
import csv,hashlib,json,math
BASE=Path(__file__).resolve().parent
def read_csv(path):
 with path.open(encoding='utf-8-sig',newline='') as f:return list(csv.DictReader(f))
def percentile(v,p):
 a=sorted(v);return a[max(0,math.ceil(p*len(a))-1)] if a else None
def pearson(x,y):
 if len(x)<2 or len(x)!=len(y):return None
 mx=sum(x)/len(x);my=sum(y)/len(y);den=math.sqrt(sum((a-mx)**2 for a in x)*sum((b-my)**2 for b in y))
 return sum((a-mx)*(b-my) for a,b in zip(x,y))/den if den else None
def stats(rows):
 d=[float(r['durationMs']) for r in rows]; al=[int(r['allocatedBytes']) for r in rows]
 g0=[int(r['gc0']) for r in rows];g1=[int(r['gc1']) for r in rows];g2=[int(r['gc2']) for r in rows]
 anygc=[int(a+b+c>0) for a,b,c in zip(g0,g1,g2)]
 over=[r for r in rows if float(r['durationMs'])>300]
 return {'samples':len(rows),'p50Ms':percentile(d,.5),'p95Ms':percentile(d,.95),'maxMs':max(d,default=None),'over100Ms':sum(x>100 for x in d),'over300Ms':len(over),'pearsonDurationVsAllocatedBytes':pearson(d,al),'pearsonDurationVsGc0Delta':pearson(d,g0),'pearsonDurationVsAnyGc':pearson(d,anygc),'operationsWithAnyGc':sum(anygc),'operationsWithGen0':sum(x>0 for x in g0),'operationsWithGen1':sum(x>0 for x in g1),'operationsWithGen2':sum(x>0 for x in g2),'p95NoGcMs':percentile([x for x,g in zip(d,anygc) if not g],.95),'p95WithGcMs':percentile([x for x,g in zip(d,anygc) if g],.95),'over300WithAnyGc':sum(int(r['gc0'])+int(r['gc1'])+int(r['gc2'])>0 for r in over),'over300WithRenderingGapOver100Ms':sum(float(r.get('maxOverlappingRenderingGapMs',0))>100 for r in over),'over300WithRenderingGapOver300Ms':sum(float(r.get('maxOverlappingRenderingGapMs',0))>300 for r in over),'outliers':[{'page':r['page'],'theme':r['theme'],'durationMs':float(r['durationMs']),'allocatedBytes':int(r['allocatedBytes']),'gc0':int(r['gc0']),'gc1':int(r['gc1']),'gc2':int(r['gc2']),'maxOverlappingRenderingGapMs':float(r.get('maxOverlappingRenderingGapMs',0))} for r in over]}
all_theme=[];all_layout=[];all_total=[];all_frames=[];runs=[]
for i in (1,2,3):
 folder=BASE/f'run-{i}';parts=read_csv(folder/'parts.csv');frames=read_csv(folder/'performance-frames.csv')
 assert len(parts)==140 and len(parts)%2==0
 timeline=[]
 for f in frames:
  end=float(f['seconds']);gap=float(f['gap_ms']);timeline.append((end-gap/1000,end,gap,f['phase']))
 theme=[r for r in parts if r['part']=='theme'];layout=[r for r in parts if r['part']=='layout'];assert len(theme)==len(layout)==70
 themed=[];laid=[];total=[]
 for a,z in zip(parts[::2],parts[1::2]):
  assert a['part']=='theme' and z['part']=='layout' and a['page']==z['page'] and a['theme']==z['theme']
  start=float(a['start_seconds']);mid=float(a['end_seconds']);end=float(z['end_seconds'])
  gaps=[g for x,y,g,phase in timeline if phase=='active' and g>0 and y>=start and x<=end]
  common={'page':a['page'],'theme':a['theme'],'maxOverlappingRenderingGapMs':max(gaps,default=0)}
  tr={**common,'durationMs':float(a['duration_ms']),'allocatedBytes':int(a['allocated_bytes']),'gc0':int(a['gc0_delta']),'gc1':int(a['gc1_delta']),'gc2':int(a['gc2_delta'])}
  lr={**common,'durationMs':float(z['duration_ms']),'allocatedBytes':int(z['allocated_bytes']),'gc0':int(z['gc0_delta']),'gc1':int(z['gc1_delta']),'gc2':int(z['gc2_delta'])}
  total.append({**common,'durationMs':(end-start)*1000,'allocatedBytes':tr['allocatedBytes']+lr['allocatedBytes'],'gc0':tr['gc0']+lr['gc0'],'gc1':tr['gc1']+lr['gc1'],'gc2':tr['gc2']+lr['gc2']})
  themed.append(tr);laid.append(lr)
 gaps=[float(f['gap_ms']) for f in frames if f['phase']=='active']
 report=(folder/'enduranceprobe-report.txt').read_text(encoding='utf-8-sig')
 summary=(folder/'performance-summary.txt').read_text(encoding='utf-8-sig')
 sample_line=next(line for line in report.splitlines() if line.startswith('SampleTimeUtc:'))
 assert 'pinned=True' in sample_line and '2026-10-28T09:00:00' in sample_line
 assert 'enduranceprobe OK' in report
 all_theme.extend(themed);all_layout.extend(laid);all_total.extend(total);all_frames.extend(gaps)
 runs.append({'run':i,'sampleTimeUtc':sample_line.split(': ',1)[1],'exitCode':0,'cycles':int(next(x.split('=')[1].split(';')[0] for x in summary.splitlines() if x.startswith('Cycles='))),'completedActions':int(next(x.split('=')[2].split(';')[0] for x in summary.splitlines() if x.startswith('Cycles='))),'themeApplication':stats(themed),'contentLayout':stats(laid),'combinedLayoutEnvelope':stats(total),'activeRenderingIntervals':{'samples':len(gaps),'p50Ms':percentile(gaps,.5),'p95Ms':percentile(gaps,.95),'maxMs':max(gaps),'over100Ms':sum(x>100 for x in gaps),'over300Ms':sum(x>300 for x in gaps)},'rawSha256':{name:hashlib.sha256((folder/name).read_bytes()).hexdigest() for name in ('page-cycles.csv','parts.csv','performance-frames.csv','performance-summary.txt','enduranceprobe-report.txt','console.log')}})
all_summary={'themeApplication':stats(all_theme),'contentLayout':stats(all_layout),'combinedLayoutEnvelope':stats(all_total),'activeRenderingIntervals':{'samples':len(all_frames),'p50Ms':percentile(all_frames,.5),'p95Ms':percentile(all_frames,.95),'maxMs':max(all_frames),'over100Ms':sum(x>100 for x in all_frames),'over300Ms':sum(x>300 for x in all_frames)}}
result={'schemaVersion':1,'measurement':'Three fixed-clock, fixed-cycle supplementary diagnostic runs. The combined envelope is reconstructed from adjacent theme and content-layout timestamp segments.','runs':runs,'combined':all_summary,'interpretation':{'segment':'The themeApplication segment brackets the two ApplyThemePalette calls; contentLayout brackets page-host assignment, responsive layout and UpdateLayout. These are WPF operation timings from the test harness.','gc':'GC.CollectionCount deltas are operation-window counters, not exact collection timestamps. Correlation is descriptive, not causal.','overlap':'Rendering callback gaps overlap operation time ranges; they are not physical/presented frames and temporal overlap is not proof of causation.','comparison':'The composite envelope approximates the older single layout timing boundary. Current U00 original frame/total-allocation baseline remains unavailable, so this does not complete the acceptance comparison.','input':'Programmatic fake-data text updates are not physical keyboard or IME response.','host':'The opacity 0.01 controlled STA window is not Playnite host rendering.'}}
(BASE/'analysis.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+chr(10),encoding='utf-8')
print(json.dumps({'combined':all_summary,'runs':[{'run':r['run'],'exitCode':r['exitCode'],'cycles':r['cycles'],'actions':r['completedActions'],'composite':{k:v for k,v in r['combinedLayoutEnvelope'].items() if k!='outliers'},'theme':{k:v for k,v in r['themeApplication'].items() if k!='outliers'},'contentLayout':{k:v for k,v in r['contentLayout'].items() if k!='outliers'},'activeRendering':r['activeRenderingIntervals']} for r in runs]},ensure_ascii=False,indent=2))