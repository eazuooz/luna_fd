# Toybox 작성 방식으로 루나 재작성 — 인수 기준

작성일: 2026-11-08. 이 문서는 사용자가 승인한 **`D:/Github/ToyboxNightmare`의 실제 게임 작성 방식에 맞춘 루나 재작성**의 완료 기준이다. 이전 프레임워크 어댑터 전환과 구분한다. 승인 전 보존한 기준 커밋은 `c1e6d6bd`이며, 현재 작업 브랜치는 `codex/native-framework-hero-repair`다.

아래 강한 원래 기준은 모두 **미검증**으로 시작했으며, 실제 근거가 충족하는 범위만 인정한다. 구현 파일이 존재하거나 이름이 바뀌었다는 이유로 통과 처리하지 않는다. 이전 [QA 원문](Archive/Revival-QA-before-native-rewrite.md)의 성공 기록은 이전 구현의 증거다. [Revival-QA.md](Revival-QA.md)는 현재 구조의 실제 증거와 남은 검수를 기록한다. 원래 E/F/H 등의 체크박스는 요구의 출처로 보존하며, 더 강한 미실행 절을 포함한 항목을 일괄 체크하지 않는다. 완료한 실용 검수 단위와 각 한계는 아래에 별도로 표시한다. 실행은 담당자가 숨김 배치 프로세스로 수행한다. 데스크톱 조작, 물리 입력, 청감, 모바일 검증은 이 문서의 자동 검사 범위가 아니다.



## 현재 완료한 실용 검수 단위

- [x] r04 동일 RunId·동일 v2 지문으로3회 Audit15/Smoke8/Matrix442 완료. 각442개 이름/성공/검사 수에 누락·중복 없고, 각 회차 실제 재사용44건을 별도로 셌다.
- [x] 고의 소유권 실패 및 저장 손상/실제 파일 잠금의 결과를 증명한 뒤 정확한 진단·경고 발생 횟수를 대조했다. 예상 밖 경고0.
- [x] 최종 C clean Windows BuildReport0/0, 새 Player seed2/verify8,302.3273초11회 순환·exact data 사용/소유 수량, DSP·일반 입력·오프스크린3해상도 및 실제 PNG 검토 완료.
- [x] 최종 소스 `e2002579392869ec4de604d96cd3193b756025093171c2aae6dfe11f670364dd`/22,368개를 모든 회차 전후/회차간 동일하게 확인했다.
- [ ] Editor native shader 할당5개/1,366바이트 및 Player 종료 UTP41,463/45,302바이트의 해결/귀속은 남아 있다.
- [ ] 모바일/IL2CPP 실제 실행, 물리 입력·청감·OS backbuffer, 모든 확률 분기/조합·강제 완료 순서 역전·무제한 soak는 입증하지 않았다. 원래 더 강한 E/F/H 기준과 아래 한계표를 그대로 유지한다.

최종 원본과 이전 실패 증거는 [Revival-QA.md](Revival-QA.md)에 연결했다. 기능 검수3회 완료와 모든 인수 이상 조건/메모리 문제 해결을 같은 결론으로 취급하지 않는다.

## 개별 안정화 실행의 증거 — 이력

아래는 개별 실행 결과이며 최종 완료 판정이 아니다. 실행 사이에 실제 결함이 수정되었으므로 동일한 최종 소스 지문으로 실행한 3회 검수와 구분한다. Windows A/B 빌드와 A의 실제 Player 검사는 아래 QA 근거로 확보했다. 전체 연속 matrix 안정화와 최종3회는 별도 판정하며, r01 실패는 통과에 포함하지 않는다.

| 실행 | 확인한 실제 범위 | 결과와 원본 보고서 |
| --- | --- | --- |
| Native Audit 04 | 4개 씬·316개 프리팹·757개 데이터, 205개 native Entity 매핑, 26개 UI 주소, CSV 식별자/참조, 공급 Core DLL 일치, 이전 중계 구조 제거 | 10개 검사 통과, 오류·경고 0. `Logs/NativeRewrite/audit-attempt04.json` |
| Native Smoke 04 | 실제 Loading→Lobby, typed data/비동기 취소/재사용, 세 종류 native 콜백 실패와 정확히 한 번 반환, 격리 저장 복구, 일반 전투/입력/사망/재소환/같은 씬 재진입/종료 | 7개 검사 통과. 손상 저장 복구용 경고 2개와 의도된 실패 진단 3개를 따로 보존. `Logs/NativeRewrite/smoke-runtime04.json` |
| Heroes 01 | 22명×7등급의 154개 실제 스킬/패시브 사례. HP·회복·버프·소환·native FSM·사망 시 소환 정리와 검토된 수치 오라클 | 부트 포함 155개 통과, 오류·경고 0. `Logs/NativeRewrite/heroes-runtime01.json` |
| Motion 01 | 실제 Raccon 회전 공격/중복 요청/전투 경계, 적 넉백 착지, 사망 중단과 새 ID 재사용 | 부트 포함 2개 통과, 오류·경고 0. `Logs/NativeRewrite/motion-runtime01.json` |
| Buffs 01 | 3초/5초 중첩 만료, Pochi 5초 방어, 실제 피해 방지·반사, 무적, 출혈/중독 횟수, 저주 상한/정화, 중단 후 재사용 | 부트 포함 2개 통과, 오류·경고 0. `Logs/NativeRewrite/buffs-runtime01.json` |
| Skins 01 / 진단 02 / 03 | 첫 실제 공격 뒤 사망, 7개 플레이어 스킨 수명 검사 | 실패 보존. 오래된 Spine attack 완료가 death를 idle로 덮는 결함과 Skull의 잘못된 native 기반 타입을 발견·수정했다. 이후 Full All 01에서 7개 스킨 모두 통과. `Logs/NativeRewrite/skins-runtime01.json`, `skins-basic-diagnostic-runtime02.json`, `skins-runtime03.json` |
| Full All 01 | 부트·소유권·154개 영웅·회전/버프·7개 스킨·첫 3개 플레이어 스킬 | 169개 통과 뒤 Thunder 검사에서 실패, 뒤 사례는 미실행. 7개 스킨의 공격/사망/재사용과 Dragon 펫은 모두 통과. 테스트가 씬 진입 전 설정한 8배속을 씬이 정상적으로 1.5배속으로 바꾼 사실을 누락했으며, 실제 이전 배율을 기록하는 검사로 수정했고 skills-runtime03의 기본 Thunder 사례와 lifecycle-runtime03의 취소 회귀가 통과했다. `Logs/NativeRewrite/all-runtime01.json` |
| Full All 02 | 부트·소유권·실제 미등록 리소스 실패와 유효 주소 재시도 | 4개 통과 뒤 새 loadout 검사에서 실패. Preview 모드는 전투 FSM/능력을 만들지 않는 설계인데 검사가 그 모드를 사용했다. 실제 전투 전 Staged 모드로 고쳤고 Full All03의 해당 loadout 사례가 통과했다. 미등록 주소의 예상 InvalidKey 오류와 NotExist 경고는 원문 보존. `Logs/NativeRewrite/all-runtime02.json` |
| Full All 03 | 소유권, 237개 영웅 사례, 7개 스킨, 14개 플레이어 스킬, 회전/2개 버프 검사, 앞선 45개 일반 객체 사례 | 311개 통과 뒤 SkeletonMagicianLeader의 실제 스킬 관찰 시간 초과로 실패. 이후 객체/투사체/보상/조합/던전/스테이지/UI 검사는 이 실행에서 미완료. 예상 실패 오류 4개와 주소가 일치하는 NotExist 경고 1개를 보존. `Logs/NativeRewrite/all-runtime03.json` |
| Actors 01 / Projectiles 01 | 58개 일반 객체·배경 사례와 16개 투사체의 실제 명중/중복 방지/오래된 대상 ID 거절 | 각각 부트 포함 59개/17개 통과, 오류·경고 0. Magician은 자연 스킬 진행 중 두 번째 요청을 겹친 검사 문제였으며 첫 트랙 완료 후 두 번째 실제 결과를 검사했다. `Logs/NativeRewrite/actors-runtime01.json`, `projectiles-runtime01.json` |
| Rewards 01 | 실제 드롭/수집/보상 사례 | 부트 포함 4개 통과 뒤 무한 모드에서 실패. 비동기 SKILL 폼 로드가 끝나기 전에 플레이어가 사망한 실제 UI 수명 문제를 발견했다. 이후 Rewards02와 Lifecycle03에서 수정 및 실제 사망 중 재개방/새 ID 연결 회귀가 통과했다. `Logs/NativeRewrite/rewards-runtime01.json` |
| Native Audit06 / Skills03 | native SkillBalance32행/576필드·장비 진행도/스냅샷/원본 보존, 14개 기본 스킬 | 각각13개/16개 통과, 오류·경고0. `Logs/NativeRewrite/audit-06.json`, `skills-runtime03.json`. 새 linker와 레벨 효과 복구는 이 실행 뒤 추가됨. |
| Rewards02 / Lifecycle03 | 실제 보상 정산, HUD 재개방·pending 소환 유지, 죽은 플레이어의 스킬 폼 재개방과 새 ID/장착 스냅샷 연결 | 각각5개/4개 통과, 오류·경고0. `rewards-runtime02.json`, `lifecycle-runtime03.json` |
| Combos03 / Dungeons03 / Stages03 / UI03 | 3종 혼합 전투, 작성된 던전6종, 일반 스테이지30개, 26UI주소와 로비/모달 수명 | 부트 포함 각각4/7/31/29개 통과, 오류·경고0. `combos-runtime03.json`, `dungeons-runtime03.json`, `stages-runtime03.json`, `ui-runtime03.json` |
| Audit08 | 운영4씬+설정 템플릿1개, 프리팹316·데이터757 및 기존 native/CSV/linker 검사 |15개·오류/경고0. `audit-08.json/.log`. r03 1회차 wrapper PASS는 이후 저장 경고로 불인정. |
| Audit07 / Ownership04 | linker의 실제 타입·필드와 설정 범위, native 요청의 이동/오라 스냅샷·실패·반환 | Audit14개 오류·경고0; Ownership5개 통과, 의도한NotExist경고1개·실패진단4개 보존. `audit-07.json`, `ownership-04-runtime.json` |
| Skill-levels01 | 경계8개와 실제LV20 앞선9개 스킬 | 부트 포함18개 성공 후Demi에서 기대481.5 대비781.5 피해로 실패. 격리되지 않은 타워/잔여탄 등의 외부 피해 개입 가능성은 있으나 추가300의 공격자는 과거로그만으로 확정하지 않음. 같은 기대값으로 격리한02에서 전체23개 통과. `skill-levels-01-runtime.json` |
| Skill-levels02 / Skills04 / Buffs04 / Motion04 / Skins04 | 경계8개+실제LV20스킬14개, 기본스킬14개와nativeCSV, 장비/버프 순서, 회전·7스킨 | 각각23/16/8/2/8개 통과, 오류·경고0. `skill-levels-02-runtime.json`, `skills-04-runtime.json`, `buffs-04-runtime.json`, `motion-04-runtime.json`, `skins-04-runtime.json` |
| Full All04 | 검사기 중첩 완료 시 추가 프레임을 제거한 후 전체 native 사례 | **442개/442개 통과**, 오류0·의도한NotExist경고1·기대한 실패진단4. 전후 v2 소스928069a2002d7e4e8ed5d388a8db142851f6f8272f961bfbfcbfddeb73677dee/22,367개 일치. `all-04-runtime.json`, `all-04.json/.log`, `all-04-source-before/after.json`. 최종3회와 별도 안정화 실행. |

[Player-Skill-Level-Rules.md](Player-Skill-Level-Rules.md)가 새 스킬 성장 검사의 독립 기대값 근거다. 현재 source에 추가한 `skill-levels`는 8개 경계 스냅샷(4/5/9/10/14/15/19/20, 추가24/25상한)과14개 스킬의 실제20레벨 결과를 분리한다. 01의Demi 실패 기록은 보존하고, 타워/잔여탄을 격리한02에서 경계8개/24·25상한과14개LV20 실제 결과 모두 통과했다. 02의 실제 타워 관찰은Arrow100이며 이전 실패의 공격자 원인을 소급 확정하지 않는다. 새 `link.xml`의 type/field/configuration coverage는 Audit07에서 통과했다.

154개 기본 사례의 2.8초 관찰을 장시간 주기형 궁극기의 검증으로 확대하지 않는다. Ria/Matilda/Clare/Tira/Lena/Joe/Lene의 장시간 발동·수명·등급 경계는 17개 자연 타이머 사례로 별도 실행해 Full All 03에서 통과했다. 실제 미등록 주소의 비동기 실패/유효 주소 재시도, 서로 겹친 플레이어 요청의 불변 loadout 스냅샷, 서로 다른 판의 파티 효과 격리 검사도 같은 실행에서 통과했다. 전체 matrix 실행 중 발견하는 실패와 수정 이력은 원본 보고서에 남긴다.

`heroes/reuse-{이름}/{순서}-{등급}`의 66개 사례는 22명 각각 ULT2→STAR1→ULT2로 실제 Show/스킬/사망/Hide를 수행해 기본 계수·저항·상태·소환과 native FSM 회수를 확인했다. Full All 03의 44개 등급 전환 모두 native 풀이 실제로 직전 Unity 인스턴스를 재사용했으며, 풀의 선택을 강제로 바꾸지 않았다. 영웅 그룹의 총 237개 사례(154+66+17)는 통과했지만 아직 최종 동일 소스 3회 검수 결과는 아니다.

### 네이티브 개별 재실행 06/03 — 추가 증거

`audit-06.json`은 13개 검사, `skills-runtime03.json`은 부트·32행 SkillBalance/장비 스냅샷·14개 실제 스킬을 포함한16개 검사, `lifecycle-runtime03.json`은4개, `combos-runtime03.json`은4개, `dungeons-runtime03.json`은7개, `stages-runtime03.json`은31개, `ui-runtime03.json`은29개 검사를 모두 통과했다. 경로는 전부 `Logs/NativeRewrite/`이며 오류·경고는 각각0이다. 이전 lifecycle02의 종료 중 UI 부모 변경 실패는 원본 로그에 남기고, 수정 후03 프로세스 결과와 구분한다.

이후 발견한 이동/오라 요청 스냅샷, 플레이어 스킬 레벨 성장, 장비와 시간제 버프의 계산 순서 수정을 포함한 최종 소스는 별도 재검증 대상이다. [플레이어 스킬 레벨 규칙](Player-Skill-Level-Rules.md)은14개 원문 성장 약속과 복구 해석, 8개 경계 스냅샷 및 실제 LV20 결과 검사를 명시한다. 이동/오라 스냅샷은 Ownership04, 성장 경계8개 및14개 실제LV20 효과는 Skill-levels02, 기본 스킬·버프 적용 순서·회전·스킨은 각04에서 통과했다. 현재 Windows A/B·Player와 전체 최종 동일 소스3회는 별도 판정한다.

## 1. 구조상 필수 조건

| ID | 완료 조건 | 필요한 증거 |
| --- | --- | --- |
| A01 | 범용 `EntityLogic`이 기존 MonoBehaviour 모듈을 찾아 순서대로 호출하는 중계 구조와 `LunaEntityBehaviour`를 제거한다. | 운영 코드·프리팹 검사와 실제 Entity 콜백 추적을 함께 확인한다. 이름만 바꾼 모듈 배열/리플렉션 호출도 검사한다. |
| A02 | 플레이어·일반 유닛·소환체·발사체·효과마다 역할에 맞는 `EntityLogic`이 실제 초기화·표시·갱신·숨김을 소유한다. | 실제 `EntityComponent.ShowEntity`로 생성한 각 역할의 Logic, 상태 전이, 효과 결과를 관찰한다. 공유 기반 클래스는 데이터 수명 같은 공통 책임만 갖는다. |
| A03 | 스폰의 위치·회전·능력치·등급·진영·발사자/대상·피해·수명은 해당 역할의 typed `EntityData`에 요청 전에 준비한다. | 로드가 지연되어도 첫 `OnShow`/FSM `OnEnter`부터 완전한 값으로 동작한다. 반환된 GameObject에 뒤늦게 값을 채우는 호출부가 없다. |
| A04 | 개별 전투 객체가 native `FsmComponent`를 통해 자신의 FSM을 만들고 정리한다. | 같은 Logic 타입 여러 개의 독립 상태 전이, 숨김 후 FSM 제거, 재사용 후 새 상태 시작을 확인한다. Unity Update와 FSM의 중복 전투 갱신이 없다. |
| A05 | native `Procedure`가 판을 생성·갱신·종료하고 판 단위 전투 객체/규칙을 소유한다. | 승리·패배·재시도·다음 스테이지·중도 이탈 때 이전 판의 스폰/보상/타이머/이벤트가 더 실행되지 않는다. |
| A06 | 화면은 `UIFormLogic`과 `UIComponent`의 열기·닫기·가리기·복원 수명을 따른다. | 실제 폼 로드 완료/실패, 빠른 열기/닫기, 팝업 겹침, 탭 전환과 판 종료를 검사한다. 별도 UIBase/독자 스택으로 중복 관리하지 않는다. |
| A07 | Everyday 코드·어셈블리·직렬화 참조·호환 어댑터를 운영 프로젝트에서 제거한다. | 소스/asmdef/프리팹/씬/설정 검사, 새 프로세스 전체 컴파일, 누락 스크립트·GUID·주소 검사가 모두 통과한다. |
| A08 | 저장 진행도와 정적 밸런스/아트 데이터, 판별 mutable 상태를 분리한다. | 스폰 데이터의 수치 변경이 원본 SO/CSV나 다른 인스턴스에 전파되지 않고 저장·재시작이 승인된 데이터 계약을 보존한다. |

상속 이름·파일 개수·네임스페이스의 부재는 A01–A08의 필요 조건일 수 있으나 행동 검증을 대체하지 않는다. Unity 표시·입력용 컴포넌트 자체를 금지하지 않는다. 전투 규칙과 수명 소유권이 해당 native Logic/Procedure에 있어야 한다.

## 2. 비동기 생성과 EntityData 소유권

Toybox의 관찰된 계약은 `Create → ShowEntity → OnShow가 소유 → 파생 정리 → base.OnHide에서 ReferencePool.Release`다. 로드 실패는 Entity가 데이터 소유권을 잡지 못한 별도 경로다. 취소 시 성공/실패 이벤트가 반드시 오는지는 추정하지 않고 실제 제공된 프레임워크 동작을 검사한다.

| 단계/결과 | 소유권과 완료 조건 |
| --- | --- |
| 요청 준비 | 팩토리로 취득한 typed data의 모든 값과 요청 ID를 먼저 설정한다. 대기 요청 수/소유 판 ID도 Show 전에 등록한다. |
| 비동기 로드 대기 | 요청 데이터는 재사용·반납하지 않는다. 호출부가 다른 요청을 해도 첫 요청의 값은 유지된다. |
| 정상 OnShow | Entity ID와 data ID가 일치한다. Logic이 소유권을 한 번 취득한 후 데이터로 상태를 구성하고 마지막에 FSM을 시작한다. |
| OnShow 전 실패 | 데이터와 대기 예약을 정확히 한 번 반환한다. 실패한 필수 플레이어 스폰은 영구 대기 대신 명시적 판 종료/복구로 이어진다. |
| Entity 등록 후 실패 | failure 이벤트가 와도 이미 Logic이 소유한 데이터를 실패 처리기가 다시 반환하지 않는다. 등록된 실패 Entity를 정리한 뒤 데이터가 한 번 반환된다. |
| pending 취소/판 종료 | 현재 로드 목록만 지우고 끝내지 않는다. 이후 도착할 완료/실패를 처리할 책임과 데이터 해제를 보존한다. 이전 판의 객체가 다음 판에 편입되지 않는다. |
| 정상 Hide/중복 Hide | 충돌·수명 만료·사망·판 정리가 겹쳐도 한 번 회수한다. FSM/코루틴/구독/자식 관계를 먼저 정리하고 borrowed data 참조를 지운 뒤 반환한다. |
| ReferencePool 재취득 | Clear가 ID·대상·소유자·피해·수명·진영 등 모든 mutable 필드와 기본값을 복원한다. 이전 요청의 데이터가 새 요청에 섞이지 않는다. |

### 필요한 실제 회귀 검사

- [ ] **E01 — 지연 초기화:** 실제 native Show 요청을 여러 프레임 대기시킨다. 첫 갱신 전에 지정한 위치/등급/진영/피해가 적용되고 기본값으로 한 번 공격하는 프레임이 없다. 즉시 캐시 성공 경로도 같은 결과다.
- [ ] **E02 — 호출자 값 독립:** 서로 다른 두 요청을 겹쳐 보내고 로드 완료 순서를 바꾼다. 각 요청이 자기 데이터와 대상 ID를 사용하며 요청 전 원본 데이터는 변하지 않는다.
- [ ] **E03 — 로드 전 실패:** 존재하지 않는 테스트 주소로 실패시킨다. 해당 ID의 failure가 정확히 한 번 처리되고 pending 수와 EntityData 사용 수가 기준값으로 돌아온다. 재시도는 정상 성공한다.
- [ ] **E04 — 소유권 취득 후 실패:** native 경로의 테스트 전용 Logic이 소유권을 잡은 후 의도적으로 실패한다. 실패 처리와 Hide가 이중 반환하지 않고 등록된 Entity/FSM이 남지 않는다.
- [ ] **E05 — pending 상태에서 판 이탈:** 플레이어/적/발사체/소환체 로드가 끝나기 전에 Procedure를 떠난다. 다음 판 진입 후 이전 ID의 표시·피해·보상·소환 완료 통보가 없다. 모든 요청이 성공/실패/취소 중 하나로 종결된다.
- [ ] **E06 — 동시에 같은 프리팹 요청:** pending도 스폰 상한에 포함한다. 동일 주소를 여러 스폰 지점에서 요청해도 각 요청의 예약이 성공과 실패 양쪽에서 풀린다.
- [ ] **E07 — 중복 회수:** 같은 프레임에 명중·수명 만료·판 이탈을 겹친다. native Hide/data Release/보상은 각 한 번이며 예외가 없다.
- [ ] **E08 — 재사용 객체와 새 ID:** 같은 풀 인스턴스를 다른 등급/진영/타깃 데이터로 재사용한다. 이전 ID를 추적하던 유도탄/펫/타이머가 새 생명주기의 객체를 공격하거나 소유하지 않는다.
- [ ] **E09 — 소유자 선행 소멸:** 자식/펫/무기 로딩 중 소유자가 사라진다. 늦게 도착한 자식이 재사용된 다른 소유자에 붙지 않고 정리된다. 부착된 자식도 소유자 숨김 때 회수된다.
- [ ] **E10 — 데이터 풀 잔여:** 정상·실패·취소·반복 판 종료 뒤 pending0과 판 전환/로비 표시 완료 뒤 concrete EntityData 타입별 ReferencePool 사용 수가 현재 살아 있는 native 소유자 수와 정확히 같다. 차이가 있으면 타입·acquire/release 수·종결 경로를 남기고 실패한다. 사용하지 않는 풀 캐시 개수의 증가는 허용하며 0을 강제하지 않는다.

로드 지연은 native Show를 우회해 `OnShow`를 직접 호출하는 방식으로 흉내 내지 않는다. 필요한 경우 Editor/Development 전용의 제한된 리소스 완료 지연 장치를 통해 **실제 리소스·native Entity 경로**를 통과한다. 장치의 API는 런타임 계약 확정 후 결정한다. 실패 주입은 ID/단계가 일치하는 의도된 이벤트만 별도 기록하고, 원본 로그를 보존한다. 다른 오류·경고를 전역 필터로 숨기지 않는다.

## 3. FSM, Procedure, UI 수명 검사

- [ ] **F01 — 첫 상태:** typed data가 적용된 뒤 첫 FSM 상태가 시작된다. 이동·공격·사망의 실제 결과와 상태 전이 로그를 연결한다.
- [ ] **F02 — 다중 인스턴스:** 같은 Logic 타입 3개 이상이 서로 다른 대상/상태를 유지한다. 한 객체의 상태 전이가 다른 객체의 FSM을 바꾸지 않는다.
- [ ] **F03 — Hide 중단:** 스킬/사망/이동 중 Hide 이후 해당 FSM·타이머·Spine 콜백이 더 실행되지 않는다. 재사용은 초기 상태에서 시작하고 native FSM 수가 누적되지 않는다.
- [ ] **F04 — 갱신 횟수:** 정해진 native 경과시간 동안 이동 거리/쿨다운 감소/지속 피해 횟수가 한 경로의 결과와 일치한다. Unity Update와 native Update 양쪽 호출로 두 배 진행되지 않는다.
- [ ] **P01 — 판 생성/종료:** Loading → Lobby → 일반/보스/무한 전투 진입과 승리/패배/재시도/다음 판을 실제 Procedure로 실행한다. 각 판의 주인과 종료 시점을 기록한다.
- [ ] **P02 — 판 보상:** 실제 타워 사망으로 결과 화면과 보상을 만들고 반복 이벤트에도 보상이 중복되지 않는다. 이전 판이 늦게 보낸 이벤트는 새 판을 종료하지 않는다.
- [ ] **P03 — 전환 중 입력:** 로딩/종료 중 중복 진입·재시도·스킬 입력이 중복 판/플레이어/소모를 만들지 않는다. 허용/거절 결과가 명시적이다.
- [ ] **P04 — 반복 전투:** 30개 일반 스테이지의 실제 구성과 모든 던전 모드 진입을 확인한다. 일부 대표 판은 자연 전투부터 종료까지 실행하고 강제 종결 검사와 구분한다.
- [ ] **U01 — 실제 폼:** 모든 운영 주소가 실제 UIFormLogic으로 로드된다. 열린 폼은 UIComponent가 보유하고 게임 상태 구독을 닫을 때 해제한다.
- [ ] **U02 — pending 닫기:** 폼을 여는 도중 닫고 다른 판으로 이동한다. 이전 폼이 뒤늦게 나타나거나 새 판 이벤트에 반응하지 않는다.
- [ ] **U03 — 일시 숨김과 종료:** 캐릭터 영웅/병사/전체 탭의 임시 비활성은 재사용 가능한 미리보기의 영구 회수가 아니다. 폼 종료는 해당 미리보기를 실제 회수한다.
- [ ] **U04 — 비동기 의존:** HUD 로드 순서를 바꿔도 플레이어/소환 버튼/진행 표시가 준비되지 않은 폼 참조를 사용하지 않는다. UI 실패는 무한 대기 대신 진단·복구 결과를 남긴다.
- [ ] **U05 — 팝업/일시정지:** 덮기·복원·닫기를 반복해도 전투 시간 배율, 입력 허용, 버튼 구독이 원래 값으로 돌아온다.

## 4. 영웅·전투 행동의 별도 기준

새 검사는 `LunaNativeValidation` partial들로 다시 작성했다. 이전 `LunaRuntimeValidation`/`LunaHeroRankRegression`/`LunaDropRegression`은 동기 Spawn와 제거된 Status 모듈에 의존하므로 운영 검증 경로에서 퇴역했다. 새 기대값은 [Gameplay-Open-Issues.md](Gameplay-Open-Issues.md)의 검토된 규칙과 실제 결과를 대조한다.

- [ ] **H01 — 독립 기대값 표:** 22개 영웅 × 7개 직렬화 등급의 154개 행에 발동 조건, 대상 진영/종류/범위, 피해/회복/버프 수치, 확률, 지속시간, 소환 수/상한, 취소 규칙과 근거를 기록한다. 2/4등급이 별칭이어도 실행한다.
- [ ] **H02 — 실제 결과:** 피해는 대상 HP 감소, 회복은 부상 대상 증가와 최대 HP 제한, 버프는 지정 계수/실제 효과, 소환은 지정 프리팹·개수·진영·초기화·수명으로 검증한다. 객체 수 변화나 예외 부재만으로 효과를 입증하지 않는다.
- [ ] **H03 — 제외 대상:** 잘못된 진영·공중/지상 분류·타워·죽은 대상·범위 밖 대상의 비영향을 함께 확인한다. 타깃 ID가 소멸한 경로도 실행한다.
- [ ] **H04 — 확률과 수치:** 고정 시드로 성공/실패 분기 둘을 재현한다. 기대 수치는 구현의 private 필드를 그대로 읽어 답으로 삼지 않고 승인된 규칙/데이터에서 계산한다. 시드 반복 횟수와 허용 오차를 기록한다.
- [ ] **H05 — 시간/취소:** 선택한 장시간 패시브·소환 수명은 실제 게임 타이머로 끝까지 관찰한다. 모든 등급마다 60초를 기다리는 대신 명시한 대표 장시간 사례와 154개 기본 발동 사례를 구분한다.
- [ ] **H06 — 등급 재사용:** 높은 등급 → 낮은 등급 → 높은 등급에서 동일 native 풀 인스턴스가 초기 계수·잠금·소환 소유권·콜백을 정상 재구성한다.
- [ ] **H07 — 회전 공격 경계:** Raccon의 실제 스킬 시작/중복 요청/대상 사망/숨김/재사용을 실행한다. 혼잡한 경계에서 영웅과 밀리는 적의 좌표를 매 프레임 측정한다. 기준은 카메라 화면이 아닌 승인된 전투 영역이다.
- [ ] **H08 — 넉백/버프:** 양 진영 경계의 넉백 착지, 타워 제외, 스턴/은신/무적/저주/성장/지속 피해의 만료·사망·재사용 후 복원을 실제 결과로 확인한다.
- [ ] **H09 — 입력/투사체/조합:** 플레이어 스킬과 장비, 실제 발사체 명중, 표본 동시 전투를 새 구조로 다시 실행한다. 발사 초기화만 확인한 사례를 명중 검증으로 기재하지 않는다.

수치/설명이 없거나 충돌하는 항목은 해당 행을 **판정 보류**로 표시하고 필요한 데이터와 영향 범위를 정리한다. 새 수치를 테스트 작성자가 임의로 정하지 않는다. 기존 초안에서 알려진 Roen 지속시간, Clare 회피 표현, Lene 대상, Ria/Lena 소환 규칙 등의 변경 사항은 전투 담당자와 기대값 표를 먼저 합의한다. 실제 자료의 완결성 확인 전 이 문서에 균형 수치를 고정하지 않는다.

## 5. 에셋·저장·Windows 회귀

- [ ] **D01:** 운영 씬/프리팹/메타/Addressables 전체의 누락·중복·잘못된 Logic/Data 매핑을 검사한다. 변경 전 GUID 보존 또는 의도된 교체 목록과 마이그레이션 결과를 기록한다.
- [ ] **D02:** 모든 배틀 생성 인자가 테이블/저장 데이터에서 typed EntityData로 전달되는 경로와 문화권 독립 파싱을 검사한다. SO/CSV 원본 및 다른 인스턴스는 변경되지 않는다.
- [ ] **D03:** 격리 저장 폴더에서 덱·스킬·스킨·재화·성장·아이템·진행도·소리 설정의 별도 Windows 프로세스 종료/재실행을 확인한다. 주 파일 손상/부재·백업 복원도 새 구조에서 다시 실행한다.
- [ ] **W01:** 전체 컴파일과 명시적 캐시 제거 Windows 빌드의 오류/경고, 프로세스 종료 후 native 진단까지 원본 로그를 검사한다.
- [ ] **W02:** 남은 JobTempAlloc의 그래픽 빌드/`-nographics` 빌드 A/B를 한 번씩 비교한다. `-nographics`가 타깃 셰이더 컴파일을 생략해 조용해진 결과는 해결로 인정하지 않는다. 빌드 타깃 shader 작업과 새 graphical Player의 실제 출력 증거가 필요하다.
- [ ] **W03:** 숨김 graphical Player에서 실제 BGM/DSP RMS의 켜짐·음소거·복원, 3개 해상도 오프스크린 PNG/주요 UI 경계, 실제 이동 처리, 최소 5분 반복 전투를 확인한다.
- [ ] **W04:** 출시 빌드에는 자동 실행 검증 경로가 활성화되지 않는다. 진단 빌드도 명시적 opt-in·절대 경로·격리 표식이 없으면 실제 저장 데이터를 변경하지 않는다.

오프스크린 출력은 실제 창/backbuffer와 동등하다고 주장하지 않는다. DSP RMS는 사람이 들었다는 증거가 아니다. 프로그램 이동 함수 검사는 물리 키보드 경로 전체를 증명하지 않는다. 유한한 스테이지·영웅·조합 검사를 전체 조합이나 전체 밸런스 검증으로 확대하지 않는다.

## 6. 현재 검증 도구

| 현재 파일/도구 | 재작성 후 필요한 조치 | 현재 상태 |
| --- | --- | --- |
| `Assets/Editor/LunaValidation.cs` | 운영 에셋 감사·빌드와 native runtime JSON 연결 | 새 Audit/Smoke 개별 통과; r04 Audit15/Smoke8/Matrix442 세 회차 및 최종 C 빌드 완료 |
| `Assets/Editor/LunaContentMatrix.cs` | `LunaValidation.ContentMatrix()`로 연결하는 CLI 별칭 | native 모드/격리 경로 사용 |
| `LunaNativeValidation.cs` 및 Ownership partial | typed 요청·native 콜백·Procedure 판·격리 저장 회귀 | Smoke 04의 실제 증거 확보 |
| `LunaNativeValidation.Heroes.cs` / CombatContracts | 154개 기본 사례와 회전/버프 검사 | 154개 기본+66개 재사용+17개 자연 장시간 사례의 개별 성공 근거 확보; r04 세 회차의 exact inventory와 실제 재사용44건씩 독립 확인 |
| `LunaNativeValidation.Content.cs` / PlayerContracts / Skills / UI / HudContracts | 적·소환·투사체·보상·조합·던전·스킨·스킬·UI 수명 실제 결과 | 수정 후 객체/투사체/보상/조합/던전/스테이지/UI 개별 실행 통과; 성장/기본스킬·버프/회전/스킨04 재검사 통과; r04 전체442개 세 회차 완료 |
| `LunaNativeValidation.Player.cs` / Offscreen | 새 Windows의 별도 프로세스 저장, DSP/렌더/이동/soak | Windows A seed2/verify8 통과, 실제301.9344초·11회 순환; r04 최종 C seed2/verify8·302.3273초11회 순환도 완료 |
| `Validate-Luna.ps1` | native matrix의 section/filter 전달 | `All`은 Audit+Smoke+ContentMatrix+BuildWindows; section/filter로 개별 실행 가능하며 빈 선택은 실패 |
| `Final-Review.ps1` | 새 최종 검수 실행표와 연결 | 각 회차에 Audit+Smoke+전체 ContentMatrix, 3회차 Build 및 새 Player seed/verify·audio/render/300초soak. 회차 전후 및 같은 RunId의 통과 회차 사이에 소스 지문 일치를 강제 |
| `Validate-Player.ps1` | `native-player`와 `LUNA_PLAYER_MODE`로 새 실행 파일의 seed/verify·음소거·렌더·soak 실행 | 격리 경로/프로세스 정책 유지; native-headless-A-20261108 실제2/8개 통과, 종료로그 오류·경고0 |

현재 `NoGraphics`는 빌드에도 전달된다. 기본값 변경은 W02 결과를 본 뒤 결정하며, `CleanBuildCache`와 `JobLeakDiagnostics`는 명시적 옵션으로 남긴다. 임의의 경고 억제나 Library 삭제로 결과를 바꾸지 않는다.

## 7. 인수 ID와 실제 증거의 대응

다음은 확보한 사례와 인수 기준을 연결한 중간 대응표다. 하나의 사례를 해당 ID의 모든 가능한 경합·순서·조건을 증명한 것으로 확대하지 않는다. 새 스킬 레벨/보존·저장 수정까지 포함한 r04의 최종 동일 소스3회 반복도 완료했다. 이는 아래 미실행 강한 절의 자동 통과를 뜻하지 않는다.

| 인수 ID | 이미 확보한 근거 | 남은 판정 범위 |
| --- | --- | --- |
| A01/A07, D01 | Audit07의 이전 중계 구조 부재·컴파일·GUID/주소/데이터 참조와 exact linker 범위 | 최종 동일 소스 재검수; IL2CPP 실행 검증 아님 |
| A02–A05, F01–F04 | FullAll03의 개별 native FSM/영웅237/스킨7 및 Actors01·Projectiles01 | 유한 사례 범위이며 모든 상태 조합을 자동 입증하지 않음; 최종 연속 matrix |
| A03/A08, E01/E02, D02 | Ownership04의 실제 겹친 loadout·이동/오라 요청, Skills03의 CSV32행/576필드·문화권·장비 스냅샷·저장/원본 보존 | Skill-levels02의 경계8개·실제LV20 전체14개도 성공; 강제 완료 순서 역전 등 실제 실행되지 않은 경합은 별도 조건 |
| E03/E04 | Ownership04의 실제 잘못된 주소/재시도, native OnShow 전후/OnHide 실패와 한 번 반환 | 동일 소스 재실행; 예상 오류 원문과 정확한 진단 목록 대조 유지 |
| E05/E06/E07/E09 | typed 요청 취소·소환 상한·중복 투사체 명중·Dragon 소유자/자식 수명의 개별 실제 사례 | 모든 종류의 pending 판 이탈/취소 경합을 하나의 테스트로 통과 처리하지 않음 |
| E08/H06 | 영웅66개 ULT2→STAR1→ULT2 실제 재사용 사례, 스킨/투사체의 새 ID 계약 | 최종 동일 소스 반복 |
| E10 | Ownership04 및 Skills03의1회·Combos03의3회 fixture 퇴장에서 exact data 사용/소유 수량 관찰 | Windows A 301.9344초·11회 로비 귀환에서도 exact 사용/소유 수량 일치; 무제한 세션 증명은 아님 |
| P01/P02/P04 | Smoke04, Rewards02, Dungeons03, Stages03 | 작성된30개 일반/6개 던전 범위; 자연 전투 완료와 강제 종결 구분 |
| U01/U03/U05, P03 | UI03의26주소 반복 열기·탭·모달·로비/성장 갱신 | 최종 재실행; 모든 입력 순열을 의미하지 않음 |
| U02/U04 | UI03 pending 열기/닫기, Lifecycle03 죽은 플레이어 상태로 열기·새 ID 재연결·타기팅 취소 | 판 전환을 겹친 모든 native UI 실패 경우와 동일하다고 주장하지 않음 |
| H01–H09 | FullAll03의237영웅/스킨/버프, Projectiles01, Combos03, Skills03의 각 named 사례 | Skill-levels02 및기본스킬/버프/회전/스킨04 성공, 최종 연속 matrix; 전체 밸런스/모든 조합 판정 아님 |
| D03/W01–W04 | 현재 구조의 격리 저장 Smoke·도구/opt-in 경로는 존재 | Windows A BuildReport0/0, 별도 seed2/verify8·오프스크린3개·DSP·301.9344초 통과; Editor native JobTemp5개/1366바이트는 잔존. graphics B도 동일 native 경고·raw 지문 일치, v2 기준 r01 1회차 실패·통과0회; r02 의도적 중단·미계산, r03 저장 경고로 불인정·중단 |

### 최종 검수에서 별도로 판정할 실행 단위

| 판정 단위 | 사용할 실제 근거 | 현재 상태 |
| --- | --- | --- |
| 운영 구조와 데이터 참조 | native 작성 구조 소스 검토 + Audit의GUID/주소/표/보존 검사 + 동일 알고리즘/소스 지문 | 저장 수정과 경고 게이트를 포함한 r04 Audit15를 세 회차 모두 확인. |
| 유한 콘텐츠 동작 | named442사례 전체, 단계별 실제 관측값과 정확한 예상 실패 진단 목록, 원본 종료 로그 | All04 442개 성공. 최종 회차의 성패와 별도. |
| 같은 물리 인스턴스의 등급 재사용 | 영웅66사례의 cycle2/3 관찰44개가 모두 `True`, 각 새 ID·계수·상태/FSM 결과 | All04와 별도로 r04 1·2·3회차 각각44개를 독립 확인했다. |
| 실제 Windows 실행 | 새 실행 파일의 별도 seed/verify, DSP RMS, 일반 입력,3개 오프스크린/눈으로 본 이미지,300초·매 귀환 exact payload 소유량 | A와 별도로 최종 C seed2/verify8·302.3273초11회 순환 및 새 PNG3개 검토 완료. |
| 종료 메모리 진단 | BuildReport와 분리한 완전 종료 native 로그, A/B 비교, 실제 Player 종료 | Editor JobTemp5개/1,366바이트는 **미해결**. A Player에서 미관측을 Editor 문제 해결로 바꾸지 않는다. |
| 최종 동일 소스3회 | 새 RunId의3개 summary·각 단계 보고서·전후/회차간 v2 지문 | r01 실패/r02 의도적 중단으로 미계산. r03 저장 경고로 불인정·중단; r04 세 회차 통과·3/3회 완료. native 종료 할당 미해결은 별도 기록. |

이 실행 단위가 완료되더라도 다음 표에 적은 더 강한 원래 기준의 미실행 절을 숨기거나 일괄 통과 처리하지 않는다. 원래 E/F/H 문구는 검증 요구의 출처로 그대로 보존한다.

### 전체 사례 수만으로 충족되지 않는 조항

All04에서442개 사례가 통과했어도 아래 더 강한 문장을 자동으로 충족했다고 표시하지 않는다. 이 표는 현재 검사 코드의 **유한 실행 범위**를 읽은 결과다. 운영 결함을 새로 확인했다는 뜻도, 미실행 조건을 삭제하거나 인수 요구를 낮춘다는 뜻도 아니다. 체크박스는 원래 조건을 유지하며 실제 관찰이 없는 조항은 미검증으로 남긴다.

| 기준 | 현재 구현된 검사와 확보 가능한 증거 | 아직 입증하지 않는 부분 |
| --- | --- | --- |
| E01/E02 | `SpawnOwnership`의 pending 즉시 취소·native 재표시, `PlayerLoadoutOwnership`의 같은 프리팹2개 겹친 요청과 완료 전 전역 값 변경. 완료 콜백 시 레벨/등급/진영/이동/장착 값 확인. | 로드를 의도적으로 여러 프레임 보류하는 장치, 두 완료 순서를 강제로 뒤집는 실행, 첫 FSM 틱의 모든 필드 추적은 없다. 겹친 요청은 완료 순서 역전의 증거가 아니다. |
| E03/E04 | 실제 미등록 주소→실패1회→유효 주소 재시도; OnShow의 소유권 취득 전/후와 OnHide의3종 실패에서 payload Clear1회·pending0·ID 부재·비활성/부모 분리 확인. | 테스트 FaultLogic이 생성하지 않는 실제 전투 FSM의 모든 초기화 실패 지점까지 주입한 것은 아니다. |
| E05/E06 | 개별 pending 취소, 판 정리, 겹친 같은 프리팹 요청, HUD를 닫은 동안 소환 완료, 실제 소환 수/소유자 정리 확인. | 플레이어/적/발사체/소환체 각각을 완료 직전에 붙잡고 Procedure 이탈시키는 교차 역할 행렬, 다중 스폰 지점 상한의 성공·로드 실패 양쪽 예약/환불 조합은 모두 주입하지 않았다. |
| E07 | 중복 SafeHide, 실제 투사체1회 명중과0.4초 뒤 추가 피해 없음, 실패 payload1회 반환을 검사한다. | 같은 프레임에 명중·자연 만료·Procedure 이탈 세 사건을 강제로 동시에 발생시키는 검사는 없다. |
| E08/E09 | 오래된 대상 ID로 생성한 실제 투사체가 새 ID 대상에 피해를 주지 않음; Dragon/영웅의 소유 자식과 타이머를 사망/Hide 후 정리. | 모든 펫/무기/자식 타입에 대해 로드 완료 직전 소유자 소멸과 같은 인스턴스 재사용을 강제한 것은 아니다. |
| E10 | 각 지정 fixture 퇴장과 Windows A의11회 로비 귀환에서 pending0 후 concrete payload 사용 수=살아 있는 native 소유 수를 비교한다. | 무제한 플레이 또는 모든 미실행 경합에서 누수가 없음을 증명하지 않는다. r01의 검사기 추가 프레임 실패 기록과 수정 검증은 별도 유지한다. |
| F01/F02 | native FSM 존재·지정 액터 실제 이동/공격/사망, 동시에 살아 있는 액터·서로 다른 판 효과 격리를 확인한다. | 동일한 구체 Logic3개 이상에 서로 다른 대상/상태를 주고 한 개만 전이시켜 나머지 상태 불변을 직접 대조하는 전용 검사는 없다. 첫 상태의 전체 콜백 순서 로그도 없다. |
| F03/F04 | Raccon 중단, 영웅/스킨 재사용, 주기형17사례의 종료 뒤4초 무효과, DOT 횟수·스킬 쿨다운·이동량 등 지정 결과 확인. | 모든 타이머/Spine 콜백과 모든 프레임률·시간 배율·중단 시점의 조합을 실행하지 않았다. |
| H01/H02/H03 | 22명×7등급154사례를 실행한다. Alice/Dren/Joe/Mei/Scarlet은 명시한 피해식, 다른 지정 사례는 양의 회복/피해·버프 계수·소환 개수 등의 결과를 확인한다. | 모든154행의 확률·지속시간·취소·대상 제외까지 채운 외부 완전 오라클 표는 없다. 모든 영웅×잘못된 진영/공중/타워/사망/범위 밖의 비영향 행렬도 없다. 개별 스킬 효과를 전체 타깃 규칙 검증으로 확대하지 않는다. |
| H04 | seed15031 고정. Clare/Lene/Rose는 최대20회의 실제 호출에서 효과 발생을 관찰하고, Alice의 실제 피해는 단일 또는 작성된3배 proc 중 하나를 허용한다. | 각 등급/확률마다 성공과 실패 분기를 각각 강제·확인하지 않는다. 관측된 proc는 확률 분포의 통계 검증이 아니며 허용된 두 결과를 모두 실행했다는 뜻도 아니다. |
| H05/H06 | 지정7명·17개 자연 타이머 사례; 22명 ULT2→STAR1→ULT2의66사례에서 새 ID/계수/상태/FSM 정리 확인. | 동일 Unity 인스턴스 재사용은 현재 `Require`가 아니라 관찰값이다. 각 최종 보고서의 `reused previous physical instance=True`44개를 실제로 세어야 같은 인스턴스 재사용을 주장할 수 있다. All04에서도44개 true/첫 생성22개 false를 직접 확인했다. 이 관찰을 이후 최종 실행에 이월하지 않는다. |
| H07/H08/H09 | Raccon 우측 경계의 실제 회전/넉백/사망/재사용, 지정 버프·DOT·장비 순서,16개 실제 투사체 명중, 명시한3개 파티 대결을 검사한다. | Raccon은 좌우 모든 진영 배치를 각각 실험하지 않았다. 모든 넉백/타워 제외/상태 조합과 전체 파티 조합의 결과를 뜻하지 않는다. |

근거 코드는 `LunaNativeValidation.cs`의 `SpawnOwnership`, `.Ownership.cs`의 `PlayerLoadoutOwnership/FailureOwnership/AssertEntityDataOwnershipBalance`, `.Heroes.cs`의 `HeroContracts`, `.HeroSchedules.cs`, `.CombatContracts.cs`, `.Content.cs`다. Runtime 보고서의 named 사례와 observations를 함께 확인한다. 모든 체크박스를 사례 총수에 따라 일괄 체크하는 완료 판정은 사용하지 않는다.

### 확정된 계약을 검사가 사용할 때 필요한 정보

테스트가 운영 설계를 대신 결정하지 않도록 아래 정보만 확정 후 받는다. 가능한 한 이미 운영에 필요한 API/이벤트를 사용한다.

1. 역할별 Logic/Data 타입, Show 요청 함수의 ID 반환·성공·실패·취소 계약과 소유 판 식별자.
2. 대기 요청/살아 있는 Entity/FSM/ReferencePool 사용량을 관찰할 공개 API 또는 읽기 전용 진단 값.
3. 전투 Procedure와 판 객체의 진입·종료·승리·패배·보상 이벤트 및 테스트 가능한 일반 입력 경로.
4. UI 주소/그룹과 폼 열기·닫기 실패 처리, HUD 준비 완료 신호.
5. 승인된 영웅 기대값 표와 분류 데이터. 확정되지 않은 수치는 별도 목록으로 전달.

## 8. 구현 후 추가 검수 3회

구현과 개별 회귀 수정이 끝난 시점의 소스/설정/에셋 지문을 먼저 고정한다. 이후 새 프로세스로 아래 3회를 실행한다. 중간 수정이 있으면 수정 범위·새 지문·재실행 근거를 남기며 이전 실패 기록을 삭제하지 않는다. `native-final3-20261108-r01`은1회차 Audit/Smoke 성공 뒤 ContentMatrix의33개 성공 후 Goldy/STAR1 퇴장 E10 검사에서 실패했다. 실제 오류는 `EntityData balance sampled during pending native loads`이며 이 회차를 최종 통과 횟수에 포함하지 않는다. `Guard`/`Independent` 검사기에서 중첩 IEnumerator 완료 뒤 추가 프레임을 보내 pending0 전제와 즉시 소유량 검사의 시점이 갈리는 원인을 확인해 중첩 완료/Dispose 뒤 불필요한 프레임 진행만 제거했다. 운영 게임 코드는 바꾸지 않았다. 수정 검증 뒤 새 RunId r02로 세 회차를 다시 시작했다. All04 안정화442개는 통과했지만 최종 회차 수에는 포함하지 않으며 당시 최종 통과 회차는0회였다. 실패 원문은 `Logs/Revival/FinalReview/native-final3-20261108-r01/pass-01/20261108-104016-261/`에 보존한다.

| 회차 | 필수 범위 | 결과 | 원본 로그/보고서/지문 |
| --- | --- | --- | --- |
| r01 시도 | Audit+Smoke+전체 ContentMatrix | **실패·통과 횟수 제외** | `Logs/Revival/FinalReview/native-final3-20261108-r01/pass-01/20261108-104016-261/` |
| r02 시도 | Audit+Smoke 후 ContentMatrix 실행 중 | **의도적 중단·통과 미계산**; testcase 실패 판정 아님 | `Logs/Revival/FinalReview/native-final3-20261108-r02/pass-01/20261108-110340-952/` |
| r03 1회차 | 운영4씬+설정 템플릿 Audit15, Smoke7, 전체 ContentMatrix442 | wrapper PASS였으나 저장 경고로 불인정 | `Logs/Revival/FinalReview/native-final3-20261108-r03/pass-01/20261108-112222-832/` |
| r03 2회차 | 새 프로세스 Audit+Smoke+전체 ContentMatrix, 실패/취소/재사용·저장 반복 | matrix 의도적 중단·통과 미계산 | `Logs/Revival/FinalReview/native-final3-20261108-r03/pass-02/20261108-113855-540/` |
| r04 1 | Audit15+Smoke8+전체 ContentMatrix442 | **통과**·예상 밖 경고0·전후 지문 일치 | `Logs/Revival/FinalReview/native-final3-20261108-r04/pass-01/20261108-115304-945/` |
| r04 2 | 새 프로세스 Audit15+Smoke8+전체 ContentMatrix442 | **통과**·예상 밖 경고0·전후/회차간 지문 일치 | `Logs/Revival/FinalReview/native-final3-20261108-r04/pass-02/20261108-120833-513/` |
| r04 3 | Audit15+Smoke8+전체 ContentMatrix442 + clean Windows 빌드/native 종료 진단 + 새 실행 파일 seed/verify·audio/render/300초soak | **통과**·새 Player/300초·최종 지문까지 완료; native 종료 집계 미해결 | `Logs/Revival/FinalReview/native-final3-20261108-r04/pass-03/20261108-122516-306/` |

각 보고서는 요청/Entity ID, 판 ID, Logic/Data 타입, 시작/종료 UTC, 실제 game-time/real-time, 전후 native 수, 의도된 실패와 예상 밖 오류, 실행한/건너뛴 사례를 남긴다. 어느 체크도 보고서 없이 `[x]`로 바꾸지 않는다.

r02는 기본 씬 템플릿의 고아 이전 Game 참조를 정리하기 위한 담당자의 명시적 중단이다. 종료코드만으로 특정 testcase 실패를 만들지 않는다. 새 Audit는 ProjectSettings의 `templateDefaultScene` 경로를 읽고 운영4씬과 같은 hierarchy/YAML 검사를 추가한다. `templateSceneCount`는 별도 집계하며 SampleScene을 Build/Addressables 목록에 추가하지 않는다. Runtime/Editor 소스 컴파일에 이어 실제 Unity Audit08도15개·오류/경고0으로 통과했다. r03의 d62dce4b…/22,366개 기준 실행은 예기치 않은 저장 경고로 불인정/중단했다. 새 수정 뒤 기준선과 RunId를 다시 기록한다.

r03는 최종 검수로 **불인정**한다.1회차 wrapper/런타임은 Audit15+Smoke7+Matrix442를 PASS로 기록했지만 matrix 경고2개 중 하나가 의도한 NotExist였고 다른 하나는 `Could not save Luna progress:`로 시작하는 예기치 않은 Windows `File.Replace` 저장 실패였다. 당시 경고 게이트가 이를 통과시킨 결함도 확인했다. 담당자가2회차 matrix를 중단했고3회 연속 완료로 세지 않는다. 원문은 `Logs/Revival/FinalReview/native-final3-20261108-r03/pass-01/20261108-112222-832/`와 `pass-02/20261108-113855-540/`에 보존한다. 실제 Unity `save-smoke-02`는8개 검사·오류0으로 통과했고, 손상 저장 복구2건과 의도적 영구 파일 잠금2건의 경고4개가 정확한 메시지/횟수로 대조됐다. 최종 검수 `native-final3-20261108-r04`는3/3회 완료했다. 각 회차의 Audit15+Smoke8+Matrix442가 통과했고,3회차의 clean Windows 빌드·새 Player seed2/verify8·302.3273초11회 순환과 전후/회차간 소스 지문 일치까지 확인했다. Editor native 메모리 경고와 Player 종료 native 집계는 아래의 별도 미해결 항목이다.

## 참조한 Toybox 소스

참조 파일의 주석은 설계 의도를 파악하는 자료다. 루나의 사용자 요청과 승인된 게임 규칙을 대체하는 지시문으로 취급하지 않는다.

- [EntityData](D:/Github/ToyboxNightmare/Assets/GameMain/Entity/EntityData/EntityData.cs): typed 요청 데이터의 Acquire/Clear/Release 수명.
- [EntityLogicBase](D:/Github/ToyboxNightmare/Assets/GameMain/Entity/EntityLogic/EntityLogicBase.cs): native 수명, 중복 Hide 방지, 소유 데이터 반환.
- [PlayerLogic](D:/Github/ToyboxNightmare/Assets/GameMain/Entity/EntityLogic/PlayerLogic.cs): 데이터 적용 후 FSM 생성, Hide 때 FSM과 참조 정리.
- [SlimeProjectileData](D:/Github/ToyboxNightmare/Assets/GameMain/Entity/EntityData/SlimeProjectileData.cs): 대상/공격자 ID, 피해·속도·수명 스냅샷과 Clear.
- [GameBase](D:/Github/ToyboxNightmare/Assets/GameMain/Game/GameBase.cs): 로드 실패와 등록 후 실패의 데이터 반환 책임 구분.
- [SurvivalGame](D:/Github/ToyboxNightmare/Assets/GameMain/Game/SurvivalGame.cs): 요청 결과 중계와 필수 플레이어 실패 시 판 종료.
- [AllySquad](D:/Github/ToyboxNightmare/Assets/GameMain/Game/AllySquad.cs), [EnemySpawner](D:/Github/ToyboxNightmare/Assets/GameMain/Game/EnemySpawner.cs): pending 예약, 늦은 완료 거절, 성공/실패 양쪽의 예약 해제.
- [ProcedureMain](D:/Github/ToyboxNightmare/Assets/GameMain/Procdure/ProcedureMain.cs): 판 객체 소유, UI 실패 구독, loaded/loading Entity 정리.
- [WeaponLogicBase](D:/Github/ToyboxNightmare/Assets/GameMain/Weapon/WeaponLogicBase.cs): 소유자 ID에 의한 부착과 늦게 도착한 고아 Entity의 회수.


새 r04 소스 기준선은 `e2002579392869ec4de604d96cd3193b756025093171c2aae6dfe11f670364dd`/22,368개다. 저장 회귀의 실제 결과와 이전 실패 fixture의 한계는 [Revival-QA.md](Revival-QA.md)의 저장 실패 수정 절에 기록한다. `save-smoke-02`의8개 통과는 최종3회 중 한 회차로 세지 않는다.


r04 최종 빌드 C는 BuildReport0/0으로 성공했지만 Editor shader-cache JobTemp5개/1,366바이트는 다시 관측됐다. native 메모리 경고 해결 기준은 미충족이다. Player·최종 지문까지 완료한 기능 검수는3/3회로 인정하되, Player 종료 UTP native 집계41,463/45,302바이트도 귀속 미확인으로 보존한다. 전체 로그 판정은 [Revival-QA.md](Revival-QA.md)의 빌드 C 절을 따른다.
