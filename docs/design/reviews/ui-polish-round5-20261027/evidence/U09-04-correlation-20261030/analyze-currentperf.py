from pathlib import Path
import csv, hashlib, json, math

BASE = Path(__file__).resolve().parent.parent
SOURCE = BASE / "U09-04-currentperf-20261030"
OUTPUT = Path(__file__).resolve().parent

def percentile(values, p):
    ordered = sorted(values)
    return ordered[max(0, math.ceil(p * len(ordered)) - 1)] if ordered else None

def pearson(xs, ys):
    mx = sum(xs) / len(xs)
    my = sum(ys) / len(ys)
    den = math.sqrt(sum((x - mx) ** 2 for x in xs) * sum((y - my) ** 2 for y in ys))
    return sum((x - mx) * (y - my) for x, y in zip(xs, ys)) / den if den else None

runs = []
for folder in sorted(SOURCE.glob("measurement-*-current")):
    parts = list(csv.DictReader((folder / "parts.csv").open(encoding="utf-8-sig")))
    layout = [row for row in parts if row["part"] == "layout"]
    times = [float(row["duration_ms"]) for row in layout]
    alloc = [int(row["allocated_bytes"]) for row in layout]
    frames = list(csv.DictReader((folder / "performance-frames.csv").open(encoding="utf-8-sig")))
    gaps = [float(row["gap_ms"]) for row in frames if row["phase"] == "active"]
    outliers = [
        {
            "page": row["page"],
            "theme": row["theme"],
            "durationMs": float(row["duration_ms"]),
            "allocatedBytes": int(row["allocated_bytes"]),
        }
        for row in layout
        if float(row["duration_ms"]) > 300
    ]
    runs.append(
        {
            "run": folder.name,
            "layout": {
                "samples": len(times),
                "p50Ms": percentile(times, 0.50),
                "p95Ms": percentile(times, 0.95),
                "maxMs": max(times),
                "over300Ms": len(outliers),
                "pearsonDurationVsAllocation": pearson(times, alloc),
            },
            "layoutOver300Ms": outliers,
            "activeRenderingIntervals": {
                "samples": len(gaps),
                "p50Ms": percentile(gaps, 0.50),
                "p95Ms": percentile(gaps, 0.95),
                "maxMs": max(gaps),
                "over33_3Ms": sum(value > 33.3 for value in gaps),
                "over100Ms": sum(value > 100 for value in gaps),
            },
            "partsCsvSha256": hashlib.sha256((folder / "parts.csv").read_bytes()).hexdigest(),
            "framesCsvSha256": hashlib.sha256((folder / "performance-frames.csv").read_bytes()).hexdigest(),
        }
    )

comparison = list(csv.DictReader((SOURCE / "comparison.csv").open(encoding="utf-8-sig")))
result = {
    "sourceCandidate": "5c0c57b06aed27dc46c41ef3d655ec20dafa5ec7",
    "currentRuns": runs,
    "highLevelComparison": comparison,
    "totalLayoutSamples": sum(run["layout"]["samples"] for run in runs),
    "totalLayoutOver300Ms": sum(run["layout"]["over300Ms"] for run in runs),
    "allocationNote": "Pearson correlation is descriptive only; no causal attribution.",
    "frameNote": "CompositionTarget.Rendering callback gaps are not presented/VSync frames.",
}
(OUTPUT / "analysis.json").write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(f"wrote {OUTPUT / 'analysis.json'}")
