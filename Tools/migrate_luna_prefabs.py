"""One-time, offline migration of authored component fields to native entity bindings.

Requires the pre-rewrite GUID inventory. Refuses to overwrite its migration report.
Unity is deliberately not involved: retired C# component types no longer compile.
"""
from pathlib import Path
import collections
import csv
import json
import re
import uuid
import yaml

ROOT = Path(__file__).resolve().parents[1]
INVENTORY = ROOT / 'Logs/NativeRewrite/original-script-guids.json'
REPORT = ROOT / 'Logs/NativeRewrite/prefab-migration.json'
SCRIPTS = json.loads(INVENTORY.read_text(encoding='utf-8'))
VIEW_GUID = '200b4907c42849d9a6e5dad370d9b3a3'
CATALOG_GUID = 'ed173d5ef25c4a86898099d19364c17b'
STAGE_GUID = '09f8a05808794181b3581913cff5906f'
HEROES = set('Alice Clare Dren Elena Goldy Joe Lena Lene Lennon Matilda Mei Miu Pobi Pochi Pring Raccon Ria Roen Roni Rose Scalet Tira'.split())
BASE_COMPONENTS = set('Status UnitAI UnitAI_Player DwarfAI LunaStateMachine HeroSkillControl'.split())
MAPPING = {
    'LunaStateMachine': 'Luna.Gameplay.PlayerLogic', 'UnitAI_Player': 'Luna.Gameplay.PlayerLogic',
    'LunaCtrl_Slime': 'Luna.Gameplay.SlimePlayerLogic', 'LunaCtrl_Rich': 'Luna.Gameplay.RichPlayerLogic',
    'LunaCtrl_Lene': 'Luna.Gameplay.PlayerLogic', 'LunaCtrl_Dragon': 'Luna.Gameplay.DragonPlayerLogic',
    'LunaDragonCtrl': 'Luna.Gameplay.DragonPetLogic', 'Skeleton_ax_ally': 'Luna.Gameplay.SkeletonAxeLogic',
    'Projectile': 'Luna.Gameplay.ProjectileLogic', 'Arrow': 'Luna.Gameplay.ArrowLogic',
    'MniMartinContol': 'Luna.Gameplay.MiniMartinLogic', 'MartinControl': 'Luna.Gameplay.MartinLogic',
    'Effect': 'Luna.Gameplay.EffectLogic', 'NormalSkeletonObjControl': 'Luna.Gameplay.EffectLogic',
    'DamageTextControl': 'Luna.Gameplay.DamageTextLogic', 'HealthBarControl': 'Luna.Gameplay.HealthBarLogic',
    'DropItem': 'Luna.Gameplay.DropItemLogic', 'DropCoin': 'Luna.Gameplay.DropCoinLogic',
    'TurretCtrl': 'Luna.Gameplay.TowerLogic', 'BackPlateDemonCtrl': 'Luna.Gameplay.BackPlateLogic',
    'BGDemonCtrl': 'Luna.Gameplay.BackgroundDemonLogic', 'Tree': 'Luna.Gameplay.TreeLogic',
    'SkillBomb': 'SkillBomb',
}
STAGES = {'BasicDungeon', 'BossDungeon', 'EndlessDungeon'}
EXTRA = {'BGEffectCreator'}


class Dumper(yaml.SafeDumper):
    def ignore_aliases(self, data):
        return True


def represent_dict(dumper, value):
    flow = 'fileID' in value and set(value) <= {'fileID', 'guid', 'type'}
    return dumper.represent_mapping('tag:yaml.org,2002:map', value, flow_style=flow)


Dumper.add_representer(dict, represent_dict)


def guid(path):
    meta = Path(str(path) + '.meta')
    if not meta.exists():
        meta.write_text('fileFormatVersion: 2\nguid: ' + uuid.uuid4().hex + '\n', encoding='utf-8')
    return re.search(r'^guid: (\w+)', meta.read_text(encoding='utf-8-sig'), re.M)[1]


def dump_block(kind, identity, content):
    return f'--- !u!{kind} &{identity}\n' + yaml.dump(content, Dumper=Dumper, sort_keys=False, allow_unicode=True, width=160)


def blocks(text):
    result = []
    for raw in re.split(r'(?=^--- !u!)', text, flags=re.M):
        match = re.match(r'--- !u!(\d+) &(-?\d+)([^\n]*)\n', raw)
        if not match:
            result.append({'raw': raw})
            continue
        result.append({'kind': int(match[1]), 'id': int(match[2]), 'suffix': match[3],
                       'raw': raw, 'content': yaml.safe_load(raw[match.end():])})
    return result


def field_ref(value):
    return isinstance(value, dict) and value.get('fileID', 0) != 0


def main():
    if REPORT.exists():
        raise SystemExit('Migration report already exists. Review it instead of rerunning a destructive source conversion.')
    asset_paths = {}
    for meta in (ROOT / 'Assets').rglob('*.meta'):
        match = re.search(r'^guid: (\w+)', meta.read_text(encoding='utf-8-sig'), re.M)
        if match:
            asset_paths[match[1]] = Path(str(meta)[:-5])
    # Extra actor classes supplied by the actor implementation; explicit mapping is reviewable.
    extra_map = ROOT / 'Logs/NativeRewrite/actor-catalog-map.json'
    if extra_map.exists():
        MAPPING.update(json.loads(extra_map.read_text(encoding='utf-8')))
    known = BASE_COMPONENTS | HEROES | set(MAPPING) | STAGES | EXTRA
    mini_definition = next(p for p in (ROOT / 'Assets/Resources').rglob('*.asset') if p.stem == 'miniMartinEnemy')
    mini_ref = {'fileID': 11400000, 'guid': guid(mini_definition), 'type': 2}
    entries, changes, parameters = [], [], []
    for path in sorted((ROOT / 'Assets/Resources').rglob('*.prefab')):
        text = path.read_text(encoding='utf-8-sig')
        docs = blocks(text)
        by_id = {b['id']: b for b in docs if 'id' in b}
        groups = collections.defaultdict(list)
        root_go = None
        for doc in docs:
            content = doc.get('content', {})
            for kind in ('Transform', 'RectTransform'):
                component = content.get(kind)
                if component and component.get('m_Father', {}).get('fileID') == 0 and component.get('m_GameObject'):
                    root_go = component['m_GameObject']['fileID']
                    break
            component = content.get('MonoBehaviour')
            if not component:
                continue
            script = SCRIPTS.get(component.get('m_Script', {}).get('guid'))
            if script and script['class'] in known:
                groups[component['m_GameObject']['fileID']].append((script['class'], doc, component))
        if root_go is None:
            continue
        prefab_guid = guid(path)
        removed = set()
        replacements = {}
        classes = [name for records in groups.values() for name, _, _ in records]
        catalog_type = None
        for name in HEROES:
            if name in classes:
                catalog_type = name
                break
        if catalog_type is None:
            priority = ['LunaCtrl_Dragon', 'LunaCtrl_Rich', 'LunaCtrl_Lene', 'LunaCtrl_Slime', 'Skeleton_ax_ally',
                        'LunaDragonCtrl', 'UnitAI_Player', 'LunaStateMachine', 'TurretCtrl', 'SkillBomb',
                        'MniMartinContol', 'MartinControl', 'Projectile', 'Arrow', 'DamageTextControl', 'HealthBarControl',
                        'DropItem', 'DropCoin', 'NormalSkeletonObjControl', 'Effect', 'BackPlateDemonCtrl', 'BGDemonCtrl', 'Tree']
            priority += [n for n in MAPPING if n not in priority]
            for name in priority:
                if name in classes:
                    catalog_type = MAPPING[name]
                    break
        if catalog_type is None and ('UnitAI' in classes or 'DwarfAI' in classes):
            catalog_type = 'Luna.Gameplay.UnitLogic'
        relative = path.relative_to(ROOT).as_posix()
        if relative == 'Assets/Resources/Prefabs/00Luna/06_DragonPet.prefab':
            catalog_type = 'Luna.Gameplay.DragonPetLogic'
        if '/08StageBackground/' in relative and catalog_type is None:
            catalog_type = 'LunaStageBackdropLogic'
        if '/06Effects/' in relative and catalog_type is None:
            catalog_type = 'Luna.Gameplay.EffectLogic'
        for go, records in groups.items():
            if any(name in STAGES for name, _, _ in records):
                name, old, component = next(record for record in records if record[0] in STAGES)
                new = {k: v for k, v in component.items() if k.startswith('m_')}
                new['m_Script'] = {'fileID': 11500000, 'guid': STAGE_GUID, 'type': 3}
                new.update(TableKey=path.stem, dungeonTable=component.get('dungeonTable', {'fileID': 0}))
                replacements[old['id']] = dump_block(114, old['id'], {'MonoBehaviour': new})
                continue
            first = next((record for record in records if record[0] == 'Status'), records[0])
            new = {k: v for k, v in first[2].items() if k.startswith('m_')}
            new['m_Script'] = {'fileID': 11500000, 'guid': VIEW_GUID, 'type': 3}
            key = f'{prefab_guid}:{go}'
            new.update(Definition={'fileID': 0}, Projectile={'fileID': 0}, AttackEffectPrefab={'fileID': 0},
                       AttackSoundClip={'fileID': 0}, AttackSoundType=-1, ParameterKey=key, Prefabs=[], Sounds=[], Numbers=[], Texts=[])
            flattened = {}
            for name, doc, component in records:
                if doc['id'] != first[1]['id']:
                    removed.add(doc['id'])
                if name == 'Status':
                    new['Definition'] = component.get('sourceObjectData', component.get('objectData', {'fileID': 0}))
                    if not field_ref(new['Definition']):
                        new['Definition'] = component.get('objectData', {'fileID': 0})
                    for field in ['AttackEffectPrefab', 'AttackSoundClip', 'AttackSoundType']:
                        if field in component:
                            new[field] = component[field]
                    continue
                for field, value in component.items():
                    if field.startswith('m_') or field in ('status', 'DebugOn', 'Attacker', 'Target', 'myTower', 'moving', 'curStateString'):
                        continue
                    flattened[f'{name}.{field}'] = value
                    flattened.setdefault(field, value)
            if ('MniMartinContol' in classes or 'MartinControl' in classes) and not field_ref(new['Definition']):
                new['Definition'] = mini_ref
            for field, value in flattened.items():
                if field == 'projectile':
                    new['Projectile'] = value
                if isinstance(value, dict) and 'fileID' in value:
                    asset_path = asset_paths.get(value.get('guid'))
                    if asset_path and asset_path.suffix.lower() in ('.wav', '.ogg', '.mp3', '.aiff'):
                        new['Sounds'].append({'Name': field, 'Value': value})
                    elif asset_path and asset_path.suffix == '.prefab':
                        new['Prefabs'].append({'Name': field, 'Value': value})
                elif isinstance(value, (int, float, bool)):
                    parameters.append([len(parameters) + 1, key, field, float(value)])
                elif isinstance(value, str):
                    new['Texts'].append({'Name': field, 'Value': value})
            replacements[first[1]['id']] = dump_block(114, first[1]['id'], {'MonoBehaviour': new})
        # The dragon pet is now requested by DragonPlayerLogic with its own typed data.
        if path.stem == '06Luna_Dragon':
            removed.update((8391561234743080450, 6951563279374113896, 8580917065936782990))
            key = f'{prefab_guid}:{root_go}'
            for axis in 'XYZ':
                parameters.append([len(parameters) + 1, key, 'petLocalPosition' + axis, 0])
                parameters.append([len(parameters) + 1, key, 'petScale' + axis, 1])
        # Strip serialized component / child links only for the exact retired IDs.
        output = []
        for doc in docs:
            if doc.get('id') in removed:
                continue
            raw = replacements.get(doc.get('id'), doc['raw'])
            for identity in removed:
                raw = re.sub(r'^  - component: \{fileID: ' + str(identity) + r'\}\n', '', raw, flags=re.M)
                raw = re.sub(r'^  - \{fileID: ' + str(identity) + r'\}\n', '', raw, flags=re.M)
            raw = re.sub(r'^  refData: \{[^\n]*\}\n', '', raw, flags=re.M)
            output.append(raw)
        rewritten = ''.join(output)
        if rewritten != text:
            path.write_bytes(rewritten.encode('utf-8'))
            changes.append({'path': relative, 'oldComponents': classes, 'logic': catalog_type, 'removedIds': sorted(removed)})
        if catalog_type:
            group = 'Actors' if any(name in classes for name in BASE_COMPONENTS | HEROES) else 'Effects'
            if catalog_type == 'LunaStageBackdropLogic': group = 'World'
            entries.append({'Prefab': {'fileID': root_go, 'guid': prefab_guid, 'type': 3}, 'Address': relative,
                            'LogicType': catalog_type, 'Group': group})
    catalog = ROOT / 'Assets/Resources/Luna/EntityCatalog.asset'
    catalog.parent.mkdir(parents=True, exist_ok=True)
    data = {'m_ObjectHideFlags': 0, 'm_CorrespondingSourceObject': {'fileID': 0}, 'm_PrefabInstance': {'fileID': 0},
            'm_PrefabAsset': {'fileID': 0}, 'm_GameObject': {'fileID': 0}, 'm_Enabled': 1, 'm_EditorHideFlags': 0,
            'm_Script': {'fileID': 11500000, 'guid': CATALOG_GUID, 'type': 3}, 'm_Name': 'EntityCatalog',
            'm_EditorClassIdentifier': '', 'Entries': entries}
    catalog.write_text('%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n' + dump_block(114, 11400000, {'MonoBehaviour': data}), encoding='utf-8')
    guid(catalog)
    csv_path = ROOT / 'Assets/Resources/DataTables/EntityParameters.csv'
    with csv_path.open('w', encoding='utf-8', newline='') as stream:
        writer = csv.writer(stream, lineterminator='\n'); writer.writerow(['RowId', 'Key', 'Parameter', 'Value']); writer.writerows(parameters)
    guid(csv_path)
    REPORT.write_text(json.dumps({'changed': changes, 'catalog': entries, 'parameterCount': len(parameters)}, indent=2), encoding='utf-8')
    print(f'Migrated {len(changes)} prefabs; native catalog {len(entries)} entries; {len(parameters)} numeric parameters.')


if __name__ == '__main__':
    main()
