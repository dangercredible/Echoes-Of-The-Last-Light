#!/usr/bin/env python3
"""
One-off friendly: updates m_LocalPosition in Unity .unity YAML for named roots.
Run from repo root: python tools/resculpt_unity_scenes.py
"""
from __future__ import annotations

import re
import sys
from pathlib import Path
from typing import Dict, List, Optional, Tuple

ROOT = Path(__file__).resolve().parents[1]


def find_transform_ids_by_name(text: str, name_prefixes: Tuple[str, ...]) -> Dict[str, str]:
    """Map object name -> Transform fileID via m_GameObject link (robust order)."""
    out: Dict[str, str] = {}
    for m in re.finditer(r"--- !u!1 &(\d+)\nGameObject:", text):
        go_id = m.group(1)
        next_hdr = text.find("\n--- !u!", m.start() + 5)
        block = text[m.start() : next_hdr if next_hdr != -1 else len(text)]
        nm = re.search(r"m_Name:\s*(.+)", block)
        if not nm:
            continue
        name = nm.group(1).strip()
        if not any(name.startswith(p) or name == p for p in name_prefixes):
            continue
        tm = re.search(
            rf"--- !u!4 &(\d+)\nTransform:\s*\n(?:.*\n)*?\s*m_GameObject: \{{fileID: {go_id}\}}",
            text,
        )
        if not tm:
            print(f"WARN: no Transform for GameObject {name} &{go_id}", file=sys.stderr)
            continue
        out[name] = tm.group(1)
    return out


def replace_transform_pos(text: str, transform_id: str, x: float, y: float) -> str:
    pat = (
        rf"(--- !u!4 &{transform_id}\nTransform:\s*\n"
        rf"(?:[^\n]+\n)*?\s*m_LocalPosition:) "
        rf"\{{x: [^,]+, y: [^,]+, z: [^}}]+\}}"
    )

    def sub(m: re.Match) -> str:
        return f"{m.group(1)} {{x: {x}, y: {y}, z: 0}}"

    new_text, n = re.subn(pat, sub, text, count=1)
    if n != 1:
        print(f"WARN: could not replace transform &{transform_id}", file=sys.stderr)
    return new_text


def overgrowth_platform_positions() -> Dict[int, Tuple[float, float]]:
    """Falling Ruins: left safe -> climb -> mid plateau -> pit zigzag -> exit."""
    d: Dict[int, Tuple[float, float]] = {}
    # Section A: entry -52 .. -38 (flat / gentle)
    d[0] = (-50.0, -0.85)
    d[1] = (-45.2, -0.6)
    d[2] = (-40.5, -0.4)
    d[3] = (-36.0, 0.2)
    d[4] = (-32.0, 0.9)
    d[5] = (-28.0, 1.4)
    # Section B: vertical / pillar climb -28 .. -8
    d[6] = (-24.0, 3.0)
    d[7] = (-20.0, 5.5)
    d[8] = (-16.5, 8.0)
    d[9] = (-13.0, 10.5)
    d[10] = (-10.0, 12.8)
    d[11] = (-7.5, 15.0)
    # Section C: midpoint raised ledge
    d[12] = (2.0, 6.2)
    d[13] = (7.0, 6.5)
    d[14] = (12.0, 6.8)
    # Section D: approach pit + crossing (pit gap ~37–49, center 43)
    d[15] = (18.0, 4.5)
    d[16] = (24.0, 8.0)
    d[17] = (30.0, 12.5)
    d[18] = (34.5, 9.0)  # lip left of pit
    d[19] = (39.0, 5.5)  # over gap
    d[20] = (44.5, 3.2)  # toward right ground
    d[21] = (49.5, -1.0)  # land toward exit strip
    return d


def docks_platform_positions() -> Dict[int, Tuple[float, float]]:
    """Hand-drawn docks: high start -> drop -> hazard crossing -> shaft -> end."""
    d: Dict[int, Tuple[float, float]] = {}
    d[0] = (-49.0, 14.5)
    d[1] = (-44.0, 11.0)
    d[2] = (-38.0, 7.5)
    d[3] = (-33.0, 4.0)
    d[4] = (-28.0, 1.0)
    d[5] = (-22.0, -0.5)
    # Hazard hop chain (low)
    d[6] = (-16.0, 0.8)
    d[7] = (-10.0, 1.2)
    d[8] = (-4.0, 1.5)
    d[9] = (2.0, 2.0)
    d[10] = (8.0, 3.5)
    d[11] = (14.0, 6.0)
    # Shaft / climb
    d[12] = (18.0, 10.0)
    d[13] = (22.0, 14.5)
    d[14] = (26.0, 19.0)
    d[15] = (30.0, 23.5)
    d[16] = (34.0, 27.0)
    d[17] = (38.0, 30.0)
    d[18] = (42.0, 28.0)
    d[19] = (46.0, 22.0)
    d[20] = (49.0, 14.0)
    d[21] = (51.5, 6.0)
    return d


def patch_scene(
    path: Path,
    platform_xy: Dict[int, Tuple[float, float]],
    extras: List[Tuple[str, float, float]],
    extra_prefixes: Tuple[str, ...] = (
        "Platform_",
        "Wall_",
        "GrapplePoint_",
        "Platform_LightBridge_",
        "Platform_Static_",
        "Ground_",
        "PitKillZone",
        "LevelCheckpoint",
        "Player",
    ),
) -> None:
    text = path.read_text(encoding="utf-8")
    mapping = find_transform_ids_by_name(text, extra_prefixes)
    for pname, x, y in extras:
        if pname not in mapping:
            print(f"WARN {path.name}: missing {pname}", file=sys.stderr)
            continue
        text = replace_transform_pos(text, mapping[pname], x, y)
    for idx, xy in platform_xy.items():
        key = f"Platform_{idx}"
        if key not in mapping:
            print(f"WARN {path.name}: missing {key}", file=sys.stderr)
            continue
        text = replace_transform_pos(text, mapping[key], xy[0], xy[1])
    path.write_text(text, encoding="utf-8")
    print(f"OK {path.name}: patched platforms + extras")


def main() -> None:
    og = ROOT / "Assets" / "Scenes" / "TheOvergrowth.unity"
    dk = ROOT / "Assets" / "Scenes" / "Theshattereddocks.unity"

    og_extras = [
        ("Ground_Left", -9.0, -4.0),
        ("Ground_Right", 52.0, -4.0),
        ("PitKillZone", 43.0, -18.0),
        ("LevelCheckpoint", 52.5, -3.15),
        ("Player", -51.5, -2.88),
        ("Wall_Left", -11.5, -1.2),
        ("Wall_Right", 54.0, -1.2),
        ("Wall_CenterClimb", -18.0, 8.5),
        ("Wall_0", -26.0, 6.0),
        ("Wall_1", 20.0, 14.0),
        ("Platform_LightBridge_A", 31.0, 11.0),
        ("Platform_LightBridge_B", 41.0, 7.5),
        ("Platform_Static_UpperLeft", 6.5, 7.2),
        ("GrapplePoint_A", -22.0, 18.0),
        ("GrapplePoint_B", -12.0, 22.0),
        ("GrapplePoint_C", 5.0, 26.0),
    ]

    dk_extras = [
        ("Ground_Left", -9.0, -4.0),
        ("Ground_Right", 52.0, -4.0),
        ("PitKillZone", 44.0, -19.0),
        ("LevelCheckpoint", 52.5, -3.15),
        ("Player", -53.0, 17.5),
        ("Wall_Left", -11.5, -1.2),
        ("Wall_Right", 54.0, -1.2),
        ("Wall_CenterClimb", 16.0, 26.0),
        ("Wall_0", -30.0, 10.0),
        ("Wall_1", 36.0, 24.0),
        ("Platform_LightBridge_A", -8.0, 3.0),
        ("Platform_LightBridge_B", 28.0, 20.0),
        ("Platform_Static_UpperLeft", 40.0, 14.0),
        ("GrapplePoint_A", -35.0, 16.0),
        ("GrapplePoint_B", -5.0, 24.0),
        ("GrapplePoint_C", 24.0, 32.0),
    ]

    # Grapple grid — spread by index (names GrapplePoint_0 .. GrapplePoint_17 etc.)
    text_og = og.read_text(encoding="utf-8")
    og_mapping = find_transform_ids_by_name(
        text_og,
        ("Platform_", "Wall_", "GrapplePoint_", "Platform_LightBridge_", "Platform_Static_", "Ground_", "PitKillZone", "LevelCheckpoint", "Player"),
    )
    # Overgrowth grapple ring
    for name, pos in og_mapping.items():
        if not name.startswith("GrapplePoint_") or name.endswith(("_A", "_B", "_C")):
            continue
        suf = name.split("_")[-1]
        if not suf.isdigit():
            continue
        i = int(suf)
        gx = -46 + (i % 6) * 10.5
        gy = 4.0 + (i // 6) * 9.0
        text_og = replace_transform_pos(text_og, pos, gx, gy)

    og.write_text(text_og, encoding="utf-8")

    patch_scene(og, overgrowth_platform_positions(), og_extras)

    text_dk = dk.read_text(encoding="utf-8")
    dk_mapping = find_transform_ids_by_name(
        text_dk,
        ("Platform_", "Wall_", "GrapplePoint_", "Platform_LightBridge_", "Platform_Static_", "Ground_", "PitKillZone", "LevelCheckpoint", "Player"),
    )
    for name, tid in dk_mapping.items():
        if not name.startswith("GrapplePoint_") or name.endswith(("_A", "_B", "_C")):
            continue
        suf = name.split("_")[-1]
        if not suf.isdigit():
            continue
        i = int(suf)
        gx = -42 + (i % 5) * 11.0
        gy = 6.0 + (i // 5) * 11.5
        text_dk = replace_transform_pos(text_dk, tid, gx, gy)
    dk.write_text(text_dk, encoding="utf-8")

    patch_scene(dk, docks_platform_positions(), dk_extras)


if __name__ == "__main__":
    main()
