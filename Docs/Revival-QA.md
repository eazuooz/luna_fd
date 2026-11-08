# 루나 디펜스 복구 최종 검수

이 문서는 실제 실행 결과가 있는 항목만 통과로 표시한다. Unity 버전은 `6000.6.0f1`이며, Windows Development 빌드와 PC 자동 검사가 이번 검수 범위다. 사용자의 요청 이후 데스크톱 화면·마우스·키보드를 직접 조작하지 않고 숨김 배치 프로세스로 실행했다. 모바일 기기 검수는 요청 범위에서 제외했다.

## 최종 검수 목록

### 프레임워크·데이터·임포트

- [x] 제공된 GameFramework Core·Unity 패키지를 임포트하고 실제 Entity/UI/Procedure/Setting/DataTable 수명 주기로 연결한다.
- [x] 기존 씬·프리팹의 스크립트 GUID와 직렬화 연결을 유지한다. 게임 Entity 모듈은 프레임워크 콜백으로 실행하고 Unity Update와 중복 실행하지 않는다.
- [x] 운영 씬 4개, 프리팹 317개, 데이터 에셋 784개, 일반 스테이지 30개, UI 주소 26개의 누락 스크립트·참조·중복 GUID를 검사한다. 서드파티 예제 씬은 운영 검사에서 제외한다.
- [x] ObjectData 76행의 수치 32개를 CSV와 실제 GameFramework DataTable로 읽는다. SO에는 그림·프리팹·설명 참조를 유지하고 런타임 복제 데이터에 성장·저장 값을 적용한다.
- [x] CSV 고유 ID/GUID, 행 누락·중복, 따옴표·쉼표와 문화권 독립 숫자 파싱을 검사한다.
- [x] Spine Sprite Slice 명령의 기존 GUID/local fileID/pivot/border 보존, 회전 slice, 멱등성, 일반 texture 전환을 실제 임시 에셋으로 검사한다.
- [x] 남은 빌드 경고 52개 및 전체 재컴파일에서 확인한 추가 경고를 수정한다. 캐시를 비운 Windows 빌드에서 C#/셰이더 오류 0·경고 0을 확인한다.

씬 흐름에는 GameFramework Procedure/Fsm, 게임 객체에는 Entity 수명·풀링, 화면에는 UI Form, 수치에는 DataTable, 저장에는 Setting을 사용한다. 기존 전투 AI/상태 규칙과 Unity 입력·표시 어댑터는 유지했다. 남은 MonoBehaviour 자체를 결함으로 보지 않으며, UI 자식·입력 이벤트·OS 저장 훅·관리자 어댑터·선택적 레거시 채팅은 해당 역할로 남아 있다. 일부 효과의 Instantiate 경로도 초기화 시 native Entity로 편입된다. 모든 API·오디오·네트워크를 일괄 교체했다는 의미는 아니다.

### 기능·수명·저장

- [x] Loading → Lobby → 일반 전투/보스/무한 던전 → Lobby 흐름을 검사한다.
- [x] 캐릭터·인벤토리·상점·업그레이드 화면과 영웅/병사/전체 탭 전환 후 미리보기 Entity가 유지된다.
- [x] 덱·스킬·스킨·재화·성장·등급·아이템·스테이지·소리 설정을 저장하고 복원한다. 별도 Windows 프로세스의 종료·재실행으로도 확인한다.
- [x] 격리 저장 폴더에서 손상 JSON·주 파일 부재·백업 복구·원본 보존을 검사한다. 실제 사용자 저장 파일은 사용하지 않는다.
- [x] 업그레이드 7종의 최대 레벨과 잘못된 비용 입력에서 재화가 차감되지 않는다.
- [x] 스테이지 선택창을 열기 전에도 다음 스테이지를 선택하고, 표시 이름과 실제 생성된 스테이지 객체가 일치한다.
- [x] 실제 타워 사망 → 패배 → 재시도, 승리 → 코인 5개 수집 → 다음 스테이지 흐름과 진행도 저장을 검사한다.
- [x] 플레이어 실제 사망·풀 재사용·부활 후 활성 객체 수, 타워 참조, 체력·스케일·유한 수치를 확인한다.
- [x] 스킨 7종의 생존/부활과 Dragon의 고유 펫·씬 재진입을 검사한다.
- [x] 일반 스테이지 30개를 실제 생성하고 기본 전투 구성을 검사한다. 모든 스테이지의 처음부터 끝까지 자연 승리는 별도 범위다.
- [x] 유닛 프리팹 66종의 등급 포함 88개 초기화·숨김·재사용 사례를 완료한다. 발동 중 효과가 관찰되지 않은 패시브/등급 사례는 해당 효과의 피해량 증명으로 처리하지 않는다.
- [x] 발사체 16종의 실제 타깃 실행·재사용, 플레이어 스킬 14종의 실제 상태 변화를 검사한다. Seren의 중첩 파티클 생존·자연 종료와 Thunder의 단일 타깃 제출/중복 거절을 포함한다.
- [x] 등급 회귀(Lene/Rose/Ria/Lena/Scarlet/Joe 등), 보스·혼합 드롭 보상, 대표 혼합 전투 2개, 데미지 텍스트 Entity 수명을 검사한다.
- [ ] 모든 파티·스킬·적 조합의 효과와 게임 밸런스를 검증한다. 유한한 대표 사례 검사는 전체 조합 검증이 아니다.

### Windows 실행·화면을 띄우지 않는 검사

- [x] 숨김 Development 플레이어에서 Loading/Lobby/전투 진입, Addressables와 BGM 리소스 로드를 확인한다.
- [x] 실제 입력 코드가 호출하는 이동 처리 함수의 좌/우 유지, 동시 입력, 해제, 플레이어 부재 및 실제 이동을 검사한다. OS 키 입력 장치 경로 전체를 자동 검증한 것은 아니다.
- [x] 실제 BGM 재생 소스, DSP 진행과 오디오 샘플 RMS를 측정한다. 음소거 시 0, 해제 시 양수로 복원된다.
- [x] 1280×720, 1920×1080, 1024×768 오프스크린 렌더 PNG를 생성·검토한다. 4:3에서 잘리던 설정/음악 버튼과 진행 표시를 수정하고 주요 HUD 사각형의 화면 범위를 확인한다.
- [x] Windows에서 약 5분 동안 전투/메뉴 전환을 11회 수행하고 런타임 예외·풀 소유권·메모리 추이를 확인한다. 수 시간 장시간 부하 검사의 대체는 아니다.
- [x] 수정 후 여러 새 Windows 프로세스 종료 로그에서 기존 ComputeBuffer 해제 경고가 재발하지 않는다.
- [ ] 실제 스피커 청감, 물리 키보드·마우스·터치의 조작감, 전체 카메라 동작을 검사한다.
- [ ] 실제 창/backbuffer의 최종 화면을 확인한다. 배치 backbuffer는 빈 이미지였으며 오프스크린 경로로 명시적으로 전환했다. 실제 디스플레이·플랫폼 렌더 동등성을 주장하지 않는다.
- [ ] Android/iOS 빌드·터치·기기 성능을 검사한다. 현재 PC에는 Android 모듈/SDK/연결 기기가 없고 사용자는 PC 자동 검사까지만 요청했다.

## 경고와 발견한 결함

상세 원인과 변경은 [Warning-Review.md](Warning-Review.md)에 기록한다. 기존 증분 빌드의 52개는 게임 C# 32개와 셰이더 20개였고, 전체 빌드에서는 캐시로 가려졌던 프레임워크/Spine C# 25개도 있었다. Editor 전체 재컴파일의 추가 경고도 별도로 수정했다. 경고 필터로 숨기지 않았다. 빌드 집계 0개와 별개로 **Editor 프로세스 종료 시 네이티브 셰이더 작업의 JobTempAlloc 진단은 남아 있다**. drained09에서는 고유 할당 5개 합계 1,366바이트였고 두 번 반복 보고됐다. Windows 플레이어 종료에서는 재현되지 않았다. 16회 유휴 업데이트를 기다린 build-drain16-12에서도 동일한 결과였다. 이 경고는 미해결로 남긴다.

자동 검사로 추가 확인한 실제 결함에는 선택창을 열기 전 스테이지 선택 실패, 0 Stamina 유닛의 NaN 마나, 재사용 시 Dragon 펫/데미지 텍스트 수명, Seren의 중첩 파티클 조기 종료, MartinKing 충전/Spine 콜백 재사용, Thunder 타깃 선택 종료, 4:3 HUD 잘림이 포함된다. 각 수정 후 관련 검사를 다시 실행했다. 전체 연속 검사에서는 조의 상위 등급 플래그가 재사용 직후 한 프레임 살아남는 추가 결함도 재현했다. STAR1의 공격 계수가 1.3→1.0으로 달랐으며, 모든 영웅 모듈은 등급 적용이 끝난 뒤에만 native Entity 업데이트를 받도록 수정했다. 해당 회귀 검사는 상위 등급으로 풀을 예열한 후 하위 등급 재사용을 엄격하게 비교한다. `matrix-joe-prefixed-11.json`에서 1.3/1.0 차이를 재현했고, 수정 후 `matrix-joe-fixed-12.json`의 STAR1/ULT2 두 사례가 통과했다. 테스트 자체의 잘못된 전제(움직인 펫 위치 비교, Lobby 오디오 측정, 한 프레임 충전 대기, 보스 테스트 중 플레이어 사망 간섭)도 분리하여 수정하고 이전 실패 보고서를 보존했다.

구현 누락이 확인되었거나 명세가 충돌/누락된 영웅 효과·수치는 [Gameplay-Open-Issues.md](Gameplay-Open-Issues.md)에 남긴다. 예: Mei/Scarlet 피해 배율, Miu 저주 감소 단위, Ria/Matilda 소환 수명 누락, 공중/공격형 분류, 일부 궁극 설명. 이를 임의의 밸런스로 채우거나 테스트 통과로 구현 완료 처리하지 않는다.

## 실행 증거

보고서의 UTC 시각·success·개별 case를 기준으로 판단한다. 실패한 전체 보고서 안의 성공한 사례를 인용할 때는 보고서 자체가 실패했다는 사실도 함께 표시한다.

| 검사 | 증거 | 결과/범위 |
| --- | --- | --- |
| Sprite/운영 에셋 Audit | `Logs/WarningReview/audit-sprite-fixture-06.json` | 7개 검사, 오류 0·경고 0 |
| 최종 캐시 제거 Windows 빌드 | `Logs/Revival/FinalReview/warning-review-final3-20261108/pass-03/20261108-055321-021/build-windows.json` | 성공, 오류 0·빌드 경고 0; 전체 로그의 C#/셰이더 경고도 0. 별도 Editor 종료 JobTemp 진단 2줄은 남음 |
| 스테이지 30개 | `Logs/WarningReview/matrix-all-final-10.json` | 최신 스테이지 30개와 스킨 7개 성공; 이후 Joe 순서 문제로 전체 보고서는 실패, 해당 결함 수정 후 actors-final12에서 88개 전체 재통과 |
| 스킨/펫 | `Logs/WarningReview/matrix-skins-05.json`, `matrix-all-final-10.json` | 7개 통과, 최신 연속 검사에서도 재통과 |
| 스킬/발사체 | `Logs/WarningReview/matrix-skills-07.json`, `matrix-projectiles-06.json` | 14개/16개 통과 |
| 유닛·등급 | `Logs/WarningReview/matrix-actors-final-12.json` | Joe 수정 후 단일 연속 실행 88개/66프리팹 통과, 오류 0·경고 0; 이전 실패 보고서도 보존 |
| 등급/보상/혼합전투/텍스트 수명 | `Logs/WarningReview/matrix-ranks-final-12.json`, `matrix-rewards-02.json`, `matrix-combos-final-13.json`, `matrix-lifecycle-05.json` | 각각 1/1/2/1 사례 통과; 상세 observation으로 검증 범위 구분 |
| Windows 재시작/오디오/입력/렌더/반복 전투 | `Logs/Revival/PlayerValidation/warning-review-final-gated-20261108/` | 최종 런타임 seed 성공, verify 7개 통과, 오류 0·경고 0; 302.855초/11회; 메모리 266.08→241.18 MB(최대 267.06 MB) |

CSV 76행을 76개의 독립 전투 유닛 검증으로 해석하지 않는다. 일반 actor 사례는 61개의 서로 다른 CSV 행을 사용하며, 스킨 7종·타워 2종·DragonPet이 다른 10행을 사용한다. 나머지 5행은 root Status 프리팹과 연결되지 않는 데이터다.

MartinKing 검사는 충전 중 숨김/재사용까지이며 60초 후 폭발 종결은 포함하지 않는다. 발사체 사례 중 Arrow는 초기화/반환을 확인했으며 명중 피해를 증명하지 않는다. Goldy는 전역 골드 계수 변화까지 확인했으며 실제 시간당 수입 밸런스는 별도다. 혼합 전투는 이름이 기록된 두 조합의 8초 실행이다.

최신 Windows 검사는 05:47:19–05:52:48 UTC에 실행했다. 실제 BGM RMS 0.0119954, 테스트 신호와 혼합 출력은 켜짐 0.0216495 → 음소거 0 → 복원 0.0190711(48 kHz)이었다. 종료 로그에도 ComputeBuffer/JobTemp/런타임 예외가 없다.

최종 3회차가 실제로 생성한 실행 파일도 `Logs/Revival/PlayerValidation/warning-review-final3-exe-20261108/`에서 별도 seed/verify 프로세스로 재확인했다(05:59:46–06:00:26 UTC). 7개 사례가 오류 0·경고 0으로 통과했고 종료 로그도 깨끗했다. 이 마지막 실행의 반복 검사는 10초 설정/실제 약 12.36초 1회이며, 앞선 동일 런타임 소스의 302.855초·11회 결과와 구분한다. 세 해상도 자동 검사와 최종 4:3 PNG 재검토도 통과했다.

Windows PNG는 위 플레이어 폴더의 `render-offscreen-1280x720.png`, `render-offscreen-1920x1080.png`, `render-offscreen-1024x768.png`다. Root가 저장된 이미지를 직접 검토했으며 데스크톱 화면은 조작하지 않았다. 기존 실패/진단 실행과 부분 성공 기록은 그대로 보존한다.

## 수정 완료 후 추가 검수 3회

RunId `warning-review-final3-20261108`에서 최신 소스를 고정한 뒤 각각 별도 Unity 프로세스로 Audit와 Smoke를 수행하고 3회차에 캐시 제거 Windows 빌드를 수행한다. 3회 모두 완료·통과했다(05:47:51–05:58:22 UTC). 시작 지문은 `Logs/WarningReview/final-source-pre-final3.json`: 선택한 소스/설정/직렬화 파일·meta·검수 스크립트 15,536개, SHA-256 `cf8f9dcfe74418f158df5446e6c0cf43059f52fc38e45b5c43722511953c5a43`. 앞선 유닛 검사의 지문과 비교해 런타임/Editor 코드는 같으며 Addressables 빌드가 자동 생성한 `link.xml.meta` 한 파일만 추가됐다. 아트·음원 원본 payload 전체의 해시는 이 지문 범위에 포함하지 않는다.

| 회차 | Audit | Smoke | Windows 빌드 | 증거 |
| --- | --- | --- | --- | --- |
| 1 | 통과: 오류 0·경고 0 | 통과: 오류 0·의도한 경고 4 | 해당 없음 | `pass-01/20261108-054751-802` |
| 2 | 통과: 오류 0·경고 0 | 통과: 오류 0·의도한 경고 4 | 해당 없음 | `pass-02/20261108-055031-595` |
| 3 | 통과: 오류 0·경고 0 | 통과: 오류 0·의도한 경고 4 | 캐시 제거 빌드 통과: 오류 0·빌드 경고 0 | `pass-03/20261108-055321-021` |

종료 지문은 `Logs/WarningReview/final-source-after.json`: 15,536개, SHA-256 `97df625f5439a70d461e5ff07c306ff8858dabbf00806fe7171efd3a94355bdb`. 비교 결과 코드·프리팹·데이터·검수 스크립트의 변경은 없었다. 최종 빌드가 다시 생성한 `Assets/AddressableAssetsData/link.xml.meta` 하나만 달라져 전체 지문은 다르다. 최종 집계는 `Logs/WarningReview/final-summary.json`이다.

각 회차 Smoke는 28개 검사다. 경고 4개는 격리 저장 파일의 잘린 데이터, 백업 복구, 잘못된 설정 개수, 뒤에 붙은 잘못된 데이터를 의도적으로 시험한 결과이며 동일하게 재현됐다. 운영 플레이어 실행에서는 경고가 없었다.

이전 마이그레이션의 3회 검수는 `Logs/Revival/FinalReview/revival-final-20261108/`에 보존되어 있다. 당시 최종 빌드는 경고 52개였으며 현재 결과와 혼동하지 않는다. 당시 문서 원본은 `Logs/WarningReview/revival-qa-before-warning-review.md`다. 마이그레이션 전 사용자의 기존 변경 661개 경로는 `Logs/Revival/pre-migration-working-state.zip`, `pre-migration.patch`, `pre-migration-status.json`으로 보존했다. reset/stash/commit은 수행하지 않았다.

## 화면 없이 재실행

프로젝트의 Unity 에디터를 닫고 프로젝트 폴더에서 PowerShell로 실행한다. 각 도구는 숨김 프로세스를 시작하고 종료 코드 및 새 보고서를 확인한다. CPU/GPU는 사용하므로 다른 작업의 성능에는 영향을 줄 수 있다.

```powershell
# 에셋/참조 검사와 유한한 콘텐츠 전체 사례 (그래픽 장치 불필요)
& .\Tools\validation\Validate-Luna.ps1 -Mode Audit -NoGraphics
& .\Tools\validation\Validate-Luna.ps1 -Mode ContentMatrix -MatrixSection all -NoGraphics

# 그래픽 셰이더를 포함하는 Windows 빌드
& .\Tools\validation\Validate-Luna.ps1 -Mode BuildWindows -CleanBuildCache -JobLeakDiagnostics

# 재시작/오디오/입력/3개 해상도 오프스크린/5분 반복 검사
& .\Tools\validation\Validate-Player.ps1 -Mode All -RenderMode offscreen -RunId 'local-pc-review'

# 추가 검수 3회. 그래픽 빌드도 포함하므로 -NoGraphics를 붙이지 않는다.
& .\Tools\validation\Final-Review.ps1 -RunId 'local-final-review' -JobLeakDiagnostics
```

`Final-Review`는 Audit+Smoke를 3회 실행한다. 전체 ContentMatrix나 별도 Windows 프로세스 검사는 위 명령으로 별도 실행한다. `-MatrixSection`/`-MatrixFilter`로 관련 사례만 다시 검사할 수 있고 `-Pass 1`, `-Pass 2`, `-Pass 3`으로 특정 회차를 다시 실행할 수 있다. 각 시도는 시간별 폴더에 보존된다.
