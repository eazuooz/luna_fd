# 전투 데이터: CSV, native DataTable, 생성 요청

현재 루나는 제공된 GameFramework의 `IDataTableManager`와 `DataRowBase`로 전투 설정을 읽는다. 액터는 실제 `EntityLogic`과 typed `EntityData`로 실행한다. 이전 `Status`/`UnitAI` 또는 MonoBehaviour 수명 중계를 유지하기 위한 데이터 연결이 아니다.

CSV는 정적 전투 설정의 원본이다. 저장 진행도는 런타임 복제본에 별도로 복원하고, 아이콘·프리팹·Spine·소리·설명은 기존 에셋의 참조를 유지한다. `.meta`와 기존 직렬화 참조를 보존하면서 숫자의 편집 원본을 옮긴 구성이다.

## 데이터별 책임

다음은 현재 CSV 파일을 직접 센 행 수다. 파일은 모두 `Assets/Resources/DataTables/`에 있다. 숫자는 테스트 통과 횟수를 뜻하지 않는다.

| 파일 | 현재 행 수 | 책임 |
| --- | --- | --- |
| `UnitBalance.csv` | 76 | 유닛 기본 수치·분류·성장 기본값34필드. `RowId/Key/ResourcePath`를 더한 전체37열. |
| `SkillBalance.csv` | 32 | 액티브 스킬14개와 장비 능력18개의 정적 전투 설정18필드. 식별3열을 더한 전체21열. |
| `EntityParameters.csv` | 260 | 프리팹별 공격/효과/이동 등의 수치 매개변수. `RowId/Key/Parameter/Value`4열. |
| `Stages.csv` | 36 | 일반30개, 보스4개, 무한2개의 폭·시간·간격·골드·배경/타워/보스 경로. |
| `StageWaves.csv` | 155 | 스테이지별 시작/반복/무한 웨이브의 프리팹·수량·레벨·순서. |
| `StageRewards.csv` | 83 | 승리·몬스터·무한 보스 보상의 아이템 경로와 확률. |

`LunaUnitBalance`, `LunaSkillBalance`, `LunaEntityParameters`, `LunaStageTables`가 실제 native DataTable을 생성하고 행을 등록한다. SO 숫자로 조용히 되돌아가는 fallback 대신 누락·형식·중복·참조 오류를 초기화/Audit에서 보고한다. 기존 `BasicDungeonData`/`BossDungeonData`는 최초 전환 자료이며 현재 스폰 목록과 판 규칙의 원본이 아니다. `LunaStageDefinition.TableKey`와 Procedure가 소유하는 `LunaGame` 파생 객체가 CSV의 판/웨이브/보상을 사용한다.

## 적용 순서와 수명

1. CSV를 파싱해 native DataTable에 등록하고 GUID/Resources 경로 및 행 참조를 검사한다.
2. `LunaRuntimeData`가 원본 `ObjectData`/`SkillData`를 복제하고 CSV 기본값을 적용한다. 원본 에셋은 변경하지 않는다.
3. 저장된 레벨·이동 성장·카드 수·승급·스킬 레벨·선택/장착·아이템 보유·스테이지 진행도만 복원한다. 정적 피해·거리·쿨다운을 저장 데이터로 덮어쓰지 않는다.
4. 비동기 native Show 요청 전에 `UnitData`, `TowerData`, `PlayerLoadoutSnapshot` 등 typed 데이터가 해당 생성에 필요한 값과 ID를 캡처한다. 이후 전역 장착/레벨이 바뀌어도 이미 요청한 스킬/장비 스냅샷은 변하지 않는다.
5. 현재 HP·상태·계수·대상·스킬 쿨다운은 액터 데이터/컨트롤러가, 소환 Gold/Mana·슬롯·대기 요청은 판의 `LunaBattleRoster`가 소유한다. UI를 닫았다 열어도 초기화하지 않는다.

SO의 런타임 복제본과 `LunaDataTable`은 선택·진행도·표시 모델이다. 액터를 갱신하는 범용 실행기나 새로운 프레임워크를 구성하지 않는다. 아트 참조는 `UnitView`, 스킬/장비 에셋, `LunaStageBackdropView` 등에 유지하고, 실제 행동은 각 구체 native Logic에 작성한다. 모델 알림과 런타임 이벤트/생성 수명의 상세는 [Framework-Migration.md](Framework-Migration.md)를 따른다.

## UnitBalance 식별과 값

| 열 | 의미 |
| --- | --- |
| `RowId` | native DataTable의 고유 양의 정수. 기존 행을 재번호하지 않는다. |
| `Key` | 원본 SO의 `.meta` GUID. 파일 이름과 분리된 정체성. |
| `ResourcePath` | `Assets/Resources/`와 확장자를 제외한 `/` 경로. 예: `ScriptableObjects/LunaSkills/FireBall`. |
| `ID` | 기존 게임의 ObjectData.ID. native RowId와 별개다. |
| 나머지33필드 | 기본 진행도, 진영/유닛/공격 분류, 가격, HP/MP/충전시간, 이동·공격·방어·스킬·치명타·성장 계수, 공중/공격형 분류 등. |

정확한34필드 이름은 `LunaUnitBalanceRow.BalanceFields`와 CSV 헤더가 정의한다. 이전 문서의32필드 수는 현재 스키마와 다르다. 기존 SO 숫자는 초기 내보내기 시드로 보존돼 있으며 Inspector의 이전 숫자를 바꿔 운영 밸런스를 수정하는 방식은 사용하지 않는다.

## SkillBalance의18필드와 성장

`SkillBalance.csv`의 식별 열은 UnitBalance와 같은 `RowId/Key/ResourcePath`다. 정적 설정 필드는 다음과 같다. 기존 직렬화 이름의 오탈자도 참조 호환성을 위해 그대로 사용한다.

| 범위 | 필드 |
| --- | --- |
| 분류/허용 | `type`, `isMultiple`, `ableSkill` |
| 비용/진행 간격 | `mana`, `passiveDelay`, `activeDelay`, `coolTime`, `levelUpPoint` |
| 피해/거리 | `damage`, `range`, `splashRange` |
| 상태/확률 | `stunTime`, `burfTime`, `probablity` |
| 장비/소환 | `multipleSkillDamageBuff`, `multipleSkillResistanceBuff`, `coefficient`, `maxSummonCount` |

**저장된 `level`, 아이콘·이펙트 프리팹·한국어 설명은18필드에 포함하지 않는다.** 14개 액티브 스킬의 LV5·10·15·20 규칙은 [Player-Skill-Level-Rules.md](Player-Skill-Level-Rules.md)의 승인된 정책을 사용한다. CSV 기본값을 매 레벨 수정하는 방식이 아니라 typed `PlayerSkillSnapshot`이 저장된 레벨과 기본값으로 유효 수치를 계산한다. 장비 스킬도 원본 SO 포인터를 직접 캡처하지 않고 이름으로 현재 런타임 스킬 테이블에서 해석하므로 장비 레벨 저장이 반영된다.

`probablity`는 스킬의 백분율0–100이다. `StageRewards.Probability`는 별도로0–1 비율이며 1이100%다. 두 열의 단위를 혼용하지 않는다. 마나·소환 한도·거리·간격·지속시간은 음수를 거부하며, 모든 부동소수 수치의 NaN/Infinity를 거부한다.

## 프리팹 매개변수와 스테이지 연결

`EntityParameters.Key`는 `프리팹 GUID:GameObject fileID`이고 `Parameter`가 수치 이름이다. `UnitView.ParameterKey`가 이 레코드를 조회한다. 프리팹·소리 참조를 CSV 숫자에 넣거나 전투 중 원본 prefab 위치/스케일을 수정하지 않는다. 아트 연결은 별도 뷰에 남는다.

`StageWaves.StageId`와 `StageRewards.StageId`는 `Stages.RowId`를 참조한다. 웨이브 Phase는 `Start/Repeat/Endless`, 보상 Trigger는 `Victory/Monster/EndlessBoss`다. 종류·개수·순서·경로와 stage 참조는 Audit 대상이다. 스테이지 boolean은0/1을 사용한다. 기존 스테이지 SO를 바꿔도 현재 테이블을 대신하지 않으므로 판 설정은 해당 CSV에서 편집한다.

## 편집과 검증

CSV는 UTF-8, 첫 행은 정확한 헤더, 숫자는 소수점 `.`을 사용한다. 쉼표·따옴표·줄바꿈이 들어간 필드는 표준 CSV 인용을 지원하고 숫자는 문화권과 독립적으로 파싱한다. 중복 RowId/Key/Resources 경로, 정의되지 않은 enum, 잘못된 bool과 없는 참조를 검증한다.

| 작업 | 유닛 메뉴 | 스킬/장비 메뉴 |
| --- | --- | --- |
| 최초 CSV 생성 | `Luna > Data > Export Initial Unit Balance CSV` | `Luna > Data > Export Initial Skill Balance CSV` |
| 새 GUID/이동한 경로 동기화 | `Luna > Data > Sync New Unit Sources and Paths` | `Luna > Data > Sync New Skill Sources and Paths` |
| 값·참조 검사 | `Luna > Data > Validate Unit Balance CSV` | `Luna > Data > Validate Skill Balance CSV` |

현재 CSV는 이미 존재하므로 **최초 내보내기는 덮어쓰기를 거부한다**. 이후 수치는 CSV에서 편집한다. Sync는 기존 수치와 RowId를 보존하고 새 GUID와 변경 경로만 반영하며 교체 전 파일을 `Logs/Revival/BalanceBackups`에 남긴다. 삭제된 GUID는 자동으로 숨기지 않는다. 새 에셋을 만들거나 이동할 때 `.meta`를 유지하고 참조/행을 함께 검토한다.

Stage/EntityParameters를 포함한 전체 데이터와 참조는 프로젝트 Editor를 닫은 뒤 CLI로 검사할 수 있다.

```powershell
& .\Tools\validation\Validate-Luna.ps1 -Mode Audit -NoGraphics
& .\Tools\validation\Validate-Luna.ps1 -Mode ContentMatrix -MatrixSection skills -NoGraphics
& .\Tools\validation\Validate-Luna.ps1 -Mode ContentMatrix -MatrixSection skill-levels -NoGraphics
& .\Tools\validation\Validate-Luna.ps1 -Mode ContentMatrix -MatrixSection stages -NoGraphics
```

Audit는 참조/구조 검사이며 실제 모든 전투의 통과를 뜻하지 않는다. 현재 소스는 r04의3회 모두 Audit15/Smoke8/Matrix442를 통과했고, 스킬32행×18필드·문화권 파싱·장비/레벨 스냅샷·원본 보존과 실제 LV20 효과는 해당 named 사례에서 검증했다. 새 Windows 실행까지 완료한 근거와 native 메모리 미해결 항목은 [Revival-QA.md](Revival-QA.md)를 따른다. 원격 CSV 다운로드·서비스 데이터 배포·온라인 기능·모든 조합의 상용 밸런스 확정은 이 로컬 복구 범위에 포함하지 않는다.
