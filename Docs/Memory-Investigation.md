# 남은 native 메모리 진단 조사

2026-11-09 KST 기준, 본 프로젝트 Unity6000.6.0f1. 아래 실행 로그 시각과 RunId는2026-11-08 UTC다. r04 기능 검수3회와 이후 메모리 원인 조사를 구분한다. 이 문서는 경고를 필터링하거나 종료 집계를 숨겨 통과시키는 근거가 아니다. 비교 실행은 담당자가 숨김 CLI 프로세스로 수행하며, 다음 실험은 실제 보고서가 생긴 항목만 결과로 인정한다.

**최신 v2의 실제 재검수를 완료했다.** source `46a2a137e47d135cebe7515dbd5651bc077930f65e24b8e706c8cb61524c02fe`/22,372개에서 gated cold Build07·Early R04·Full Player302초·새 Matrix442가 통과했고 전후 지문도 정확히 같다. 게임 소유 객체/handle의 정상 종료를 수정했지만 엔진 warm shader-cache 경로의5개/1,366바이트와 귀속 미확인 UTP 집계를 영구 해결했다고 주장하지 않는다.6000.6.3f1/6000.6.8f1과 public unused asset/64프레임 비교에서도 재현돼 본 프로젝트6000.6.0f1을 유지하고, 명시적 cold-cache 빌드 우회책과 완전 종료 로그 게이트를 제공한다.

Audit15·Smoke8는 게임 종료 코드가 같은 직전 source7a837e…에서 실제 통과했다. 이후 Early R03 검사기가 파괴된 Procedure의 enabled를 읽은 실패를 보존하고, 검사 조건만 보정해 source46a2a1…로 바꿨다. 이 최종 소스의 offline 컴파일과 새 Build/Early/Player/Matrix를 별도로 통과한 것이며 Audit/Smoke를46a에서 다시 실행했다고 표시하지 않는다. 앞선 source1fd…의442개·R01/R02 실행과 r04의3회 검수도 각각의 이력으로 유지한다.

## 최종 v2에서 새로 확인한 빌드·초기 종료·장시간 실행

`memory-final-build-07`은6000.6.0f1에서2026-11-08 15:57:53→15:59:23 UTC에 clean/cold Windows 빌드와 완전 종료를 마쳤다. BuildReport 오류0/경고0, 명시적 native memory gate0, JobTemp0이다. 광범위한 Editor UTP 집계37,474,850바이트는 그대로 원문에 남기며 gate0을 모든 native 할당 해제로 해석하지 않는다.

`memory-final-early-20261109-r04`의 실제 Player는 Start 전 native FSM0·Procedure enabled·게임 데이터/세션 미초기화 상태에서 공개 Shutdown을 호출했다. Procedure 동기 disable, 종료 후3정상 프레임·반복 Shutdown, native/FSM/resource/pending/ReferencePool 사용량0, 기존 isolated primary/backup을 포함한 모든 저장 파일 byte-identical을 통과했다. 실제 프로세스 exit0·native gate0이며 UTP는40,123바이트였다. 앞선 R03 실패를 덮어쓰지 않는 새 실행이다.

`memory-final-full-20261109-r04`의 별도 seed3/verify9 검사도 오류·경고0과 native gate0으로 끝났다. verify는 실제302.38324초11회 전투/로비 순환이며 매 귀환의 EntityData 사용량과 live 소유량이 일치했다. Unity 총 할당 샘플은 initial/peak249,630,323→final222,722,001바이트, 마지막 로비 native Entity17개다. 종료 직전 SO116·Sprite4·Entity17·FSM8·loaded handle41·pending load/spawn 각각1개를 직접 캡처했고 모두0/Unity null이 됐다. 요청778의 typed Id0·성공/실패 콜백0·두 번째 종료 불변성도 통과했다. 최신 seed의 UTP는40,122바이트, verify는43,962바이트로 보존한다.

담당자가 이 실행의 새1280×720/1920×1080/1024×768 PNG3개에서 정상 배경·액터·HUD와 주요 잘림/분홍 재질/빈 화면 없음을 확인했다. PNG별 SHA와 실제 관찰은 `Logs/NativeRewrite/memory-player-r04-visual-review.json`에 있다. 이미지 검사의 offscreen/HUD 범위와 물리 입력·청감 미검증 한계는 이전과 같다. 현재 실행 파일·UnityPlayer·Assembly-CSharp·제공 Core DLL의 SHA는 `memory-final-artifacts-v2.json`, 독립 Player 원문 대조는 `memory-final-v2-player-independent.json`이다.

마지막 `memory-final-all-02`는2026-11-08 16:05:32→16:19:56 UTC에442개 고유 사례/검사 전부 성공, 오류0, 정확히 검증한 고의 NotExist 경고1회로 종료했다. 완전 종료 native gate도 명시적 오류0이었다. `memory-final-source-v2-before.json`과 `-after.json`의 v2 알고리즘·SHA46a2a1…·22,372개가 모두 같다. Addressables가 생성하는 정확한 `Assets/AddressableAssetsData/link.xml`과 meta는 실행 전 존재/후 부재를 excludedGeneratedOutputs에 이유·당시 SHA와 함께 기록했다. authored `Assets/link.xml`과 게임 소스는 계속 지문 대상이다. 이 추가 회귀는 이번 변경의1회 전체 Matrix와 실제 build/early/player 검증이며 이전 r04의3회를 새 변경의3회로 옮기지 않는다.

최종 독립 감사는 `Logs/NativeRewrite/memory-final-v2-independent-audit.json`에 있다.442개 예상 이름과 실제 목록이 정확히 같고 누락·추가·중복은0이었다.22명 영웅의 관찰66건과 같은 물리 인스턴스 재사용44건, 고의 진단4개와 정확한 경고1회, 프로세스33032의 exit0·native gate0을 대조했다. 지문은 합계뿐 아니라22,372개 경로·크기·SHA 항목 모두 동일했다. Matrix 종료 UTP34,274,687바이트도 삭제하지 않고 보존한다.

## 메모리 수정 후 실제로 획득한 결과

메모리 종료 수정 후의 본 프로젝트6000.6.0f1 재검수도 완료했다. `memory-audit-01`은15개 검사/경고0, `memory-all-01`은442개 고유 사례 전부 성공/오류0/검증된 NotExist 경고1회, 실제 Windows Player는 아래 seed3·verify9개 검사를 통과했다. Full Matrix 뒤 source 지문은 `1fd7d1d68ebfb4cd7dddd347f75724a999314a569a59534ba0a12d20fb6d766c`/22,370개로 비교 빌드 전후와 같다. 이는 수정 후 한 차례의 전체 회귀이며 이전 r04의3회 검수 기록과 혼합해 새3회 검수라고 표시하지 않는다.

`memory-fix-20261108-r01`의 첫 번째 Windows Player 프로세스는3개 검사를 통과했다. 새 종료 검사는 살아 있던 런타임 SO113개, 생성 Sprite4개, native Entity17개, FSM8개, 로드 handle19개, pending load3개, pending spawn1개를 직접 잡아 두었다. 정상 종료 뒤 캡처한 SO/Sprite는 실제 Unity null이 됐고 native 소유자·캐시·handle·대기 수는 모두0이었다. 대기 중이던 요청18의 typed data Id는0, 성공/실패 콜백은 각각0이며, 종료를 다시 호출하고 한 정상 프레임이 지난 후에도0을 유지했다. 이 검사는 종료 직전 단순히 사전을 비운 것만 확인하지 않는다. 증거: `Logs/Revival/PlayerValidation/memory-fix-20261108-r01/player-seed.json`.

같은 수정의 실제 Unity Smoke는8개 검사, 예상 저장 손상/파일 잠금 경고4회만으로 통과했다. clean Windows build도 BuildReport0오류/0경고였다. 그러나 닫힌 build 로그에는 ShaderCompilation JobTemp5개/1,366바이트가 그대로 남았다. 새 seed Player의 닫힌 로그 UTP는40,123바이트로 기존41,463과 달랐지만, 이 차이를 특정 수정이1,340바이트를 해결했다는 인과 증거로 해석하지 않는다. 현재 통과 근거는 직접 소유 객체의 실제 정리와 회귀 검사다. 증거: `Logs/NativeRewrite/memory-smoke-01{,-runtime}.json`, `memory-build-01.{json,log}`, 위 Player 디렉터리의 `player-seed.log`.

별도 verify 프로세스도9개 검사, 오류/경고0으로 완료했다. 실제302.208649초11회 씬/전투 순환 동안 매 로비 귀환에서 concrete EntityData 사용량과 live 소유량이 일치했다. Unity 총 할당 샘플은 initial/peak249,773,374→final223,074,666바이트, 마지막 로비의 native Entity는17개였다. 마지막 종료 검사는 runtime SO116개·Sprite4개·Entity17개·FSM8개·handle40개·pending load/spawn 각각1개에서 출발해 모두0으로 정리됐다. 요청782의 Data Id0·성공/실패 콜백0과 두 번째 종료 뒤 상태도 확인했다.

두 프로세스의 완전히 닫힌 로그는 JobTemp·ComputeBuffer·Exception·warning·error 검색 결과가 없지만 UTP는 seed40,123/verify43,962바이트로 남는다. 각각 예전41,463/45,302보다1,340 작다는 사실은 관측값이며 특정 allocation의 인과 증명은 아니다. 원본 참조를 캡처해 Unity null을 확인하고, 새 모듈을 만들지 않는 captured manager와 ReferencePool 사용량0을 검사하는 실제 테스트 소스도 독립 검토했다. 보고서·로그 SHA·종료 사례·UTP 원문은 `Logs/NativeRewrite/memory-player-r01-independent-audit.json`에 보존했다.

루나 스크립트 없는 URP 최소 프로젝트와 Spine/Sprite shader를 더한 최소 프로젝트의 cold/warm build는 각각 모두 JobTemp0이었다. 실제 Luna에서는 정확한 Library/ShaderCache를 백업한 뒤 cold02에서 JobTemp0이 됐지만, 곧바로 같은 새 캐시로 실행한 warm03에서5개/1,366바이트가 다시 발생했다. 따라서 이전 강제 업그레이드의 낡은 캐시만 제거하면 지속 해결된다는 가설은 채택할 수 없다. 현재 Luna의 새 cache를 읽는 빌드 경로에서도 재현되며, 이 단계의 Player 검수 완료가 남은 Editor 진단의 해결을 뜻하지 않는다.

## 실제 종료 검사 게이트와 최신 배포 후보 확인

`-CheckNativeMemory`는 프로세스가 종료된 뒤 전체 로그를 별도 보존하고 명시적 native 메모리 오류를 검사한다. 실제 warm04 빌드는 BuildReport 성공과 별개로 JobTemp5개/1,366바이트 때문에 **거부**됐다. 이 음성 검사는 `memory-gate-negative-result.json`의 `ExpectedWarmNativeRejectionObserved=true`와 `memory-gate-warm-04-memory.json`의 gate=false에 남아 있다.

`-RebuildShaderCache -CheckNativeMemory`를 함께 사용한 실제 cold05는 BuildReport0오류/0경고, 닫힌 로그의 명시적 native 메모리 오류0/JobTemp0으로 통과했다. 캐시는 매번 명시적 opt-in으로 정확한 폴더만 검증·백업하며, 원인을 영구 수정했다고 표시하지 않는 **빌드 우회책**이다. `-RebuildShaderCache`를 쓰지 않는 warm 재빌드에서 재발할 수 있고 게이트는 이를 거부한다. UTP 집계는 삭제·숨김 처리하지 않으며 별도 원문으로 보존한다. 증거는 `Logs/NativeRewrite/memory-gate-cold-05.{json,log}`, `memory-gate-cold-05-memory.json`이다.

그 새 실행 파일로 `memory-gated-final-20261108-r02` Player seed3/verify9를 다시 통과했고, 두 완전 종료 로그의 명시적 native gate도0이었다. 새 캡처3개는 담당자가1280×720/1920×1080/1024×768에서 정상 배경·액터·HUD, 분홍 재질/빈 화면/주요 컨트롤 잘림 없음을 확인했다. 일반 입력 처리와 DSP on/off/on 및 실제 종료 정리도 재검사했다. **이 R02의 soak 설정은10초, 실제12.5649986초1회 순환**이며 앞선 R01의302.208649초11회를 새 빌드의 장시간 검사로 잘못 옮기지 않는다. R02 종료 직전은 seed SO113/handle19, verify SO116/handle34와 각각 Sprite4/Entity17/FSM8/pending spawn1이었고 모두0으로 정리됐다. Id0·취소 콜백0·두 번째 종료 불변성도 통과했다. raw 로그의 JobTemp/ComputeBuffer/Exception/경고/오류는0이지만 UTP40,123/43,962바이트는 그대로 남는다. 독립 대조는 `Logs/NativeRewrite/memory-player-r02-independent-audit.json`에 있다.

## 현재 직접 확인한 서로 다른 진단

| 진단 | 실제 관측 | 해석 범위 |
| --- | --- | --- |
| Editor JobTemp | A/B/최종 C와 메모리 수정 후 build01은5개 고유 할당504+582+152+64+64=1,366바이트. cache를 비운 cold02는0, 바로 이어진 새 cache warm03은 다시5개/1,366바이트. 스택에 `ShaderCompilation::CompileVariantJob` 및 shader cache subprogram/parameter 읽기. | 프로젝트 managed 할당 스택이 아니다. 단순한 오래된 cache 잔존만으로 설명할 수 없으며 새 cache 재사용에서도 발생했다. 지속 해결로 판정하지 않는다. |
| Player UTP | 수정 전 A/C seed41,463바이트, verify45,302바이트로 각각 일치. 수정 후 seed40,123/verify43,962바이트. `PlayerConnection::Cleanup##utp:` 뒤 `MemoryLeaks.allocatedMemory`. | 종료 시 native 집계다. 호출 스택·개별 객체 귀속이 없으며 앞에 붙은 PlayerConnection 문구만으로 그 모듈의 누수라고 단정하지 않는다. |
| 게임 수명 회귀 | C의302.3273초11회 순환에서 매 로비 귀환 concrete EntityData 사용량=live native 소유량. Unity 총 할당 샘플249,005,390→222,361,173바이트. | 해당 유한 실행의 소유권/메모리 관측이다. 모든 엔진 native 할당이 해제됐다는 증명은 아니다. |

원문은 `Logs/NativeRewrite/final-r04-build-audit.json`, `final-r04-player-exit-audit.json`과 그 안의 완전 종료 로그 경로에 있다. 같은 주소의 중복 덤프, 서로 다른 프로세스의 집계, UTP label별 값과 전체 allocatedMemory를 임의로 합산하지 않는다.

## 공개 근거와 그 한계

Unity 직원 Tautvydas-Zilys는 이 UTP 형식을 프로세스 종료 시 해제되지 않은 메모리 양을 보여주는 출력이라고 설명했다. 이 설명은 형식의 의미를 뒷받침하며, 현재 프로젝트의 특정 객체나 모든 종료 집계가 무해하다는 원인 증명은 아니다. [Unity Discussions의 직원 답변13](https://discussions.unity.com/t/what-is-utp-type-memoryleak/742073/13)

`NativeLeakDetectionMode.EnabledWithStackTrace`는 native leak 추적에 전체 호출 스택을 더하는 공개 설정이다. 다만 추적 대상은 MallocTracked/FreeTracked 경로이며, 모든 엔진의 종료 UTP 레이블에 자동으로 객체별 스택을 제공한다는 약속은 없다. 현재 설치한 `UnityPlayer.dll` 문자열에도 정확한 실행 안내 `-native-leak-detection EnabledWithStackTrace`가 있다. [NativeLeakDetectionMode](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Unity.Collections.NativeLeakDetectionMode.html), [Unity leak detection 설명](https://docs.unity3d.com/2023.1/Documentation/Manual/Preferences.html)

`HideFlags.DontSave`는 DontUnloadUnusedAsset을 포함하고 명시적 소유자의 정리가 필요하다. 일반 `Object.Destroy`는 Update 뒤로 지연된다. 따라서 마지막 종료 프레임에 정리를 예약하는 코드와, 프레임이 계속 도는 동안 정상 소유자를 먼저 해제하는 코드는 비교할 가치가 있다. 이것만으로 현재 UTP 수치의 원인이라고 단정하지 않는다. [DontSave](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/hideflags/dontsave), [Object.Destroy](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/object/destroy)

## 소스 조사 결과 — 수정 전 경로

- `Validate-Player.ps1`은 batchmode·해상도·로그 옵션을 쓰며 현재 추가 native stack 옵션 없이 실행했다. `LunaNativeValidation.Guard`는 결과 기록 후 직접 `Application.Quit`한다. 빌드는 Development이고 AllowDebugging 옵션은 추가하지 않는다.
- `LunaSaveSettingsHelper`의 파일·MemoryStream은 using으로 닫힌다. 설정 문자열/사전만으로 UTP의 native Serialization 레이블에 귀속할 근거는 없다.
- `LunaUIViews`의 생성 Sprite는 DontSave 캐시에 들어가지만 기존 해제 호출은 다음 SubsystemRegistration뿐이었다. 실제 종료 소유 경로를 확인할 대상이다.
- `LunaProgressAutosave.OnDestroy`의 데이터 복제본 정리는 Application.isPlaying일 때 지연 Destroy를 쓴다. 종료 시 다음 Update가 보장되지 않는 경로와 정상 사전 정리를 구분해야 한다.
- supplied ResourceComponent가 `new ResourceManager()`로 만든 Addressables 어댑터는 Core의 GameFrameworkModule 레지스트리에 속하지 않는다. native 모듈 종료만으로 어댑터의 모든 pending/loaded handle 수명이 끝난다고 가정할 수 없다. 수정 여부와 callback 경합은 실제 구현 리뷰/회귀로 판정한다.

## Core 종료 순서에서 지켜야 할 계약

Toybox `External/GameFramework/GameFramework/Base/GameFrameworkEntry.cs`는 우선순위 내림차순 리스트를 역순으로 종료한다. Procedure(-2)가 먼저 판/FSM을 종료하고, 기본0의 UI/Entity/Sound는 생성 역순, Fsm(1), ObjectPool(6), Event(7) 순이다. 동일 우선순위의 세부 순서를 별도 보장으로 만들지 않는다. 마지막 ReferencePool.ClearAll 및 Marshal 캐시 해제도 원본 Core가 수행한다.

Entity/UI 종료는 Hide/Close 뒤 recycle queue를 비우며, ObjectPool 종료가 모든 instance의 helper.Release를 호출한다. 이때 기본 helper는 ResourceComponent.UnloadAsset과 지연 Destroy를 사용하므로 resource adapter와 helper GameObject가 먼저 죽으면 안 된다. Sound 종료는 Stop만 수행하고 asset 반환은 DefaultSoundAgentHelper의 다음 Update→Core SoundAgent.Reset에서 수행될 수 있다. 정상 사전 종료에서는 소리 Stop(0) 후 helper 갱신을 먼저 허용하고, resource adapter는 native 소비자/풀 정리가 끝난 뒤 마지막으로 종료해야 한다. 원본 Core DLL 수정으로 순서를 바꾸지 않는다.

구현 리뷰에서 추가로 두 가지 종료 순서를 확인했다. Base 업데이트를 멈추면 EventManager의 대기 이벤트도 멈추므로 LunaSpawn 이벤트를 기다려 drain해서는 안 된다. 새 종료 코드는 먼저 모든 요청을 취소하고 관찰자를 해제한 뒤 resource operation 자체의 완료를 기다린다. 또한 unclaimed EntityData는 Core의 마지막 ReferencePool.ClearAll **이전**에 반환해야 한다. 이후 반환하면 새 빈 pool의 UsingReferenceCount가 음수가 될 수 있어 순서를 바로잡았다.

## 공개한 수정과 검증 범위

Addressables 어댑터는 로드 요청마다 별도 소유 기록을 남긴다. 같은 주소를 두 번 로드하거나 서로 다른 주소가 같은 객체를 반환해도 획득 횟수만큼 한 번씩 해제한다. `BeginShutdown`은 새 요청을 거절하면서 기존 native 소비자가 완료/취소 처리를 하도록 허용한다. native 모듈과 풀이 끝난 뒤 `Shutdown`은 소비자 콜백을 막고 잔여 handle을 반환한다. 이미 참조를 반환한 pending asset도 내부 완료까지 PendingLoadCount에 남겨, 실제 작업이 끝나기 전에 drain이 끝났다고 판정하지 않는다. pending scene은 완료 후 즉시 Addressables unload로 넘기고 scene load handle은 SceneProvider가, unload operation handle은 어댑터가 각각 한 번 해제한다.

실제 수정 파일을 포함한 별도 .NET probe의8개 소유권 사례와 Unity가 만든 RSP를 사용한 Runtime assembly 컴파일은 통과했다. 사례는 동일 주소 중복, alias, BeginShutdown 경계, 늦은 성공/실패, 실패 해제, pending scene, 이미 진행 중인 scene unload, 소비자 unload callback 예외의 finally 해제다. `Logs/NativeRewrite/resource-shutdown-probe/results.log`에 결과가 있다. 이 probe는 Addressables를 통제한 이중체이므로 실제 Unity Player 종료와 UTP 감소를 대신 증명하지 않는다. 이후 실제 seed Player에서도 위에 기록한 생성 객체·native 소비자·pending 요청의 종료 회귀를 확인했다.

## 통제 비교 계획과 판정

| 단계 | 통제 조건 | 얻으려는 근거 | 상태 |
| --- | --- | --- | --- |
| B0/B1 빈 Built-in Player + 스택 옵션 | 별도 최소 프로젝트, 같은6000.6.0f1/Windows x64/Mono/Development, Luna·GF·Addressables·Spine·URP 없음. 실제3초 후 Application.Quit, graphics/audio 유지. native stack/temp 진단 옵션 사용. | 게임/패키지 없는 엔진 자체의 종료 UTP 기준선. | 실행 완료: UTP22,891바이트, JobTemp0. 이 실행만으로 스택 옵션 유무 차이를 비교한 것은 아님. |
| B2 렌더 분리 구상 | B0에 -nographics만 변경하는 추가 Player 대조 구상. | graphics/렌더 경로의 고정 종료 비용을 분리. | 이 독립 A/B 결과를 완료 근거로 채택하지 않음. 실제 후속 URP/Spine/캐시/엔진 비교 범위는 아래와 같음. |
| B3 최소 URP shader | Luna/GF/Spine 없는 URP 렌더 fixture, headless clean build와 같은 프로젝트 warm build,16회 idle. | URP 사용만으로 같은 Shader JobTemp가 생기는지 확인. | 두 build 모두 BuildReport0/0, JobTemp0. 별도 graphical Player UTP23,753바이트. |
| B4 최소 Spine/Sprite shader | 같은 URP fixture에 Luna의 Spine URP shader package를 복사하고 실제 material을 Universal Render Pipeline/Spine/Sprite로 지정. Luna 게임 코드·GF 없음. | 이 shader/package의 단순 사용으로 같은 JobTemp가 생기는지 확인. | cold/warm 두 build exit0, BuildReport0/0, JobTemp0. Luna 전체 variant 조합을 대표하지 않음. |
| L0 Luna 기존 종료 기준 | 기존 A/C의 완전 종료 로그를 비교 기준으로 보존. | 기존 수치의 반복 관측을 유지. | 기존 seed41,463/verify45,302 확인. 동일 구 binary에 스택 옵션만 더한 별도 재실행은 완료 근거에 포함하지 않음. |
| L1 Luna 소유자 사전 종료 | 게임 전투/데이터/렌더 조건 유지, 정상 public 종료 경로로 소비자→풀→resource→생성 asset 정리 후 frame settle. | 실제 고아 handle/생성 Sprite/SO 감소 및 저장/late callback 회귀. UTP 감소만을 정답으로 삼지 않음. | Smoke8/예상 경고4, clean build0/0, 실제 seed3/verify9 검사 통과.302.208649초11순환과 두 프로세스의 실제 자원 종료 통과. |
| L2 Luna 생성 shader cache 분리 | 전체 Library를 지우지 않고 정확한 Library/ShaderCache만 workspace 내부 ignored 고유 백업으로 이동, 원본 복구 가능하게 보존. 같은 Luna clean build의 cold/warm을 비교. | 이전 강제 업그레이드의 생성 cache 잔존과 현재 cache 경로 문제를 구분할 근거. | cold02 JobTemp0, 새 cache warm03에서5개/1,366바이트 재발. 두 build0오류/0경고, authored source와 shader 작업 수 동일. 단순 cache 재생성은 지속 해결 아님. |
| E1 별도6000.6.3f1 후보 | 검증한 Unity 배포 payload로 ignored 복제 프로젝트만 비교. 본 프로젝트6000.6.0f1 유지. | 현재 엔진 patch와 실제 post-build 프레임 진행의 차이를 확인. | 첫 실제 build는0오류/1경고, 외부 경고 검수 실패. 이어64실제 프레임을 진행한 build는0/0이지만 raw JobTemp는 둘 다5개/1,366바이트. 채택하지 않음. |
| E2 별도6000.6.8f1 후보 | 공식 배포본 검증 뒤 짧은 별도 사용자 경로로 설치/격리 비교. | 더 새 patch의 실제 상태를 비교. | cold/warm 모두 Player build 생성 성공, warm BuildReport0/0. 둘 다 raw JobTemp5개/1,366바이트 재발. 채택하지 않음. |
| E3 public unused asset 정리 |603 복제본에서 public Resources.UnloadUnusedAssets 완료 및64실제 프레임 뒤 같은 닫힌 로그 검사. GC 강제 호출·로그 필터·private cleanup 없음. | 공개 정리 API가 같은 할당을 해소하는지 확인. | 실제 완료 뒤에도5개/1,366바이트. 본 프로젝트에 채택하지 않고 엔진 비교 종료. |

최초 빈 기준선의 닫힌 로그는 `Logs/MemoryIsolation/empty-ng-01/attempts/20261108-134212-295-player`에 있다. PID88040, exit0, graphical Player이며 실행 인자에 `-nographics`가 없다. 이 Player를 만든 별도 Editor build는 `20261108-134111-980-build`에서 `-nographics`, clean build,16회 idle 후 BuildReport0오류/0경고, JobTemp0이었다. 전체 UTP 값과 label은 `closed-log-evidence.json`에 별도 보존했다. 빈 프로젝트에서도 집계가 발생한다는 사실만 증명하며 Luna와의 차액을 게임 누수 또는 무해한 엔진 비용으로 바로 분류하지 않는다.

URP 기준선은 `Logs/MemoryIsolation/urp-ng-01/attempts/20261108-134952-753-build`(cold), `20261108-135256-984-build`(warm), `20261108-135235-523-player`에 있다. 실제 pipeline 필드는 UniversalRenderPipelineAsset이며 두 build의 graphicsDevice는 Null이다. cold는16회 idle/6.757초, warm은16회/0.323초 후 종료했다. 둘 다 닫힌 로그의 고유 JobTemp 할당0이며, 이 작은 fixture가 Luna의 모든 shader variant를 대표하지는 않는다.

Spine 비교는 같은 attempts 아래 `20261108-140256-922-build`(cold)와 `20261108-140511-565-build`(warm)에 있다. 실제 materialShader 필드는 `Universal Render Pipeline/Spine/Sprite`이며16회 idle 뒤 닫힌 두 로그에서 JobTemp 할당·shader/C#/UAC 경고가 모두0이었다. 원래 package154개 파일의 SHA manifest를 보존했다. import 후 독립 비교에서는153개가 그대로 같고, 예제 PNG1개의 `.meta`만 Unity의 TextureImporter10→13 및 sprite slice 생성으로 바뀌었다. shader/include 소스는 모두 원본과 같으며,154개 파일 전체가 import 뒤에도 byte-identical하다고 표현하지 않는다.

ShaderCache 비교는 경로를 workspace 내부로 확인하고, 덮어쓰기 없는 고유 백업을 남기는 가역 작업으로 제한했다. 실제 백업은 `Logs/MemoryIsolation/luna-shader-cache-old-01/ShaderCache-before`,2,783개 파일·7,118,972바이트다. 이동 전후 manifest의 tree SHA256은 모두 `79e71954a6a8c6ce65061183f10bb56b8fcfde3fb82ee0e4578642584a7529f0`이고 파일별 기록도 동일하다. 실행 전·cold 뒤·warm 뒤 authored source 지문은 모두 `1fd7d1d68ebfb4cd7dddd347f75724a999314a569a59534ba0a12d20fb6d766c`/22,370개다.

실제 Luna 비교 결과는 다음과 같다. 어느 단계에서도 shader/variant를 제거하는 설정 변경을 하지 않았다.

| Luna build | shader 이름 수 | 완료 pass 수(dx11/dx12) | local cache hits | 실제 새 compile 수 | remote/skipped | 고유 JobTemp |
| --- | ---: | --- | ---: | ---: | --- | --- |
| 기존 cache, memory-build-01 | 93 | 750(375/375) | 1,672 | 0 | 0/0 | 5개/1,366바이트 |
| cache 재생성, cold02 | 93 | 750(375/375) | 225 | 1,447 | 0/0 | 0 |
| 같은 새 cache, warm03 | 93 | 750(375/375) | 1,672 | 0 | 0/0 | 5개/1,366바이트 |

단순 합계뿐 아니라 shader 이름, shader/pass/stage/API별 처리량 multiset, scriptable stripping 뒤846개 기록(합계1,674)의 multiset이 세 로그에서 모두 동일했다. pass 완료 요약의 처리 합계1,672와 stripping 합계1,674는 서로 다른 로그 지표로 그대로 보존한다. 이 비교는 로그에 실제 나타난 작업의 동등성을 증명하며 모든 가능한 variant나 시각 결과 전체의 동등성 증명으로 확장하지 않는다. 원문과 상세 비교는 `Logs/NativeRewrite/memory-build-{cold-02,warm-03}.{json,log}`, `memory-shader-cold-warm-independent.json`에 있다.

## 별도 엔진 patch 비교의 출처와 현재 상태

후보의 정확한 버전은 **6000.6.3f1 (45d8eee7de74)**이다. Unity 공식 배포 URL의 Windows x64 installer3,859,981,488바이트를 받았고, 공식 release metadata의 MD5(`vAdoyM3M8+xaHlnIlExGnQ==`)와 로컬 MD5 `bc0768c8cdccf3ec5a1e59c8944c469d`가 일치한다. Authenticode 서명도 Unity Technologies SF/Valid다. 출처·바이너리 SHA256은 `Logs/NativeRewrite/unity-6000.6.3f1-release.json`, `-installer-verified.json`에 보존했다. [Unity 공식 installer](https://download.unity3d.com/download_unity/45d8eee7de74/Windows64EditorInstaller/UnitySetup64-6000.6.3f1.exe)

일반 사용자 권한, `UseShellExecute=false`, elevation/shell fallback 없이 시작한 installer PID95352는 payload 약8.43GB를 추출한 뒤 멈춘 상태여서 해당 확인된 프로세스만 중단했다. 설치 종료 코드는-1이며 **설치 완료로 인정하지 않는다**. `-user-install-launch.json`은 실행 기록이지 성공 기록이 아니다. 별도로 추출된 Unity.exe·Unity.dll·Windows Mono UnityPlayer.dll은 각각6000.6.3f1 및 Valid 서명을 확인했고, 필수 dependency 검사에서 누락0을 확인한 범위로만 비교 실행에 사용했다. 세 바이너리의 버전·서명·SHA는 `-payload-binaries.json`에 있다.

후보의 첫 격리 build는 `MaterialReferenceChanger`를 찾지 못하는 CS0246으로 실패했다. 추출된 payload에서 `.cs` 본문은 존재하지만 그 옆 `.meta`가 빠진4개 파일을 확인했다. 코드 경로는256/259자, meta 경로는261/264자였고,4개 C# 본문 SHA256은 정상 설치된6000.6.0f1의 해당 vendor 코드와 각각 동일했다. 원래부터 `.meta`가 없는 `Source~`/`Documentation~` 파일은 복원 대상으로 삼지 않았다.

본문이 동일한4개 파일에 한해 정상 설치본의 원래 metadata를 .NET long-path 경로로 후보 user payload와 **격리 복제본** PackageCache에 복원했다. 대상은 Core의 `ReadonlyMaterialConverter.MaterialReferenceBuilder/Changer.cs.meta`와 URP의 대응 `ReadonlyMaterialConverterTests.MaterialReferenceBuilder/Changer.cs.meta`다. vendor C# 본문 변경은 없고 본 프로젝트 Assets/Packages/ProjectSettings도 바꾸지 않았다. 파일별 C#/meta SHA, 원본·목적 경로는 `unity-6000.6.3f1-missing-cs-metadata.json` 및 `unity-6000.6.3f1-identical-code-meta-repair.json`에 있다. 이 제한된 payload 보완은 중단된 installer를 정상 설치 성공으로 바꾸어 기록하는 근거가 아니다.

후보 실행 대상은 `Logs/MemoryIsolation/engine-patch-603/Project`의 격리 복제본이다. metadata 복원 뒤 실제 build 자체는 Succeeded/오류0/경고1로 끝났으나 외부 검수는 예기치 않은 경고2회 때문에 실패했다. 두 경고는 압축된 `LogoAtlas/BG.png`의 SpriteAtlas 입력과 `PerformanceTestRunInfo.json` 재작성이다. 더불어 완전 종료 로그에도 동일 ShaderCompileVariantJob5개/1,366바이트가 남았다. `patch603-attempt02-memory.json`과 복제본 `Logs/Revival/build-windows.{json,log}`에 보존하며 이 후보를 해결책으로 채택하지 않았다.

추가603 비교에서는 public `EditorApplication.QueuePlayerLoopUpdate`를64번 요청하고 실제 `Time.frameCount`와 `Time.renderedFrameCount`가0→64로 진행하는 것을 확인했다. Editor update65회/idle65회,1.291초 뒤 build는0오류/0경고였지만 닫힌 로그에는 같은5개/1,366바이트가 남았다. 따라서 이 실행에서는 단순 Editor idle 대신 실제 player-loop 프레임을 진행하면 사라진다는 설명도 맞지 않는다. 이는 모든 가능한 대기 시간의 무용성을 증명하는 것이 아니라 실행한64프레임 조건의 결과다. 증거는 복제본 `Logs/Revival/patch603-playerloop64.{json,log,memory.json}`이다. 본 프로젝트 코드는 변경하지 않았다.

6000.6.8f1은 공식 installer를 검증한 뒤 짧은 사용자 설치 경로 `C:/Users/1/UE668`에 **정상 설치 exit0**으로 완료됐다(2026-11-08 15:15:22 UTC). Unity.exe·Unity.dll·Windows Mono UnityPlayer.dll3개는 모두6000.6.8f1(237b6593792b)이며 Authenticode Valid다. installer MD5는 `ca0c8cedd665ed04d33c216858080e6f`, 파일 크기3,854,705,888바이트이며 원문은 `unity-6000.6.8f1-{installer-verified,user-install-result,payload-binaries}.json`에 있다. 설치가 남긴 MissingDependencies 안내1,160바이트는 remote Profiler의 방화벽 설정만 다루고 VC 런타임 누락은 없었다. 방화벽 규칙을 바꾸지 않았다.

608의 새 복제본 cold/warm도 완료했다. 두 Player build는 모두 생성됐고 BuildReport는 각각0오류/0경고였다. cold 외부 검수는 압축된 SpriteAtlas 입력 BG.png 경고1회 때문에 실패했고, warm 외부 검수는0/0으로 통과했다. 하지만 두 완전 종료 로그 모두 age3/4에서 같은5개/1,366바이트를 출력해 native gate는 실패했다. 원문은 `Logs/MemoryIsolation/engine-patch-608/Project/Logs/Revival/patch608-{cold,warm}.{json,log,memory.json}`이다. 따라서 엔진 교체로 해결되는 이점이 입증되지 않아 채택하지 않았으며 본 프로젝트는 `6000.6.0f1 (f7f8ed4d1e24)`을 유지한다.

마지막603 복제본 비교는 public `Resources.UnloadUnusedAssets`가0.743초/1회 update 후 실제 완료됐고(frame0→1), 이어 QueuePlayerLoopUpdate64회 요청·Editor update65회로 frame1→66까지 진행했으나 동일5개/1,366바이트가 남았다. 증거는 복제본 `Logs/Revival/patch603-unload64.{json,log,memory.json}`이다. 이 결과까지 확인한 뒤 엔진 후보/공개 정리 대안의 비교를 종료했다. 엔진 코드·vendor C#을 임의로 바꾸거나 GC/로그 필터로 출력만 숨기지 않았으며, main은6000.6.0f1과 명시적 cold-cache 빌드 우회책·native 종료 게이트를 유지한다. warm 재발의 엔진 내부 결함을 영구 수정했다고 주장하지 않는다.

## 초기 종료 경로의 추가 보강과 실패 이력

부트 완료 전에는 Procedure FSM이 아직 초기화되지 않았을 수 있으므로 종료 코드가 활성 판/Procedure 접근을 가정하지 않도록 보강했다. 또한 중첩 shutdown coroutine의 예외가 OS 종료 요청을 영구 거부하지 않도록 최대20초의 종료 시도, 실패 로그·비정상 종료 코드, 원래 비정상 종료 코드 보존을 추가했다. 실패를 정상적인 정리 완료로 위장하지 않고 살아 있을 수 있는 소비자의 데이터를 무작정 지우지 않는다.

실제 `QuitRoutine` 본문을 그대로 추출한 .NET probe12개 제어 흐름 사례는 통과했다. 정상/Async 완료, MoveNext·Current·Dispose 실패, 원래 실패 보존, 무한 대기/Async stall의 deadline, terminal 상태 미도달, 종료 코드 보존을 검사한다. 근거는 `Logs/NativeRewrite/quit-routine-probe/results.json`, 추출 메서드 SHA256 `f712a17405e96daea49d826bacaf5a4f48e53f8e5df2c4a74c07ecb5009fcee0`이다. 이 통제 probe는 Unity 스케줄링·실제 프로세스 종료·native 자원 반환·Editor 분기를 대신 증명하지 않는다. 이후 최종 v2 source46a2a1…의 실제 Early R04/Full Player·전체 Matrix442·최종 전후 지문 대조까지 위와 같이 별도로 통과했다.

첫 실제 Early R03 실패 원문은 `Logs/Revival/PlayerValidation/memory-final-early-20261109-r03/player-early.json`에 보존한다. sceneLoaded/Awake 뒤 Start 전 native FSM0·Procedure enabled·게임 데이터/세션 없음에서 시작해, 공개 Shutdown 첫 yield까지 예외 없이 도달하고 Procedure가 동기적으로 disabled된 것은 관찰했다. 이후 종료를 기다린 검사 코드가 파괴된 Unity 객체의 enabled를 읽어 NullReferenceException으로 실패했다. 해당 조건을 `procedure == null || !procedure.enabled`로 수정했으며, 뒤의 반복 종료/저장 무변경 검사는 이 실패 실행에서 미획득이다. 이는 게임 종료 코드 변경이 아니라 검사기의 Unity 객체 파괴 상태 처리 보정이고, R03 전체 PASS로 바꾸지 않는다.

빈 Player에서도 집계가 남으면 그 관측값과 조건을 기록한다. Luna와 다르면 package/게임 수명 차이를 좁힌다. 같은 값이어도 전체 엔진 문제가 해결됐다고 하지 않는다. 기능·저장/비동기 소유권 회귀가 통과하고 직접 소유 자원의 반환이 증명된 수정만 채택한다. GC 강제 호출, private engine cleanup, 진단 무효화, 로그 필터를 원인 수정으로 사용하지 않는다.
