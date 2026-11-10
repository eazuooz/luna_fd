"""One-time legacy stage snapshot into the native Game Framework CSV schemas.
Run before removing legacy stage configuration. Subsequent balance edits belong in CSV.
"""
import csv, json, re, sys
from pathlib import Path
import yaml
ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Assets/Resources/DataTables"
current = OUT / "Stages.csv"
if current.is_file():
    with current.open(encoding="utf-8-sig", newline="") as stream:
        if "StartGold" in next(csv.reader(stream), []):
            raise SystemExit("This is a legacy exporter. Preserve the authored encounter plan and edit the current CSVs directly.")

def documents(path):
    text = path.read_text(encoding="utf-8-sig")
    text = re.sub(r"^%.*\n", "", text, flags=re.M)
    text = re.sub(r"^--- !u!\d+ &-?\d+.*$", "---", text, flags=re.M)
    return [x for x in yaml.safe_load_all(text) if x]

def guid(path):
    return re.search(r"^guid: (\w+)", path.read_text(), re.M)[1]

index = {guid(p): p.with_suffix("") for p in (ROOT/"Assets/Resources").rglob("*.meta") if p.is_file() and "guid:" in p.read_text(errors="ignore")}

def resource(ref):
    if not ref or not ref.get("guid"): return ""
    path = index.get(ref["guid"])
    if path is None: raise ValueError("Stage reference is outside Resources: " + ref["guid"])
    return str(path.relative_to(ROOT/"Assets/Resources").with_suffix("")).replace("\\", "/")

stage_root = ROOT/"Assets/Script/Contents/StageAndDungeon"
kinds = {guid(stage_root/(name+".cs.meta")): kind for name,kind in [("BasicDungeon",0),("BossDungeon",1),("EndlessDungeon",2)]}
stages, waves, rewards, normalizations = [], [], [], []
paths = sorted((ROOT/"Assets/Resources/Prefabs/10Stage").glob("*.prefab")) + sorted((ROOT/"Assets/Resources/Prefabs/11Dungeons").glob("*.prefab"))
for path in paths:
    authored = next((d["MonoBehaviour"] for d in documents(path) if "MonoBehaviour" in d and d["MonoBehaviour"].get("m_Script",{}).get("guid") in kinds), None)
    if authored is None: continue
    sid,kind = len(stages)+1,kinds[authored["m_Script"]["guid"]]
    balance = authored
    if kind == 0:
        source = ROOT/"Assets/Resources/ScriptableObjects/DungeonData/BasicDungeon"/(path.stem+".asset")
        balance = documents(source)[0]["MonoBehaviour"]
    stages.append([sid,path.stem,kind,(authored.get("DungeonName") or authored.get("stageName") or path.stem),authored.get("stageWidth",3.25),authored.get("dungeonCount",90),balance.get("spawnTerm",6),balance.get("goldAmount",0),int(authored.get("randomYDistance",0)),int(authored.get("isBossStage",0)),resource(authored.get("backgroundImage")),resource(authored.get("allyTower")),resource(authored.get("enemyTower")),resource(authored.get("bossMonster"))])
    def wave(phase,ref,count,level,order):
        name=resource(ref)
        if name and count>0: waves.append([len(waves)+1,sid,phase,name,count,level,order])
    if kind == 0:
        initial=balance.get("startMonsters") or {}
        repeat=balance.get("nonBossMonsters") or []
        if resource(initial.get("monster")):
            wave("Start",initial["monster"],max(1,initial.get("spawnOrder",1)),max(1,initial.get("monsterLevel",1)),0)
        elif repeat:
            wave("Start",repeat[0]["monster"],1,max(1,repeat[0].get("monsterLevel",1)),0)
        for order, entry in enumerate(repeat):
            wave("Repeat",entry["monster"],entry.get("spawnOrder",1),max(1,entry.get("monsterLevel",1)),order)
    elif kind == 2:
        for order,ref in enumerate(authored.get("Monsters",[])): wave("Endless",ref,1,0,order)
    for field,trigger in [("ItemDropTable","Victory"),("MonterDropTable","Monster"),("BossDropTable","EndlessBoss")]:
        for entry in balance.get(field,[]) or []:
            probability = entry.get("probability",1)
            item = resource(entry.get("itemData"))
            if probability == 100:
                # The old game multiplied this value by 100 before CanProbability;
                # authored 100 was a guaranteed reward, not a fractional probability.
                normalizations.append({"stage":path.stem,"source":str(path.relative_to(ROOT)),"field":field,"item":item,"authoredValue":100,"csvFraction":1})
                probability = 1
            if not 0 <= probability <= 1: raise ValueError(f"Unsupported authored reward probability: {path}:{field}={probability}")
            rewards.append([len(rewards)+1,sid,trigger,item,probability])

schemas = [("Stages",["RowId","Key","Kind","DisplayName","Width","TimeLimit","SpawnInterval","Gold","RandomY","IsBossStage","Background","AllyTower","EnemyTower","Boss"],stages),("StageWaves",["RowId","StageId","Phase","Prefab","Count","Level","Order"],waves),("StageRewards",["RowId","StageId","Trigger","Item","Probability"],rewards)]
if "--overwrite" not in sys.argv and any((OUT/(name+".csv")).exists() for name,_,_ in schemas): raise SystemExit("CSV already exists; it is now the balance authority. Explicit --overwrite required for a legacy re-export.")
if not stages: raise SystemExit("No legacy stage components remain. Export from the preserved pre-migration checkpoint; do not overwrite the current CSVs.")
OUT.mkdir(parents=True,exist_ok=True)
for name,header,rows in schemas:
    with (OUT/(name+".csv")).open("w",newline="",encoding="utf-8") as stream:
        writer=csv.writer(stream);writer.writerow(header);writer.writerows(rows)
    print(name,len(rows))
print("Reward probability normalizations:", json.dumps(normalizations, ensure_ascii=False))
