# 유닛 밸런스: CSV와 Game Framework DataTable

몬스터 데이터는 `ObjectData` ScriptableObject에 이미지 참조와 밸런스 수치가 섞여 있었다. 수치를 CSV로 분리하고, 프리팹·아이콘·Spine 등 Unity 객체 참조와 한국어 설명은 기존 에셋에 유지한다. SO를 모두 제거하면 프리팹의 직렬화 참조가 깨지므로, 실행할 때만 SO 복제본에 테이블 값을 적용하는 호환 계층을 사용한다.

## 원본과 우선순위

1. `Assets/Resources/DataTables/UnitBalance.csv`: 유닛 기본 수치·분류의 편집 원본이다. 코드가 실제로 Game Framework `IDataTableManager.CreateDataTable<LunaUnitBalanceRow>("UnitBalance")`와 `AddDataRow`를 호출해 읽는다.
2. 기존 `ObjectData` SO: 이미지·프리팹·표시 이름·현지화 설명의 편집 원본이다. 기존 숫자는 최초 내보내기 시드로 남아 있으며 실행 시 CSV 값이 우선한다. CSV 생성 후 Inspector에서 이전 숫자를 수정할 수 없게 한다.
3. 개인 저장: 레벨·카드 수·승급 등 저장된 진행도만 CSV 기본값 위에 복원한다. CSV를 바꿔도 기존 저장을 지우지 않는다.
4. 스테이지 설정: 기존 몬스터 스폰 목록과 스테이지별 몬스터 레벨 조정은 여전히 `BasicDungeonData`/`BossDungeonData`의 역할이다. CSV는 공통 유닛 기본값이며 이 전투별 설정을 제거하지 않는다.

`LunaRuntimeData`는 원본 SO를 변경하지 않는다. 처음 복제할 때 테이블을 적용하고, 같은 원본을 사용하는 모든 게임 코드에는 같은 런타임 복제본을 제공한다. `Status.objectData`를 읽는 기존 전투·UI 코드도 이 값으로 계산한다. CSV 누락·잘못된 열·중복 키·없는 리소스는 초기화 오류로 명확히 알리며, 조용히 오래된 SO 숫자로 돌아가지 않는다.

## 대상과 형식

최초 소스 조사에서 ObjectData 76개를 확인했다: 일반 몬스터 16, 보스 9, 소환체 11, 영웅 22, 병사 5, 루나 스킨 7, 기타/타워 6. 실제 최종 숫자는 내보내기와 검증 로그를 기준으로 한다.

CSV는 UTF-8, 첫 행 헤더, 소수점 `.` 형식이다. 쉼표, 따옴표, 줄바꿈이 들어간 필드는 표준 CSV 인용을 지원한다. 시스템 언어가 독일어·한국어여도 숫자 파싱 결과가 달라지지 않는다.

| 열 | 의미 |
|---|---|
| `RowId` | Game Framework에서 사용하는 고유 양의 정수. 기존 행 번호를 재할당하지 않는다. |
| `Key` | 원본 SO의 `.meta` GUID. 에셋 이름이 바뀌어도 식별은 유지된다. |
| `ResourcePath` | 확장자를 뺀 Resources 경로. 이름이 같은 다른 몬스터도 혼동하지 않는다. |
| `ID` | 기존 게임의 ObjectData.ID 값. Game Framework RowId와 별개이며 기존 값을 그대로 보존한다. |
| 나머지 31개 | 레벨/카드/등급 기본값, 진영/유닛 종류/공격 종류, 체력·마나·속도·방어·공격·스킬·치명타·성장 계수 등. 정확한 스키마는 `LunaUnitBalanceRow.BalanceFields`에 명시된다. |

직접 바꿀 수 있는 밸런스 열은 총 32개이며 에셋 참조 열은 포함하지 않는다. NaN/Infinity, 정의되지 않은 enum, 잘못된 bool, 중복 RowId/Key/ResourcePath는 거부한다. 기존 수치를 임의로 보정하거나 게임 밸런스를 재설계하지 않는다.

## 편집과 갱신

- 최초 한 번: Unity 메뉴 `Luna > Data > Export Initial Unit Balance CSV` 또는 CLI `-executeMethod LunaUnitBalanceExporter.ExportInitialTable`. CSV가 이미 있으면 덮어쓰기를 거부한다.
- 이후 밸런스 조정: CSV의 해당 행 수치를 편집하고 `Luna > Data > Validate Unit Balance CSV`를 실행한다.
- 유닛 에셋 추가·이름/폴더 이동: `Luna > Data > Sync New Unit Sources and Paths`. 기존 수치·RowId는 보존하고, 새 GUID 행과 변경된 리소스 경로만 반영한다. 기존 CSV 백업은 `Logs/Revival/BalanceBackups`에 저장한다.
- 유닛 삭제: 기존 행의 GUID가 사라지면 자동으로 삭제하지 않는다. 의도한 삭제인지 확인한 뒤 참조 에셋과 CSV 행을 함께 정리한다.
- `.meta`는 유지한다. 새 GUID를 만들면 새로운 유닛 정의로 취급된다.

UI용 `LunaDataTable`과 호환 SO 뷰는 남아 있지만 실제 수치 원본은 Game Framework 데이터 테이블이다. 외부 서버/원격 패치에서 CSV를 다운로드하는 기능과 배포 중 데이터 버전 마이그레이션은 이번 로컬 복구 범위에 포함하지 않는다.
