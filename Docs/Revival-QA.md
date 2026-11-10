# 루나 복구 검수 — 현재 native 재작성

2026-11-09 KST 현재, Unity `6000.6.0f1`, 브랜치 `codex/native-framework-hero-repair`. 아래 로그 시각/RunId는2026-11-08 UTC다.

기본 30층, 던전, 카드·골드 경제를 구성한 다음의 검수는 별도 문서에 기록한다. 이 문서에는 구조 복구와 메모리 수정 당시의 실행을 남긴다. [레벨 디자인과 검수](Level-Design-QA.md)

**native 재작성의 PC 복구 검수는 r04에서3/3회 완료했고, 이후 메모리·초기 종료 수정의 최종 추가 회귀도 완료했다.** 최종 v2의 Build/Early/Player302초를 통과하고 Matrix442는2026-11-08 16:19:56 UTC에 종료했다. 이후 전후 소스 지문 일치도 확인했다. 아래 r04 표는12:48:03 UTC까지의 이전 소스/실행 파일 이력이며 새 변경의3회 검수로 옮기지 않는다. 현재 상태는 다음 추가 검수 항목과 남은 제한을 함께 판정한다. 승인 전 체크포인트 `c1e6d6bd`는 원격 저장소에 보존돼 있다.

## 메모리 조사 이후의 최신 상태

- 종료 소유권 수정 source `1fd7d1d68ebfb4cd7dddd347f75724a999314a569a59534ba0a12d20fb6d766c`/22,370개에서 Audit15·Smoke8·Matrix442와 실제 Windows seed3/verify9를 통과했다. R01은302.208649초11순환이며 SO/Sprite 원본 참조의 실제 파괴, native Entity/FSM/handle/pending0, 두 번 종료의 불변성을 확인했다.
- 실제 warm build는 `-CheckNativeMemory`에서5개/1,366바이트로 거부됐다. `-RebuildShaderCache -CheckNativeMemory`의 cold05는 BuildReport0/0 및 완전 종료의 명시적 native 오류0으로 통과했다. 이는 가역적 캐시 백업을 쓰는 명시적 빌드 우회책이며 warm 재발 자체를 고친 것은 아니다.
- cold05 실행 파일의 gated R02도 seed3/verify9·native gate0과 새3해상도 캡처를 통과했다. 이 실행의 soak는설정10초/실제12.5649986초1회이며 R01의302초를 최신 빌드의 새 장시간 실행으로 표시하지 않는다. 담당자가 PNG3개에서 정상 배경/액터/HUD와 주요 잘림·분홍 재질·빈 화면 없음을 확인했다.
- Player의 귀속 미확인 UTP는40,123/43,962바이트로 남는다. 빈 Unity Player에도22,891바이트가 발생했지만 Luna 차액의 귀속이나 무해함은 증명하지 않았다.6000.6.3f1/6000.6.8f1 복제본도 같은 Editor5개/1,366바이트를 재현해 본 프로젝트 엔진은 바꾸지 않았다.
- **최종 v2의 Build07·Early R04·Full Player302초·전체 Matrix442·마지막 지문 대조를 모두 통과했다.** 전후 고정 소스는 `46a2a137e47d135cebe7515dbd5651bc077930f65e24b8e706c8cb61524c02fe`/22,372개다. Build07은0오류/0경고·native gate0/JobTemp0, Early는Start 전 FSM0→Procedure 동기 disable·3정상 프레임·반복 종료·native 사용량0·저장 파일 byte-identical을 실제 확인했다. Full seed3/verify9는오류/경고0,302.38324초11순환과 종료 직전SO116/Sprite4/Entity17/FSM8/handle41/pending load·spawn각1→모두0을 통과했다. 전후 Unity 총 할당 샘플249,630,323→222,722,001바이트, 종료 전 로비 Entity17개다. 두 번 종료의 Id0/취소 콜백0도 확인했다.
- 최종 v2의 새로운3해상도 PNG도 담당자가 정상으로 검토했다. 경계·배경·액터·HUD 관찰과 PNG SHA는 `Logs/NativeRewrite/memory-player-r04-visual-review.json`에 있다. native gate는Early/seed/verify 모두0이지만 UTP는각각40,123/40,122/43,962바이트로 남는다. 실행 파일·실제 Assembly-CSharp·제공 Core DLL의 해시는 `memory-final-artifacts-v2.json`, 독립 Player 대조는 `memory-final-v2-player-independent.json`에 있다.
- `memory-final-all-02{,-runtime}.json`은442개 고유 사례/검사 전부 성공, 오류0·정확히 검증한 고의 NotExist 경고1회다. 완전 종료 native gate0이며 Editor16:05:32→16:19:56 UTC에 종료했다. `memory-final-source-v2-{before,after}.json`의 알고리즘·SHA·22,372개가 정확히 같다. 패키지 생성물인 정확한 Addressables link.xml/meta2개는 실행 전 존재/후 부재를 이유·SHA와 함께 별도 기록했으며 authored `Assets/link.xml`은 지문 대상에서 빠지지 않았다.
- 독립 감사 `Logs/NativeRewrite/memory-final-v2-independent-audit.json`에서도442개 이름의 누락·추가·중복0, 영웅 관찰66건/같은 물리 객체 재사용44건, 고의 진단4개와 경고1회, 실제 프로세스 exit0을 확인했다.22,372개 파일의 경로·크기·SHA 항목도 모두 같았다. Matrix의 UTP34,274,687바이트는 별도 원문으로 보존했다.
- 마지막 Audit15·Smoke8는 게임 종료 코드가 같은 source7a837e…에서 실제 통과했다(`memory-final-audit-02.json`, `memory-final-smoke-02.json`). 다음 변경은 Early 검사기의 파괴 객체 판정 한 조건뿐이다. 최종46a의 offline 컴파일과 Build/Early/Player/Matrix를 새로 실행했고, Audit/Smoke를46a에서 다시 실행했다고 주장하지 않는다.
- source7a837e…의 Early R03은 종료 뒤 파괴된 Procedure의 enabled를 읽은 검사기 오류로 실패했으며 통과로 세지 않는다. 게임 코드는 그대로 두고 검사 조건만 보정한 새Early R04에서 위 전체 사례를 실제 확인했다. 이전1fd의442개 결과, R02짧은 Player 검사, 추출 메서드12개 probe와 최신 실제 실행은 서로 다른 근거로 보존한다.

상세 원문·캐시/엔진 통제 비교·후보 설치 경위는 [Memory-Investigation.md](Memory-Investigation.md)에 있다. 이번 추가 검수는 변경 후1회 전체 회귀와 대상별 build/early/player 검사이며, 이전442개/3회 결과와 합쳐 새 변경의3회 완료로 표시하지 않는다.

이전 어댑터 구조의 완료 문서는 [Archive/Revival-QA-before-native-rewrite.md](Archive/Revival-QA-before-native-rewrite.md)에 원문 그대로 보관했다. 그 문서의 기존 3회 검수, Windows 실행, 경고 수치는 현재 Toybox 방식 재작성의 통과 증거가 아니다. [Warning-Review.md](Warning-Review.md)도 이전 단계의 원인 조사 기록이다.



## r04 당시 완료 항목과 남았던 제한

- [x] Toybox 방식 native EntityLogic/typed EntityData/FSM·Procedure 판·UI 재작성, Everyday 런타임 제거와 참조/CSV/linker 감사.
- [x] 같은 RunId·동일 소스에서3회: 각각 Audit15 + Smoke8 + 이름이 고유한 Matrix442. 오류0, 예상 밖 runtime/Editor-captured 경고0. Smoke의4개·Matrix의1개 경고는 검증한 고의 실패 fixture와 정확한 메시지/횟수가 일치했다.
- [x] 세 회차 각각22명 영웅의66개 생성/등급 회전과 실제 같은 물리 인스턴스 재사용44건을 독립 대조했다. 모든 확률/게임 조합을 실행한 것은 아니다.
- [x] 최종 clean Windows 빌드 C: BuildReport 오류0·경고0. 실행 파일 `Builds/Revival/Windows/LunaDefense.exe`, 주요 바이너리 해시 `Logs/NativeRewrite/final-r04-binaries.json`; 포함된 Core DLL도 제공 패키지의 SHA-256과 같다.
- [x] C의 별도 Player seed2/verify8: 저장·재시작, 실제 DSP RMS, 일반 이동 입력 처리,1280×720/1920×1080/1024×768 캡처와 HUD 경계,302.3273초11회 순환 통과. 매 로비 귀환의 EntityData 사용량과 실제 native 소유량 일치. 담당자가 새 PNG3개도 정상 액터/배경/HUD로 확인했다.
- [x] 모든 회차 전후/회차간 v2 지문 `e2002579392869ec4de604d96cd3193b756025093171c2aae6dfe11f670364dd`/22,368개 동일. 문서 갱신은 authoring 지문 대상이 아니다.
- [ ] native 종료 할당 문제: Editor C의 shader-cache JobTemp5개/1,366바이트와 Player seed/verify UTP MemoryLeaks41,463/45,302바이트는 남아 있다. 둘은 별도 집계이며 프로젝트 액터에 귀속되지 않았다. 실행 중 무오류를 모든 native 누수 부재로 확대하지 않는다.
- [ ] Android/iOS·IL2CPP 실제 실행, 물리 키/OS backbuffer·청감, 무제한 soak, 모든 확률 분기/조합/완료 순서 역전은 미검증이다. [인수 문서](Toybox-Authoring-Acceptance.md)의 강한 미실행 조건은 체크하지 않는다.

| 최종 회차 | 실제 완료 시각 UTC | 성공 단계 | 증거 폴더 |
| --- | --- | --- | --- |
| r04 1 | 12:08:33 | Audit15·Smoke8·Matrix442 | `Logs/Revival/FinalReview/native-final3-20261108-r04/pass-01/20261108-115304-945/` |
| r04 2 | 12:25:16 | Audit15·Smoke8·Matrix442 | `Logs/Revival/FinalReview/native-final3-20261108-r04/pass-02/20261108-120833-513/` |
| r04 3 | 12:48:03 | Audit15·Smoke8·Matrix442·clean Build·Player | `Logs/Revival/FinalReview/native-final3-20261108-r04/pass-03/20261108-122516-306/` |

아래 개별 실행과 중간 실패 기록은 원인과 수정 근거를 보존한 이력이다. r04 뒤 바뀐 소스/실행 파일의 판정은 이 문서 맨 위의 추가 검수 범위를 기준으로 하며, 이전 단계의 대기·실패·완료를 현재 상태와 혼용하지 않는다.

## 현재 구현과 범위

- supplied Core DLL은 변경하지 않았고, 실제 제공 패키지 해시를 Audit에서 검사한다.
- Everyday 패키지·12개 관리자·범용 MonoBehaviour 수명 중계는 제거했다. 각 액터의 `EntityLogic`과 typed `EntityData`, 액터 소유 native FSM, Procedure 소유 일반 C# 판 객체가 게임을 실행한다.
- UI는 native `UIFormLogic` 수명을 따른다. 소환 골드·마나·쿨다운·생성 요청과 사망 후 재생성은 화면이 아닌 판/플레이어가 소유한다. 로비 액터도 로비 판이 소유한다.
- 유닛·스킬/장비·프리팹 매개변수·스테이지/웨이브/보상 수치는 native CSV/DataTable에서 읽는다. 아트 참조, 저장 진행도, 생성 요청 데이터는 별개다.
- 새 `link.xml`은 실제 문자열 선택 타입·Core 모듈·반사 접근 필드만 보존한다. 타입/필드 및 설정 범위의 Editor 검사는 Audit07에서 통과했다. 이것이 IL2CPP 실행 검증을 의미하지는 않는다.

구조 상세는 [Framework-Migration.md](Framework-Migration.md), 인수 조건은 [Toybox-Authoring-Acceptance.md](Toybox-Authoring-Acceptance.md), 전투 규칙은 [Gameplay-Open-Issues.md](Gameplay-Open-Issues.md), 스킬 성장의 확정 규칙은 [Player-Skill-Level-Rules.md](Player-Skill-Level-Rules.md)에 기록한다. UI 이벤트·물리/렌더·Spine·데이터 바인딩에 필요한 Unity 컴포넌트는 유지한다. 미사용 채팅 백엔드와 PVP/계정 상점의 별도 온라인 기능을 완성했다고 주장하지 않는다.

## 확보한 실행 증거 — 개별 안정화 이력

보고서의 검사는 부트 사례를 포함한다. 서로 다른 실행의 수를 합쳐 한 번의 전체 통과 수로 표시하지 않는다.

| 실제 보고서 (`Logs/NativeRewrite/`) | 결과 | 확인한 범위와 한계 |
| --- | --- | --- |
| `audit-08.json` | **15개, 오류·경고0** | 운영4씬과 설정의 기본 템플릿1개를 별도 검사. 프리팹316·데이터757, 기존 native/CSV/linker 검사와 SampleScene의 hierarchy/YAML 참조 확인. |
| `audit-07.json` | 14개, 오류·경고 0 | 운영 4개 씬, 316개 프리팹, 757개 데이터; 205개 Entity 매핑, 26개 UI 주소; 유닛/스킬/스테이지 CSV 및 linker의 57개 카탈로그 타입·5개 Procedure·19개 helper·18개 Core 모듈·2개 generic 구현·52개 필드 보존 범위. |
| `smoke-runtime04.json` | 7개 통과 | 부트, 실제 typed 요청/취소/재사용, native 콜백 실패 반환, 저장 복구, 전투/입력/사망/재소환/같은 씬 재진입/퇴장. 의도한 저장 경고 2개·실패 진단 3개를 별도로 기록. |
| `ownership-04-runtime.json` | 5개 통과; 예상 NotExist 경고1개·실패 진단4개 | 실제 native 실패/재시도·한 번 반환·겹친 요청. 이동1.8/2.1, 오라4/7과 부호 있는 스탯 재계산이 요청 값을 유지했고, 판 종료 뒤 concrete 데이터 사용 수가 살아 있는 native 소유 수와 일치. |
| `all-runtime03.json` | **전체 실패**; 311개 사례 성공 후 중단 | 영웅 237개(154 기본+66 재사용+17 장시간), 7개 스킨, 소유권·회전·버프·14개 기본 스킬의 개별 성공 근거. SkeletonMagicianLeader 사례에서 중단했으며 이 실행의 뒤 사례는 통과하지 않음. |
| `all-04-runtime.json` | **전체442개 통과**, 오류0·예상NotExist경고1·실패진단4 | 검사기 완료 프레임 수정 후 동일 소스 전후 지문928069a2…/22,367개. 66개 등급 재사용 관찰 중 실제 전환44개 모두 같은 물리 인스턴스. 최종3회 전 안정화 실행. |
| `actors-runtime01.json` | 59개, 오류·경고 0 | 수정 후 58개 일반 액터/배경 사례. |
| `projectiles-runtime01.json` | 17개, 오류·경고 0 | 16개 실제 투사체 명중·중복 피해·오래된 대상 ID 거절 사례. |
| `rewards-runtime02.json` | 5개, 오류·경고 0 | 실제 드롭 정체성, 승리 직후 이탈 시 정산, 적 사망, 무한 모드 지급. |
| `skills-04-runtime.json` | 16개, 오류·경고 0 | 새 레벨 규칙 복구 후에도14개 기본 스킬 결과와 native SkillBalance32행/576필드, 문화권 파싱, 장비 요청 스냅샷/레벨 저장/원본 보존이 통과. |
| `skill-levels-02-runtime.json` | 23개, 오류·경고 0 | 부트+독립 경계 스냅샷8개+실제LV20 효과14개. 24/25상한과 typed 요청 불변성 포함. |
| `buffs-04-runtime.json` | 8개, 오류·경고 0 | 기존 지속/중첩/판 격리 및 Skull/Slime 장비↔버프 적용 순서4개, Dragon 펫 장비 계산. |
| `motion-04-runtime.json` / `skins-04-runtime.json` | 각각2개/8개, 오류·경고 0 | 새 소스의 Raccon 회전/경계/사망/재사용 및7개 플레이어 스킨의 실제 공격/사망/재사용·Dragon 펫. |
| `lifecycle-runtime03.json` | 4개, 오류·경고 0 | HUD 재개방 중 판 경제/쿨다운/전리품/대기 소환 유지, 실제 사망 중 SKILL 재개방과 새 플레이어·변경 장착 정보 연결, Thunder 취소 시 시간·마나 복원. |
| `combos-runtime03.json` | 4개, 오류·경고 0 | 명시된 3종 혼합 전투. 모든 조합의 균형을 뜻하지 않음. |
| `dungeons-runtime03.json` | 7개, 오류·경고 0 | 6개 작성된 던전 모드의 실제 진입/종결/재시도/퇴장. |
| `stages-runtime03.json` | 31개, 오류·경고 0 | 30개 일반 스테이지의 실제 구성/진입/종결. 모든 판을 자연 전투로 끝까지 진행했다는 뜻은 아님. |
| `ui-runtime03.json` | 29개, 오류·경고 0 | 26개 주소의 실제 반복 열기/닫기, pending 중복 취소, 로비 액터 유지·선택/성장 갱신, 중첩 팝업 입력 차단. |
| `build-headless-A.json` / `.log` | clean BuildReport 성공, 오류·경고 0; **종료 native 경고 잔존** | `-nographics`에서도 타깃 셰이더 처리와 빌드는 수행됐다. 종료 후 JobTemp 5개/1,366바이트는 해결되지 않았다. |
| `build-graphics-B.json` / `.log` | clean BuildReport 성공, 오류·경고 0; **동일 native 경고 잔존** | 실제 D3D11 초기화. raw 전후 지문92beee…/22,369개가 정확히 같았고 추가 프리팹 변경은 없었다. |

새 Windows A 실행 근거는 `Logs/Revival/PlayerValidation/native-headless-A-20261108/player-seed.json`(2개)와 `player-verify.json`(8개)다. 별도 프로세스 저장 복원, 실제 DSP RMS on/off/on, 일반 입력 처리 경로, 1280×720·1920×1080·1024×768 오프스크린 캡처/HUD 경계, 301.9344초·11회 전투/로비 순환이 통과했고 오류·경고는 0이다. 매 로비 귀환에서 concrete EntityData 사용 수와 실제 native 소유 수가 일치했다. 할당량은 시작246,216,366 / 최대247,647,890 / 종료222,844,104바이트, 마지막 native Entity는17개였다. 두 프로세스의 완전 종료 로그에도 JobTemp/ComputeBuffer/Exception/경고/오류가 관측되지 않았다. 루트 검수자가 세 PNG에서 액터·배경·HUD를 확인했으며 분홍 재질·빈 화면·주요 컨트롤 잘림은 관측하지 않았다. 이 유한 실행은 무제한 세션의 누수 부재를 증명하지 않는다.

`skill-levels-01-runtime.json`은 총18개 성공 후 DemiGod에서 기대481.5 대비781.5 피해로 실패한 기록으로 보존한다. 그 검사에서는 타워의 공격/FSM과 잔여 투사체가 격리되지 않아 외부 피해 개입 가능성이 있었다. 기존 로그에 공격자 추적이 없으므로 추가300의 원인을 확정하지 않는다. 타워 FSM/잔여탄을 격리한02는 동일한 정확한 피해 기대값으로 전체23개를 통과했다. 새 관찰은 `Arrow=100` 타워 설정, clone의 정확한 실제 공격, 두 clone의 자연 종료25.19637게임초(기대25초)를 기록했다. 검사 격리 수정으로 기대 피해를 낮추거나 clone 수치를 조정하지 않았다.

이 결과는14개 스킬의 모든 레벨을 실제 전투로 반복한 것이 아니다. 값/정책은4/5/9/10/14/15/19/20과24/25상한에서 검사했고, 실제 전투 결과·자연 만료는LV20의14개 사례로 검증했다.

`native-final3-20261108-r01`은1회차 Audit/Smoke 성공 뒤 ContentMatrix의33개 성공 후 Goldy/STAR1 퇴장 E10 검사에서 실패했다. 실제 오류는 `EntityData balance sampled during pending native loads`이며 이 회차를 최종 통과 횟수에 포함하지 않는다. `Guard`/`Independent` 검사기에서 중첩 IEnumerator 완료 뒤 추가 프레임을 보내 pending0 전제와 즉시 소유량 검사의 시점이 갈리는 원인을 확인해 중첩 완료/Dispose 뒤 불필요한 프레임 진행만 제거했다. 운영 게임 코드는 바꾸지 않았고 검사기 중첩 완료/Dispose 뒤 불필요한 프레임 진행을 제거했다. 수정 뒤 All04 442개를 통과했고 새 RunId r02로 세 회차를 다시 시작했다. All04는 최종 회차 수에 포함하지 않으며 당시 최종 통과 회차는0회였다. 실패 원문은 `Logs/Revival/FinalReview/native-final3-20261108-r01/pass-01/20261108-104016-261/`에 보존한다.

전체 matrix의 사례 수와 최종 회차 수는 인수 조건의 모든 이상적인 경합·확률·조합을 자동으로 증명하지 않는다. [인수 문서의 유한 실행 범위](Toybox-Authoring-Acceptance.md#전체-사례-수만으로-충족되지-않는-조항)는 강제 완료 순서 역전, 동일 구체 FSM3개 독립 상태 대조, 모든 확률의 성공/실패 분기, 전체 제외 대상 행렬 등 아직 입증하지 않은 조항을 별도로 표시한다. 동일 물리 인스턴스 재사용도 각 실행의 관찰값을 따로 확인한다.

r02의1회차는 기본 씬 템플릿에 남아 있던 이전 Game 프리팹 참조를 정리하기 위해 담당자가 ContentMatrix 프로세스를 의도적으로 중단했다. 이는 실행 중 testcase 실패가 증명된 결과가 아니며 통과로도 세지 않는다. 원문은 `Logs/Revival/FinalReview/native-final3-20261108-r02/pass-01/20261108-110340-952/`에 보존한다. `SampleScene`의 고아 프리팹 인스턴스/stripped Canvas만 제거했고, 설정의 `templateDefaultScene`을 운영4씬과 별도로 감사하는 코드와 Runtime/Editor 소스 컴파일은 완료했다. 새 Unity Audit08은15개·오류/경고0으로 통과했다. r03 1회차 wrapper는 통과했지만 실제 저장 경고 때문에 최종 검수로 불인정됐으며2회차를 중단했다.

r03는 최종 검수로 **불인정**한다.1회차 wrapper/런타임은 Audit15+Smoke7+Matrix442를 PASS로 기록했지만 matrix 경고2개 중 하나가 의도한 NotExist였고 다른 하나는 `Could not save Luna progress:`로 시작하는 예기치 않은 Windows `File.Replace` 저장 실패였다. 당시 경고 게이트가 이를 통과시킨 결함도 확인했다. 담당자가2회차 matrix를 중단했고3회 연속 완료로 세지 않는다. 원문은 `Logs/Revival/FinalReview/native-final3-20261108-r03/pass-01/20261108-112222-832/`와 `pass-02/20261108-113855-540/`에 보존한다. 실제 Unity `save-smoke-02`는8개 검사·오류0으로 통과했고, 손상 저장 복구2건과 의도적 영구 파일 잠금2건의 경고4개가 정확한 메시지/횟수로 대조됐다. 최종 검수 `native-final3-20261108-r04`는3/3회 완료했다. 각 회차의 Audit15+Smoke8+Matrix442가 통과했고,3회차의 clean Windows 빌드·새 Player seed2/verify8·302.3273초11회 순환과 전후/회차간 소스 지문 일치까지 확인했다. Editor native 메모리 경고와 Player 종료 native 집계는 아래의 별도 미해결 항목이다.

## 안정화 단계에서 확보한 판정

- [x] 새 linker 타입/필드/설정 범위 구조 감사 — Audit07 통과, IL2CPP 실행은 아님.
- [x] 지정 스킬 성장 회귀 —02에서8개 경계 스냅샷·14개 실제LV20 효과 통과. 기본14스킬·버프/장비 순서·회전·스킨 재검사도 각각 통과.
- [x] 수정한 검사기로 동일 소스의 전체 native matrix 연속 실행 — All04 442개 통과; 최종3회의 성공으로 세지 않음.
- [x] native 재작성의 첫 clean Windows A 빌드와 완전 종료 로그 검토 — BuildReport는 성공했으나 native JobTemp 경고는 남아 있다.
- [x] Editor 그래픽 사용/`-nographics` 빌드 A/B 및 A의 실제 graphical Player 출력·종료 비교 — Editor 경고는 두 빌드에서 그대로 재현, B 실행 파일의 별도 Player 검사는 이 항목에 포함하지 않음.
- [x] A 실행 파일의 별도 seed/verify 프로세스 저장, DSP RMS, 정상 이동 처리, 3개 해상도 오프스크린 캡처/HUD 경계, 최소 300초 전투·로비 반복 — 2/8개 검사 통과, 최종3회차의 별도 재실행을 대체하지 않음.
- [x] 같은 RunId·소스 지문으로 최종 추가 검수1/2/3회 — r04 세 회차 모두 완료. r01 실패·r02 의도적 중단·r03 예상 밖 저장 경고의 불인정 기록은 보존하며 통과 횟수에 포함하지 않는다.

새 Soak는 판 퇴장 후 로비 표시 완료·전환 종료·pending 0을 기다린 뒤 concrete EntityData 타입별 `ReferencePool.UsingReferenceCount`와 살아 있는 native 소유자의 수가 정확히 같은지도 검사한다. 미사용 풀 캐시 증가는 누수로 오인하지 않는다. A의11회 귀환은 이 검사를 통과했다.

## 네이티브 메모리 경고

현재 native 재작성의 headless A와 graphics B 모두 `ShaderCompilation::CompileVariantJob` 종료 경고가 재현됐다. 같은 주소의5개 할당(504·582·152·64·64바이트), 총1,366바이트가 age3/4에 두 번 출력됐다. 16회 Editor 유휴 대기와 `-nographics`로 해결되지 않았다. 현재 A의 실제 graphical Player 두 프로세스에서는 이 경고나 ComputeBuffer 경고가 관측되지 않았지만 Editor 빌드 문제는 별개로 남는다.

| 빌드 | Editor 그래픽 | 셰이더 처리 제목 | 완전한 pass 결과(dx11/dx12) | 로컬 캐시 적중(dx11/dx12) | 신규 컴파일 |
| --- | --- | --- | --- | --- | --- |
| A | `-nographics` | 99 | 393/393 | 849/901 | 0 |
| B | D3D11 초기화 | 93 | 375/375 | 823/849 | 0 |

A의 추가6개 제목은 Spine/SkeletonGraphic 계열4개, Spine/Skeleton, URP/Spine/Skeleton Lit이다. 두 빌드는 모두 warm cache를 사용했다. 이 표는 타깃 셰이더 처리 근거이며 새로 컴파일한 변형의 검증이나 A/B 성능 개선 근거가 아니다.

완전 종료 로그의 별도 `##utp MemoryLeaks` 진단도 보존한다: A36,657,690바이트, B36,175,263바이트이며 AssetDatabase 등 넓은 엔진/Editor 메모리 항목이 포함된다. 이 집계는 정확한 JobTemp1,366바이트와 같은 값이 아니고 프로젝트 액터에 귀속할 근거도 없다. 이 기록을 제외하고 전체 종료가 무경고였다고 표현하지 않는다. 초기 licensing access-token 실패 메시지 및 종료 Curl42도 로그에 남지만 빌드 실패로 이어지지는 않았다.

새 빌드는 그래픽 사용 여부별 완전 종료 로그를 보존한다. `-nographics`에서 조용해졌다는 이유만으로 문제 해결을 선언하지 않으며, 타깃 셰이더 작업과 새 Player의 그래픽/오디오·정상 종료를 확인한다. JSON 작성 뒤 프로세스 종료에서 나온 경고도 원본 로그에서 확인한다. 강제 GC, 경고 필터, 비공개 엔진 정리 호출은 해결책으로 사용하지 않는다.

## 숨김 프로세스로 재실행

프로젝트의 Unity 에디터를 닫고 루트 PowerShell에서 실행한다. 실제 사용자 저장은 사용하지 않는다. 새 절대 경로·격리 표식·보고서 시각·프로세스 종료 코드를 검사하며 로그/JSON/PNG를 보존한다.

```powershell
& .\Tools\validation\Validate-Luna.ps1 -Mode Audit -NoGraphics -CheckNativeMemory
& .\Tools\validation\Validate-Luna.ps1 -Mode ContentMatrix -MatrixSection all -NoGraphics -CheckNativeMemory
& .\Tools\validation\Validate-Luna.ps1 -Mode BuildWindows -NoGraphics -CleanBuildCache -JobLeakDiagnostics -RebuildShaderCache -CheckNativeMemory
& .\Tools\validation\Validate-Player.ps1 -Mode All -Section all -RenderMode offscreen -SoakSeconds 300 -RunId 'native-pc-review' -CheckNativeMemory
& .\Tools\validation\Final-Review.ps1 -RunId 'native-final-review' -NoGraphics -CleanBuildCache -JobLeakDiagnostics -RebuildShaderCache -CheckNativeMemory
```

`Validate-Luna -Mode All`은 Audit+Smoke+ContentMatrix+BuildWindows를 실행한다. `Final-Review`는 Audit+Smoke+전체 ContentMatrix를 매 회차 새 Editor 프로세스로 실행하고, 3회차에는 빌드한 실행 파일의 실제 seed/verify·전체 진단·300초 Soak를 추가한다. `-NoGraphics`는 Editor에만 전달되고 Player의 그래픽/오디오는 켜진다.

최종 1회차 지문을 잡기 전에 Addressables 설정/생성물 경로를 안정화한다. 새 content state는 `Builds/Revival/AddressablesState/<BuildTarget>`에 쓰고 기존 추적 파일을 덮어쓰지 않는다. A 전후의10개 프리팹·설정 변경은 원래 SHA-256으로 정확히 복원해 직렬화/경로 변경임을 확인했다(`build-headless-A-source-audit.json/.diff`). 8개 프리팹은 같은 한국어 설명의 YAML 표기만 바뀌었고, 배경2개는 줄바꿈·블록 위치·빈 필드 직렬화였다. Addressables가 생성한 별도 `Assets/AddressableAssetsData/link.xml`과 meta는 패키지가 Editor 시작 때 삭제하고 빌드 때 다시 생성하므로 운영 소스와 생성물을 구별해야 한다. 직접 작성한 `Assets/link.xml`의 보존 규칙은 계속 소스 검증 대상이다. B의 raw 전후 지문은92beee9410f92eab76a73b1de10f959ba5365a03447cbb84a76b7465744207ba/22,369개로 같았다. 게시된 `unity-authoring-sha256-v2`는 정확한 생성물2개만 해시에서 제외하고 존재 여부·크기·SHA-256·이유를 따로 기록한다. 작성 소스 기준선은b2a3a48a82c2c186f04d1abc182a9ed3850e347360d3d3beb372b154d59a92ae/22,367개다. FinalReview는 알고리즘 버전과 해시를 함께 비교하며 이 기준선으로 시작한 `native-final3-20261108-r01`은 실패했으며 최종 통과에 포함하지 않는다.

`Final-Review`3회 시리즈 도중 소스를 바꾸면 새 RunId로 그 시리즈를 다시 시작해야 한다. 이번 메모리 후속 작업의 별도1회 전체 회귀를 새3회 시리즈로 표시하지 않는다. 오프스크린 PNG는 실제 OS 창 레이아웃 전체를 증명하지 않으며, DSP RMS는 사람의 청감, 프로그램 이동 검사는 물리 키보드 입력을 증명하지 않는다. Android/IL2CPP·모바일 기기는 아직 검증하지 않았다.

의도적으로 중단한 r02의 기준선은 검사기의 두 coroutine 진행 수정 후 `928069a2002d7e4e8ed5d388a8db142851f6f8272f961bfbfcbfddeb73677dee`/22,367개다. 위 b2a3a48…는 실패한 r01과 A/B 당시 기록이다. 템플릿 정리/Audit 추가 후 r03 기준선은 `d62dce4bfb39d2b6c3e0708fd1187b11aca5a31e12f005b8a3034e460205b30f`/22,366개다. 앞선 지문을 현재 소스로 혼용하지 않는다.

r03의 각 회차 필수 실행은 Audit15 + Smoke7 + named ContentMatrix442다. 3회차에는 clean Windows 빌드와 그 실행 파일의 실제 Player All/300초가 추가된다. r03 1회차는 matrix의 예상 밖 저장 경고로 불인정했다. 저장 수정/경고 게이트의 실제 Unity Smoke02 통과 후 새 r04를 시작했다. 증거 위치: `Logs/Revival/FinalReview/native-final3-20261108-r03/pass-01/20261108-112222-832/`.

Editor 경고 게이트 수정은 원문 로그의 모든 경고 횟수를 보존하고 native observed/expected 목록과 정확한 메시지·횟수로 대조한다. 플레이 종료 후 다시 검사해 늦게 나온 동일 경고도 실패시킨다. Editor 소스 컴파일과 추출한 실제 대조 메서드의6개 독립 사례(정상 주장/예상 밖 저장/중복/관측되지 않은 주장/늦은 중복/대소문자 차이)는 통과했다. 이어서 실제 Unity `save-smoke-02`도8개 검사·의도된 경고4개로 통과했으며, 새 최종 Windows 실행은 r04의3회차에서 별도로 수행한다. 저장 실패 종류 전체를 허용하는 규칙은 없다.


## 저장 실패 수정 이후의 새 검수 기준

`save-smoke-01`은 잠금 해제 검사 스레드가 이전 저장의 재시도 값을 읽어 너무 일찍 잠금을 해제한 fixture 결함으로 실패한 기록을 보존한다. 저장 시도 세대와 실제 두 번째 시도를 확인하도록 검사만 바로잡은 `save-smoke-02`는2026-11-08 11:52:37 UTC에8개 검사·오류0으로 완료했다. 일시 잠금은 실제2회 시도 뒤 새 세대704002를 복원했고, 백업/주 파일의 영구 잠금은 각각6회 시도 뒤 실패를 보고하면서 정상 세대 보존 및 잠금 해제 후 저장/복원을 확인했다. 의도된 손상 복구2건과 영구 잠금2건의 경고4개만 observed/expected 목록에 동일하게 존재한다. native 콜백의 의도된 실패3건은 소유권 반환 검증과 별도로 대조했다. 근거는 `Logs/NativeRewrite/save-smoke-01.json`, `save-smoke-02.json`, `save-smoke-02-runtime.json`, `save-smoke-02.log`다.

r04의 v2 소스 기준선은 `e2002579392869ec4de604d96cd3193b756025093171c2aae6dfe11f670364dd`/22,368개다. 실제 `source-before.json`과 `summary.json`은 `Logs/Revival/FinalReview/native-final3-20261108-r04/pass-01/20261108-115304-945/`에 있다. 각 회차의 요구 실행은 Audit15 + Smoke8 + named ContentMatrix442이며,3회차에는 clean Windows 빌드와 새 실행 파일의 Player All/300초 검사를 추가한다. 회차 실행 중에는 통과로 선계산하지 않았고, 최종 summary와 지문까지 완료한 r04만3/3회로 인정했다. 이전 r03 및 A/B 지문·Player 결과를 이 기준선의 최종 결과로 혼용하지 않는다.


r04 1회차 `summary.json`은2026-11-08 12:08:33 UTC에 성공으로 완료됐다. Audit15/Smoke8/Matrix442가 모두 통과했고, 경고는 각각0/4/1개로 모두 검증한 expected 목록과 정확히 일치했다. 예상 밖 경고와 오류는0이며, 전후 v2 지문 `e2002579392869ec4de604d96cd3193b756025093171c2aae6dfe11f670364dd`/22,368개도 동일했다. 이 문장은 당시1회차 완료 시점의 근거다. 이후2·3회차도 통과했으며 현재 판정은 맨 위 표를 따른다.


r04 2회차는2026-11-08 12:25:16 UTC에 성공으로 완료됐다. `pass-02/20261108-120833-513/`의 Audit15/Smoke8/Matrix442 모두 통과했고 경고0/4/1개는 정확히 주장한 fixture 경고뿐이었다. 독립 대조에서1·2회차 각각442개의 예상 이름/성공 결과/검사 수가 일치하고 누락·중복은 없었다. 각 회차의22명 영웅66회 생성에서 같은 물리 인스턴스 재사용44건도 각각 확인했다. 전후·회차간 v2 소스 지문은 e200257…/22,368개로 같다. 이 문장은 당시2회차 완료 시점의 기록이다. 이후3회차의 새 clean Windows 빌드와 Player All/300초·최종 지문까지 끝나3/3회로 확정했다.


r04 3회차는 Audit15/Smoke8/Matrix442 단계까지 성공했고 경고0/4/1개도 각각 정확한 expected 목록과 일치했다. 당시에는 clean Windows 빌드가 진행 중이었으므로 완료를2/3회로 유지했다. 이후 빌드·Player300초·최종 지문까지 확인한 현재 결과는 맨 위 r04 완료 표를 따른다.


## 최종 r04의 Windows 빌드 C — 종료 로그 판정

r04 3회차의 새 clean headless Windows 빌드 C는2026-11-08 12:42:18 UTC에 BuildReport 오류0·경고0으로 성공했다. 근거는 `Logs/Revival/FinalReview/native-final3-20261108-r04/pass-03/20261108-122516-306/build-windows.json/.log`이며, 닫힌 전체 로그의 주소별 집계는 `Logs/NativeRewrite/final-r04-build-audit.json`에 보존한다. 이 빌드는 앞선 A/B와 다른 현재 최종 소스의 실행이다.

그러나 C에서도 Editor 종료 시 `ShaderCompilation::CompileVariantJob`의 **서로 다른5개 할당·총1,366바이트**가 남았다. 주소/크기는 `703d7370`/504, `703d79f0`/582, `703d7d00`/152, `703d7e60`/64, `703d7f70`/64이며, 같은5개를 age3/4에서 각각 출력했다(lifetime15). 두 덤프를10개 또는2,732바이트로 중복 계산하지 않는다.4개 스택에는 shader cache의 subprogram/parameter 읽기가 포함된다.16회 정상 Editor 유휴 갱신(0.656초) 및 `-nographics` 후에도 재현됐으므로 native 경고를 해결했다고 판정할 수 없다. 마지막 UTP `MemoryLeaks`의36,158,078바이트는 여러 Editor/엔진 레이블의 별도 집계이며 이1,366바이트나 프로젝트 게임 객체의 누수량으로 해석하지 않는다.

C 전체 로그에서 C#/UAC·셰이더 경고, ComputeBuffer, 게임 코드의 NullReference/MissingReference/InvalidOperation/IO 예외는 관측되지 않았다. 다만 초기 라이선스 서명/토큰 진단 뒤 entitlement/갱신 성공, primary listening socket 실패 뒤 PlayerConnection endpoint/multicast 등록, Bee cache-client `move_path failed` 뒤 같은 Csc 노드 CacheWrite/빌드 성공, 종료 Curl42와 worker session 메시지는 원문에 남는다. 따라서 BuildReport0/0을 전체 프로세스의 모든 진단0으로 표현하지 않는다.

C 빌드 종료 당시에는 Player와 최종 지문이 대기였으므로 완료를2/3회로 유지했다. 이후 Player300초·완전 종료 로그·최종 지문을 확인해 r04는3/3회 완료했다. native 종료 집계는 완료된 기능 검수와 별도로 남는다.


## 최종 C Player와 완전 종료 로그

최종 실행 증거는 `Logs/Revival/PlayerValidation/native-final3-20261108-r04-pass03-20261108-122516-306/`의 `player-seed.json`(2개), `player-verify.json`(8개), 각 `.log`와 PNG3개다. 두 JSON은 오류·경고0이고 별도 프로세스가 nonce·재화·덱·스킬·스킨·진행·성장·인벤토리·음소거를 복원했다. 실제 게임 오디오 RMS0.0122213643,48000Hz PCM 혼합 on0.0151079549/off0/restored0.0182190575를 측정했다. 일반 입력 처리의1초 이동은2.400001월드 단위이며 물리 키 입력이나 사람이 스피커를 들었다는 주장은 아니다.

302.3273초11회 씬/전투 순환에서 시작/최대249,005,390바이트, 종료222,361,173바이트를 관측했고 마지막 native Entity는17개였다. 각 로비 귀환의 concrete EntityData ReferencePool 사용 수가 살아 있는 native 소유 수와 일치했다. 세 PNG는 담당자가 액터·배경·HUD를 확인했으며 분홍 재질·빈 화면·주요 컨트롤 잘림은 관측하지 않았다.4:3의 배치는 A와 같은 비율 차이를 보였으며, 오프스크린 캡처와 임시 camera-space UI를 실제 OS backbuffer/원래 overlay의 모든 배치 검증으로 확대하지 않는다.

**완전 종료 로그는 native 무할당 상태가 아니다.** JobTemp/ComputeBuffer/관리 예외는 관측되지 않았지만 `PlayerConnection::Cleanup##utp:`에 별도 `MemoryLeaks.allocatedMemory`가 seed41,463바이트, verify45,302바이트로 남았다. broad native 메모리 레이블이며 정확한 발생 원인·프로젝트 귀속은 입증하지 못했다. Editor C의 shader-cache1,366바이트와 더하거나 같은 누수로 단정하지 않는다. 이 집계까지 보존하므로 runtime JSON의 경고0을 전체 native 메모리 문제 해결로 해석하지 않는다.


A의 닫힌 Player 로그도 같은 UTP native 집계(seed41,463/verify45,302바이트)를 갖는다. 따라서 C에서 새로 증가했다고 주장하지 않으며, 같은 값이라는 이유로 무해하거나 해결됐다고 판정하지도 않는다. 주소/소유권 스택이 없어 정확한 귀속은 미확인이다. 비교 근거: `Logs/NativeRewrite/final-r04-player-exit-audit.json`.
