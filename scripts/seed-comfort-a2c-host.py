#!/usr/bin/env python3
"""Prepare the isolated, synthetic A2C Playnite host profile for UI audits.

The script changes only a profile supplied below the repository's .tmp directory.
It is safe to rerun after Playnite and its Worker have been closed.
"""

from __future__ import annotations

import argparse
import binascii
import hashlib
import json
import sqlite3
import struct
import zlib
from pathlib import Path


GAME_ONE = "16246ed8-8a3f-4a8d-87a0-be6840afbd5f"
GAME_TWO = "75658223-6718-44b4-bde0-9fc23c501a58"
DUPLICATE_NAME = "GSC Audit Synthetic 001"
OLD_BACKUP = "backup-20261125T110800Z-diff.zip"
NEW_BACKUP = "backup-20261125T130124Z-diff.zip"
COVER_BACKUPS = {
    "backup-20261120T212544Z.zip": "royal-shot-1",
    "backup-20261121T052544Z.zip": "royal-shot-2",
    "backup-20261121T062544Z.zip": "royal-shot-3",
}


FONT = {
    "0": ("111", "101", "101", "101", "111"),
    "1": ("010", "110", "010", "010", "111"),
    "2": ("111", "001", "111", "100", "111"),
    "3": ("111", "001", "111", "001", "111"),
}


def png_chunk(kind: bytes, data: bytes) -> bytes:
    return (
        struct.pack(">I", len(data))
        + kind
        + data
        + struct.pack(">I", binascii.crc32(kind + data) & 0xFFFFFFFF)
    )


def make_scene(index: int, destination: Path) -> tuple[str, int]:
    """Write a small, visibly distinct 16:9 pixel-art game capture as PNG."""
    width, height, scale = 160, 90, 3
    pixels = bytearray(width * height * 3)

    def put(x: int, y: int, color: tuple[int, int, int]) -> None:
        if 0 <= x < width and 0 <= y < height:
            offset = (y * width + x) * 3
            pixels[offset : offset + 3] = bytes(color)

    def rect(x: int, y: int, w: int, h: int, color: tuple[int, int, int]) -> None:
        for yy in range(max(0, y), min(height, y + h)):
            for xx in range(max(0, x), min(width, x + w)):
                put(xx, yy, color)

    def polygon(points: list[tuple[int, int]], color: tuple[int, int, int]) -> None:
        ymin = max(0, min(y for _, y in points))
        ymax = min(height - 1, max(y for _, y in points))
        for yy in range(ymin, ymax + 1):
            intersections: list[float] = []
            for pos, (x1, y1) in enumerate(points):
                x2, y2 = points[(pos + 1) % len(points)]
                if (y1 <= yy < y2) or (y2 <= yy < y1):
                    intersections.append(x1 + (yy - y1) * (x2 - x1) / (y2 - y1))
            intersections.sort()
            for start in range(0, len(intersections) - 1, 2):
                for xx in range(int(intersections[start]), int(intersections[start + 1]) + 1):
                    put(xx, yy, color)

    def text(x: int, y: int, value: str, color: tuple[int, int, int]) -> None:
        for char_index, char in enumerate(value):
            glyph = FONT.get(char)
            if glyph:
                for yy, row in enumerate(glyph):
                    for xx, bit in enumerate(row):
                        if bit == "1":
                            put(x + char_index * 4 + xx, y + yy, color)

    def player(x: int, y: int, tunic: tuple[int, int, int]) -> None:
        rect(x + 2, y, 5, 4, (244, 190, 132))
        rect(x + 1, y + 4, 7, 8, tunic)
        rect(x, y + 5, 2, 4, (227, 229, 222))
        rect(x + 8, y + 5, 3, 2, (220, 222, 215))
        rect(x + 2, y + 12, 2, 4, (42, 45, 59))
        rect(x + 6, y + 12, 2, 4, (42, 45, 59))

    if index == 1:
        for y in range(height):
            t = y / height
            rect(0, y, width, 1, (48, 111 - int(35 * t), 157 - int(48 * t)))
        rect(12, 13, 15, 4, (211, 232, 225))
        rect(22, 10, 9, 6, (211, 232, 225))
        polygon([(0, 54), (25, 27), (51, 54)], (74, 112, 135))
        polygon([(27, 55), (61, 22), (94, 55)], (59, 97, 123))
        polygon([(78, 55), (117, 29), (159, 55)], (81, 122, 135))
        rect(0, 54, width, 36, (43, 95, 58))
        rect(0, 70, width, 20, (56, 119, 63))
        for x, y in ((7, 45), (19, 40), (124, 46), (145, 42)):
            polygon([(x, y + 13), (x + 8, y - 7), (x + 16, y + 13)], (31, 78, 52))
            rect(x + 6, y + 9, 4, 13, (104, 74, 48))
        rect(69, 45, 5, 24, (177, 142, 92))
        rect(88, 45, 5, 24, (177, 142, 92))
        rect(74, 49, 14, 20, (81, 55, 48))
        polygon([(74, 51), (81, 45), (88, 51)], (215, 178, 105))
        rect(0, 0, 27, 15, (23, 40, 59))
        rect(3, 3, 21, 9, (45, 77, 73))
        text(7, 5, "01", (240, 226, 164))
        player(44, 59, (69, 133, 184))
    elif index == 2:
        for y in range(height):
            t = y / height
            rect(0, y, width, 1, (34 - int(13 * t), 44 - int(12 * t), 86 - int(26 * t)))
        polygon([(0, 50), (12, 17), (25, 48), (41, 8), (54, 48), (68, 19), (83, 50), (103, 7), (119, 48), (139, 14), (159, 50)], (26, 35, 72))
        rect(0, 51, width, 39, (37, 47, 82))
        rect(0, 70, width, 20, (39, 67, 95))
        for x, y in ((31, 57), (99, 52), (133, 64)):
            polygon([(x, y + 12), (x + 5, y - 9), (x + 10, y + 12)], (47, 218, 185))
            polygon([(x + 3, y + 10), (x + 5, y - 3), (x + 8, y + 10)], (177, 255, 219))
        for x in range(5, width, 19):
            rect(x, 78 + (x % 7), 11, 3, (90, 88, 133))
        rect(0, 0, 27, 15, (20, 30, 55))
        rect(3, 3, 21, 9, (50, 65, 107))
        text(7, 5, "02", (141, 248, 213))
        player(74, 56, (174, 86, 210))
    else:
        for y in range(height):
            if y < 51:
                t = y / 51
                color = (218, 102 + int(72 * t), 69 + int(57 * t))
            else:
                t = (y - 51) / 39
                color = (41 + int(29 * t), 106 + int(43 * t), 154 + int(35 * t))
            rect(0, y, width, 1, color)
        rect(116, 16, 14, 14, (255, 224, 148))
        polygon([(0, 52), (29, 37), (62, 52)], (131, 83, 86))
        polygon([(78, 54), (112, 32), (159, 54)], (114, 76, 96))
        for y in (59, 66, 74, 82):
            rect(0, y, width, 1, (79 + y, 144 + y // 4, 177 + y // 5))
        polygon([(69, 64), (111, 64), (100, 77), (79, 77)], (57, 44, 53))
        rect(86, 46, 2, 18, (228, 218, 190))
        polygon([(88, 47), (88, 61), (104, 57)], (246, 232, 194))
        rect(121, 41, 7, 19, (230, 221, 192))
        polygon([(119, 42), (124, 32), (130, 42)], (171, 62, 64))
        rect(0, 0, 27, 15, (79, 36, 46))
        rect(3, 3, 21, 9, (147, 74, 61))
        text(7, 5, "03", (255, 235, 174))
        player(45, 59, (238, 142, 59))

    raw = bytearray()
    for y in range(height):
        row = pixels[y * width * 3 : (y + 1) * width * 3]
        expanded = bytearray()
        for x in range(width):
            expanded.extend(row[x * 3 : x * 3 + 3] * scale)
        for _ in range(scale):
            raw.append(0)
            raw.extend(expanded)
    out_width, out_height = width * scale, height * scale
    header = struct.pack(">IIBBBBB", out_width, out_height, 8, 2, 0, 0, 0)
    png = (
        b"\x89PNG\r\n\x1a\n"
        + png_chunk(b"IHDR", header)
        + png_chunk(b"IDAT", zlib.compress(bytes(raw), 9))
        + png_chunk(b"IEND", b"")
    )
    destination.write_bytes(png)
    return hashlib.sha256(png).hexdigest().upper(), len(png)


def is_reparse_point(path: Path) -> bool:
    try:
        attributes = path.lstat().st_file_attributes
    except (AttributeError, FileNotFoundError):
        return path.is_symlink()
    return path.is_symlink() or bool(attributes & 0x400)


def ensure_regular_path(path: Path, *, directory: bool = False) -> Path:
    if is_reparse_point(path):
        raise SystemExit(f"拒绝沿重解析点访问: {path}")
    if directory and not path.is_dir():
        raise SystemExit(f"缺少隔离目录: {path}")
    if not directory and not path.is_file():
        raise SystemExit(f"缺少隔离文件: {path}")
    return path


def ensure_output_path(path: Path) -> None:
    if is_reparse_point(path):
        raise SystemExit(f"拒绝覆盖重解析点: {path}")
    if path.exists() and not path.is_file():
        raise SystemExit(f"输出目标不是普通文件: {path}")


def ensure_isolated_profile(profile: Path, repository: Path) -> Path:
    repository = repository.resolve(strict=True)
    temp_root_path = repository / ".tmp"
    ensure_regular_path(temp_root_path, directory=True)
    temp_root = temp_root_path.resolve(strict=True)
    supplied_path = profile.absolute()
    try:
        supplied_path.relative_to(temp_root_path.absolute())
    except ValueError as exc:
        raise SystemExit(f"拒绝修改 .tmp 之外的 profile: {supplied_path}") from exc

    current = temp_root_path
    for part in supplied_path.relative_to(temp_root_path.absolute()).parts:
        current = current / part
        ensure_regular_path(current, directory=True)
    profile = supplied_path.resolve(strict=True)
    try:
        profile.relative_to(temp_root)
    except ValueError as exc:
        raise SystemExit(f"profile 解析后位于 .tmp 之外: {profile}") from exc

    marker_path = profile / ".gsc-playnite-audit-profile.json"
    ensure_regular_path(marker_path)
    marker = json.loads(marker_path.read_text(encoding="utf-8"))
    if not marker.get("ProfileId"):
        raise SystemExit(f"profile marker 缺少 ProfileId: {marker_path}")
    expected = marker.get("ProfilePath")
    if expected and Path(expected).resolve(strict=True) != profile:
        raise SystemExit("profile marker 与目标路径不一致")

    for path in (profile, profile / "GameSaveCenter", profile / "audit-fixtures"):
        ensure_regular_path(path, directory=True)
    return profile


def make_entry(relative_path: str, data: bytes, last_write_utc: str) -> dict[str, object]:
    return {
        "RelativePath": relative_path,
        "SizeBytes": len(data),
        "LastWriteUtc": last_write_utc,
        "Sha256": hashlib.sha256(data).hexdigest(),
        "IsRegistryValue": False,
    }


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("profile", type=Path, help="目标隔离 Playnite profile，必须位于仓库 .tmp 下")
    args = parser.parse_args()

    repository = Path(__file__).resolve().parents[1]
    profile = ensure_isolated_profile(args.profile, repository)
    fixtures = profile / "audit-fixtures"
    database = profile / "GameSaveCenter" / "gamesavecenter.db"
    ensure_regular_path(database)
    for suffix in ("-wal", "-shm", "-journal"):
        sidecar = database.with_name(database.name + suffix)
        if sidecar.exists() or sidecar.is_symlink():
            ensure_regular_path(sidecar)
    manifest_path = fixtures / "comfort-a2c-seed-manifest.json"
    if manifest_path.exists() or manifest_path.is_symlink():
        ensure_regular_path(manifest_path)

    slot_a = fixtures / "diff" / "slot-a" / "save.dat"
    slot_b = fixtures / "diff" / "slot-b" / "settings.dat"
    ensure_regular_path(fixtures / "diff", directory=True)
    ensure_regular_path(slot_a.parent, directory=True)
    ensure_regular_path(slot_b.parent, directory=True)
    ensure_regular_path(slot_a)
    ensure_regular_path(slot_b)

    screenshots: dict[str, dict[str, object]] = {}
    media_rows: list[tuple[str, int, str]] = []
    for index in range(1, 4):
        media_id = f"royal-shot-{index}"
        image_path = fixtures / f"royal-gate-{index}.png"
        ensure_output_path(image_path)
        digest, size = make_scene(index, image_path)
        screenshots[media_id] = {
            "file": str(image_path),
            "sha256": digest,
            "sizeBytes": size,
            "dimensions": "480x270",
            "scene": ("森林入口", "水晶洞穴", "港口日落")[index - 1],
        }
        media_rows.append((media_id, size, digest))

    settings = slot_b.read_bytes()
    baseline_settings = settings + b"\nprevious-synthetic-state"
    slot_a_relative = "slot-a/save.dat"
    slot_b_relative = "slot-b/settings.dat"
    old_manifest = [
        make_entry(
            slot_b_relative,
            baseline_settings,
            "2026-11-25T11:08:00.0000000Z",
        )
    ]
    new_manifest = [
        make_entry(
            slot_a_relative,
            slot_a.read_bytes(),
            "2026-11-25T13:01:24.0000000Z",
        ),
        make_entry(
            slot_b_relative,
            settings,
            "2026-11-25T13:01:24.0000000Z",
        ),
    ]

    connection = sqlite3.connect(database)
    connection.execute("PRAGMA foreign_keys=ON")
    try:
        integrity = connection.execute("PRAGMA integrity_check").fetchone()[0]
        if integrity != "ok":
            raise SystemExit(f"Worker 数据库完整性检查失败: {integrity}")
        game_ids = {
            row[0]
            for row in connection.execute("SELECT playnite_id FROM games")
        }
        if not {GAME_ONE, GAME_TWO}.issubset(game_ids):
            raise SystemExit("Worker 数据库缺少预期的两个合成游戏 ID")

        backup_ids = {
            row[0]
            for row in connection.execute(
                "SELECT backup_id FROM backup_versions WHERE playnite_id=?",
                (GAME_ONE,),
            )
        }
        if not {OLD_BACKUP, NEW_BACKUP, *COVER_BACKUPS}.issubset(backup_ids):
            raise SystemExit("Worker 数据库缺少预期的差异或回顾图版本")

        with connection:
            cursor = connection.execute(
                "UPDATE backup_versions SET manifest_json=?,file_count=?,total_bytes=?,comment=? "
                "WHERE playnite_id=? AND backup_id=?",
                (
                    json.dumps(old_manifest, ensure_ascii=False, separators=(",", ":")),
                    len(old_manifest),
                    sum(item["SizeBytes"] for item in old_manifest),
                    "合成目录差异旧版本，仅供界面审计，不用于恢复。",
                    GAME_ONE,
                    OLD_BACKUP,
                ),
            )
            if cursor.rowcount != 1:
                raise SystemExit(f"旧差异版本更新行数错误: {cursor.rowcount}")
            cursor = connection.execute(
                "UPDATE backup_versions SET manifest_json=?,file_count=?,total_bytes=?,comment=? "
                "WHERE playnite_id=? AND backup_id=?",
                (
                    json.dumps(new_manifest, ensure_ascii=False, separators=(",", ":")),
                    len(new_manifest),
                    sum(item["SizeBytes"] for item in new_manifest),
                    "合成目录差异新版本，仅供界面审计，不用于恢复。",
                    GAME_ONE,
                    NEW_BACKUP,
                ),
            )
            if cursor.rowcount != 1:
                raise SystemExit(f"新差异版本更新行数错误: {cursor.rowcount}")
            for media_id, size, digest in media_rows:
                cursor = connection.execute(
                    "UPDATE media SET size_bytes=?,sha256=? WHERE media_id=? AND playnite_id=?",
                    (size, digest, media_id, GAME_ONE),
                )
                if cursor.rowcount != 1:
                    raise SystemExit(f"截图媒体更新行数错误: {media_id} / {cursor.rowcount}")
            for backup_id, media_id in COVER_BACKUPS.items():
                connection.execute(
                    "INSERT INTO backup_covers(playnite_id,backup_id,media_id) VALUES(?,?,?) "
                    "ON CONFLICT(playnite_id,backup_id) DO UPDATE SET media_id=excluded.media_id",
                    (GAME_ONE, backup_id, media_id),
                )
            cursor = connection.execute(
                "UPDATE games SET name=? WHERE playnite_id=?",
                (DUPLICATE_NAME, GAME_TWO),
            )
            if cursor.rowcount != 1:
                raise SystemExit(f"第二个合成游戏更新行数错误: {cursor.rowcount}")
            connection.execute(
                "UPDATE notification_records SET is_read=0 WHERE game_id IN (?,?) "
                "OR notification_id='a2c-notice-global'",
                (GAME_ONE, GAME_TWO),
            )

        foreign_key_errors = list(connection.execute("PRAGMA foreign_key_check"))
        final_integrity = connection.execute("PRAGMA integrity_check").fetchone()[0]
        if foreign_key_errors or final_integrity != "ok":
            raise SystemExit(
                f"更新后数据库校验失败: integrity={final_integrity}, foreignKeys={foreign_key_errors}"
            )
        for backup_id, media_id in COVER_BACKUPS.items():
            if connection.execute(
                "SELECT 1 FROM backup_covers WHERE playnite_id=? AND backup_id=? AND media_id=?",
                (GAME_ONE, backup_id, media_id),
            ).fetchone() is None:
                raise SystemExit(f"封面关联未写入: {backup_id} / {media_id}")
        connection.execute("PRAGMA wal_checkpoint(TRUNCATE)")
        connection.execute("PRAGMA journal_mode=DELETE")
    finally:
        connection.close()

    manifest = json.loads(manifest_path.read_text(encoding="utf-8")) if manifest_path.exists() else {}
    manifest.update(
        {
            "profileId": json.loads(
                (profile / ".gsc-playnite-audit-profile.json").read_text(encoding="utf-8")
            )["ProfileId"],
            "profilePath": str(profile),
            "playniteGameIds": [GAME_ONE, GAME_TWO],
            "duplicateDisplayName": DUPLICATE_NAME,
            "duplicateNameRequiresPlayniteLibraryEdit": True,
            "backupDirectories": [str(slot_a.parent), str(slot_b.parent)],
            "comparisonBackups": {"older": OLD_BACKUP, "newer": NEW_BACKUP},
            "screenshotIds": ["royal-shot-1", "royal-shot-2", "royal-shot-3"],
            "screenshotAssets": screenshots,
            "offlineMediaId": "royal-shot-4",
            "offlineMediaPath": str(fixtures / "offline" / "royal-shot-4.png"),
            "coverAssignments": COVER_BACKUPS,
            "privatePipe": "assigned by real-host-audit.ps1 at launch",
        }
    )
    manifest_path.write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    print(
        json.dumps(
            {
                "profile": str(profile),
                "profileId": manifest["profileId"],
                "integrity": final_integrity,
                "foreignKeyErrors": len(foreign_key_errors),
                "comparison": {
                    "older": OLD_BACKUP,
                    "olderFileCount": len(old_manifest),
                    "newer": NEW_BACKUP,
                    "newerFileCount": len(new_manifest),
                    "expected": {"added": 1, "modified": 1, "removed": 0},
                },
                "coverAssignments": len(COVER_BACKUPS),
                "screenshots": screenshots,
                "offlineMediaId": "royal-shot-4",
            },
            ensure_ascii=False,
        )
    )


if __name__ == "__main__":
    main()
