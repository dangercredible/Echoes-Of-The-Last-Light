import fs from "fs";
import path from "path";
import { fileURLToPath } from "url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const ROOT = path.join(__dirname, "..");

function findTransformIdForGameObject(text, goId) {
  // One YAML document per --- !u!; stop at next doc so we do not match m_GameObject on BoxCollider, etc.
  const re = /--- !u!4 &(\d+)\nTransform:\n([\s\S]*?)(?=\n--- !u!)/g;
  let m;
  while ((m = re.exec(text)) !== null) {
    const tid = m[1];
    const body = m[2];
    if (body.includes(`m_GameObject: {fileID: ${goId}}`)) return tid;
  }
  return null;
}

function findTransformIdsByName(text, prefixes) {
  const out = {};
  const re = /--- !u!1 &(\d+)\nGameObject:\n/g;
  let m;
  while ((m = re.exec(text)) !== null) {
    const goId = m[1];
    const start = m.index;
    const nextGo = text.indexOf("--- !u!1 &", start + 5);
    const block = text.slice(start, nextGo === -1 ? text.length : nextGo);
    const nm = block.match(/m_Name:\s*(.+)/);
    if (!nm) continue;
    const name = nm[1].trim();
    const ok = prefixes.some((p) => name === p || name.startsWith(p));
    if (!ok) continue;
    const tid = findTransformIdForGameObject(text, goId);
    if (!tid) {
      console.warn(`WARN: no Transform for GameObject ${name} &${goId}`);
      continue;
    }
    out[name] = tid;
  }
  return out;
}

function replaceTransformPos(text, transformId, x, y) {
  const header = `--- !u!4 &${transformId}\nTransform:\n`;
  let start = 0;
  while ((start = text.indexOf(header, start)) !== -1) {
    const afterHeader = start + header.length;
    const scanLen = 2048;
    const scan = text.slice(afterHeader, afterHeader + scanLen);
    const relPos = scan.indexOf("m_LocalPosition:");
    if (relPos === -1) {
      start += header.length;
      continue;
    }
    const posIdx = afterHeader + relPos;
    const lineEnd = text.indexOf("\n", posIdx);
    if (lineEnd === -1) break;
    let oldLine = text.slice(posIdx, lineEnd).replace(/\r$/, "");
    const braceRe = /\{x:\s*[^,]+,\s*y:\s*[^,]+,\s*z:\s*[^\}]+\}/;
    if (!braceRe.test(oldLine)) {
      console.warn(`WARN: could not parse position line for transform &${transformId}`);
      start += header.length;
      continue;
    }
    const newLine = oldLine.replace(braceRe, `{x: ${x}, y: ${y}, z: 0}`);
    return text.slice(0, posIdx) + newLine + text.slice(lineEnd);
  }
  console.warn(`WARN: could not replace transform &${transformId}`);
  return text;
}

const overgrowthPlatforms = () => ({
  0: [-50.0, -0.85],
  1: [-45.2, -0.6],
  2: [-40.5, -0.4],
  3: [-36.0, 0.2],
  4: [-32.0, 0.9],
  5: [-28.0, 1.4],
  6: [-24.0, 3.0],
  7: [-20.0, 5.5],
  8: [-16.5, 8.0],
  9: [-13.0, 10.5],
  10: [-10.0, 12.8],
  11: [-7.5, 15.0],
  12: [2.0, 6.2],
  13: [7.0, 6.5],
  14: [12.0, 6.8],
  15: [18.0, 4.5],
  16: [24.0, 8.0],
  17: [30.0, 12.5],
  18: [34.5, 9.0],
  19: [39.0, 5.5],
  20: [44.5, 3.2],
  21: [49.5, -1.0],
});

const docksPlatforms = () => ({
  0: [-49.0, 14.5],
  1: [-44.0, 11.0],
  2: [-38.0, 7.5],
  3: [-33.0, 4.0],
  4: [-28.0, 1.0],
  5: [-22.0, -0.5],
  6: [-16.0, 0.8],
  7: [-10.0, 1.2],
  8: [-4.0, 1.5],
  9: [2.0, 2.0],
  10: [8.0, 3.5],
  11: [14.0, 6.0],
  12: [18.0, 10.0],
  13: [22.0, 14.5],
  14: [26.0, 19.0],
  15: [30.0, 23.5],
  16: [34.0, 27.0],
  17: [38.0, 30.0],
  18: [42.0, 28.0],
  19: [46.0, 22.0],
  20: [49.0, 14.0],
  21: [51.5, 6.0],
});

const PREFIXES = [
  "Platform_",
  "Wall_",
  "GrapplePoint_",
  "Platform_LightBridge_",
  "Platform_Static_",
  "Ground_",
  "PitKillZone",
  "LevelCheckpoint",
  "Player",
  "TutorialPrompt_",
];

function patchScene(filePath, platformXY, extras) {
  let text = fs.readFileSync(filePath, "utf8");
  const mapping = findTransformIdsByName(text, PREFIXES);
  for (const [pname, x, y] of extras) {
    const tid = mapping[pname];
    if (!tid) {
      console.warn(`WARN ${path.basename(filePath)}: missing ${pname}`);
      continue;
    }
    text = replaceTransformPos(text, tid, x, y);
  }
  for (const [idx, xy] of Object.entries(platformXY)) {
    const key = `Platform_${idx}`;
    const tid = mapping[key];
    if (!tid) {
      console.warn(`WARN ${path.basename(filePath)}: missing ${key}`);
      continue;
    }
    text = replaceTransformPos(text, tid, xy[0], xy[1]);
  }
  fs.writeFileSync(filePath, text, "utf8");
  console.log(`OK ${path.basename(filePath)}`);
}

function scatterGrapples(text, mapping) {
  for (const name of Object.keys(mapping)) {
    if (!name.startsWith("GrapplePoint_") || /_A$|_B$|_C$/.test(name)) continue;
    const suf = name.split("_").pop();
    if (!/^\d+$/.test(suf)) continue;
    const i = parseInt(suf, 10);
    const gx = -46 + (i % 6) * 10.5;
    const gy = 4.0 + Math.floor(i / 6) * 9.0;
    text = replaceTransformPos(text, mapping[name], gx, gy);
  }
  return text;
}

function scatterGrapplesDocks(text, mapping) {
  for (const name of Object.keys(mapping)) {
    if (!name.startsWith("GrapplePoint_") || /_A$|_B$|_C$/.test(name)) continue;
    const suf = name.split("_").pop();
    if (!/^\d+$/.test(suf)) continue;
    const i = parseInt(suf, 10);
    const gx = -42 + (i % 5) * 11.0;
    const gy = 6.0 + Math.floor(i / 5) * 11.5;
    text = replaceTransformPos(text, mapping[name], gx, gy);
  }
  return text;
}

const ogPath = path.join(ROOT, "Assets", "Scenes", "TheOvergrowth.unity");
const dkPath = path.join(ROOT, "Assets", "Scenes", "Theshattereddocks.unity");

const ogExtras = [
  ["Ground_Left", -9.0, -4.0],
  ["Ground_Right", 52.0, -4.0],
  ["PitKillZone", 43.0, -18.0],
  ["LevelCheckpoint", 52.5, -3.15],
  ["Player", -51.5, -2.88],
  ["Wall_Left", -11.5, -1.2],
  ["Wall_Right", 54.0, -1.2],
  ["Wall_CenterClimb", -18.0, 8.5],
  ["Wall_0", -26.0, 6.0],
  ["Wall_1", 20.0, 14.0],
  ["Platform_LightBridge_A", 31.0, 11.0],
  ["Platform_LightBridge_B", 41.0, 7.5],
  ["Platform_Static_UpperLeft", 6.5, 7.2],
  ["GrapplePoint_A", -22.0, 18.0],
  ["GrapplePoint_B", -12.0, 22.0],
  ["GrapplePoint_C", 5.0, 26.0],
  ["TutorialPrompt_1", -49.0, -1.2],
  ["TutorialPrompt_2", -24.0, 4.5],
  ["TutorialPrompt_3", -10.0, 13.5],
  ["TutorialPrompt_4", 19.0, 11.0],
];

const dkExtras = [
  ["Ground_Left", -9.0, -4.0],
  ["Ground_Right", 52.0, -4.0],
  ["PitKillZone", 44.0, -19.0],
  ["LevelCheckpoint", 52.5, -3.15],
  ["Player", -53.0, 17.5],
  ["Wall_Left", -11.5, -1.2],
  ["Wall_Right", 54.0, -1.2],
  ["Wall_CenterClimb", 16.0, 26.0],
  ["Wall_0", -30.0, 10.0],
  ["Wall_1", 36.0, 24.0],
  ["Platform_LightBridge_A", -8.0, 3.0],
  ["Platform_LightBridge_B", 28.0, 20.0],
  ["Platform_Static_UpperLeft", 40.0, 14.0],
  ["GrapplePoint_A", -35.0, 16.0],
  ["GrapplePoint_B", -5.0, 24.0],
  ["GrapplePoint_C", 24.0, 32.0],
  ["TutorialPrompt_1", -52.0, 15.5],
  ["TutorialPrompt_2", -28.0, 8.0],
  ["TutorialPrompt_3", -12.0, 2.5],
  ["TutorialPrompt_4", 22.0, 21.0],
];

patchScene(ogPath, overgrowthPlatforms(), ogExtras);
let t = fs.readFileSync(ogPath, "utf8");
let map = findTransformIdsByName(t, PREFIXES);
t = scatterGrapples(t, map);
fs.writeFileSync(ogPath, t);

patchScene(dkPath, docksPlatforms(), dkExtras);
t = fs.readFileSync(dkPath, "utf8");
map = findTransformIdsByName(t, PREFIXES);
t = scatterGrapplesDocks(t, map);
fs.writeFileSync(dkPath, t);
