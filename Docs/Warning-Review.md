# 빌드 경고 원인 검토

> **역사 기록 — Toybox native 재작성 이전 단계.** 아래의 경고 77/52개, 예전 `Status`·`UnitAI`·`LunaEntityBehaviour` 수정, 빌드/Windows 실행 및 “최종 3회”는 당시 구현의 근거다. 현재 코드에서 해당 중계 구조는 제거됐으며 이 기록을 새 재작성의 완료·무경고·메모리 문제 해결 증거로 사용하지 않는다. 현재 소스의 실제 결과와 남은 검수는 [Revival-QA.md](Revival-QA.md), 이전 QA 원문은 [Archive/Revival-QA-before-native-rewrite.md](Archive/Revival-QA-before-native-rewrite.md)에 있다. 새 A/B/C 비교에서도 Shader CompileVariantJob 종료 경고가 남았으며 현재 판정은 아래와 QA 문서에 기록한다.

r04 당시 native 재작성은 동일 소스3회 기능 검수와 Windows C/Player 검사를 완료했지만 A/B/C의 Editor shader-cache JobTemp5개/1,366바이트와 Player UTP seed41,463/verify45,302바이트가 남았다. 이후 실제 게임 소유 자원 종료 수정, source1fd의 전체442개 재검사·302초 Player 및 gated cold build를 별도로 수행했고 Player UTP는40,123/43,962로 관측됐다.6000.6.3f1/6000.6.8f1과 public 정리 비교에서도 warm JobTemp는 재발해 엔진을 유지하고 명시적 cache 백업·재생성 우회책/종료 로그 게이트를 제공한다. Early R03의 검사기 파괴 객체 접근 실패를 보정한 최종 v2 source46a2a1…는 Build07·실제 Early R04·Full Player302초·새 Matrix442와 전후 지문 일치를 모두 통과했다. Audit15·Smoke8는 검사기 한 조건만 보정하기 전 source7a에서 수행한 별도 근거다. 최신 cold build의 명시적 native gate는0이지만 warm 재발을 영구 해결한 것은 아니다. 게임 종료 코드는 해당 검사기 보정에서 바꾸지 않았고 R03을 통과로 세지 않는다. 이 문서의 역사적 수정이나 BuildReport0/0을 모든 native 메모리 문제 해결로 해석하지 않는다. 현재 소스별 실행 범위는 [Revival-QA.md](Revival-QA.md), 메모리 원문·후보 비교·남은 한계는 [Memory-Investigation.md](Memory-Investigation.md)에 보존한다.

2026-11-08. 최초 전체 빌드는 경고 77개(게임 및 프레임워크 C# 57개, Spine URP 셰이더 20개), 이후 증분 빌드는 경고 52개를 기록했다. 증분 빌드는 다시 컴파일하지 않은 어셈블리의 경고를 생략할 수 있으므로 두 수치를 서로 다른 문제 개수로 해석하지 않는다. 아래는 원인별 소스 수정 기록이며, 최종 통과 판정은 변경 후 Unity 빌드 보고서를 기준으로 한다.

## 게임 코드: 경고 32개

| 경고 | 원인과 조치 | 기존 동작·데이터 보존 |
| --- | --- | --- |
| CS0618 × 3 | `LunaStateMachine`의 단일 활성 플레이어 UI 조회를 `FindAnyObjectByType`으로 변경. `MartinKing`의 두 `Random.RandomRange`를 `Random.Range`로 변경. | 같은 UI 유형은 하나만 열리며 난수 범위와 호출 횟수는 유지한다. |
| CS0108 × 1 | `UnitAI_Player.ResetAnimation`은 `BaseAI.ResetAnimation`과 동일한 구현이라 중복 선언을 제거한다. | 호출자는 동일한 상속 메서드를 사용한다. |
| UAC1001 × 3 | `Status.coefficient`, `stat`, `unitState`에 `NonSerialized`를 명시한다. | 매 생성·재출현 시 코드로 초기화하는 전투 상태다. SO·CSV·저장 파일의 필드와 무관하며 새로 직렬화하지 않는다. |
| CS0414 × 8 | 보스/병사/루나 스킬의 `skillMatter`는 프리팹에 기록된 설명이다. 직렬화된 필드명·값을 유지하고 읽기 전용 `SkillDescription`으로 제공한다. | 기존 한국어 설명을 삭제하지 않으며 검사 도구에서도 읽을 수 있다. |
| CS0414 × 16 | 소비자가 없는 비직렬화 private 필드와 상수 대입을 정리하고, 원본 설명과 비교해 확정된 승급 조건 오류를 별도로 수정했다. 아래 목록을 참조한다. | 원본에 정의된 수치를 사용한다. 단위나 작동 방식이 없는 효과는 임의로 만들지 않는다. |
| CS0162 × 1 | `EndlessDungeon.DropItem`의 `if (true)`와 실행 불가능한 대체 보상을 제거한다. 동시에 프리팹 목록과 아이템 번호 목록의 길이가 달라지는 오류를 고쳐 `ItemData`를 기준으로 선택·생성한다. | 기존 첫 보스 보상, 일반 아이템 추첨 횟수·확률, 층수별 지급 횟수를 유지한다. `SetId`는 생성한 객체에만 적용하며 원본 프리팹을 바꾸지 않는다. |

### 사용되지 않던 private 상태

- `Idle.AutoAttackMoveDistance`, `AutoAttackMoveTrigger`: 생성자에서만 대입되던 이동 실험값. 현재 이동 입력/상태 전환과 연결되어 있지 않다.
- `Bellerin.skillControlCounter`, `MartinKing.skillControlCounter`: 0 대입 이후 읽지 않던 카운터. 실제 타이머와 스킬 코루틴은 유지한다.
- `Roen.coefSkill_1`, `triggerSkill_1`, `Scalet.coefSkill_3`: 계산식에 연결되지 않았던 값. Roen의 실제 전역 승급 효과와 Scalet의 사용 중인 공격 계수는 유지한다.
- `Tira.durationAlly`, `skillDamageAlly`, `skillDamageEnemy`, `coefSkill_1`: 실제 크기 변경 동작에서 읽지 않던 값. 기존 아군 성장 지속 시간 5초, 적 축소 시간 4초, 조건과 범위는 유지한다.
- `Lene.triggerSkill_1`: 사용되지 않던 플래그를 제거하고 실제 확률 계산을 원본 `Lene.asset.start5/ultra1` 설명에 연결했다. 본인 확률은 기본 40%, STAR5에서 +40, ULT1에서 +20으로 40/80/100%이며 아군 대상의 기본 40%는 유지한다. 기존에는 모든 등급·대상에 +40이 적용되어 있었다.
- `Ria.triggerSkill_3`, `Lena.triggerSkill_3`: 기존 플래그 대신 실제로 사용하는 `ultUnlocked`를 ULT2에서만 켠다. 원본 `ultra2` 설명에 해당하는 궁극 소환이 STAR5부터 열리던 조건을 수정했다. 기존 33초/12초 주기와 소환 프리팹은 유지한다.
- `Miu.triggerSkill_1`: STAR5에서 켜기만 하던 플래그. `Miu.asset.start5`의 “저주 10감소”는 단위·누적값·해제 API가 정의되지 않았다. 현재 `Status.Curse()`는 방어력과 이동 속도를 각각 0.8배로 변경하므로 임의의 10 단위 해제를 만들지 않는다.
- `Matilda.triggerSkill_2`: ULT1에서 켜기만 하던 플래그. `Matilda.asset.ultra1`은 미니/불 마틴 무적을 설명하지만 두 실제 프리팹(`Mni_Martin`, `MartinBall`)에는 Status와 Unit 태그가 없어서 모든 등급에서 전투 타깃이 아닌 공격 효과로 동작한다. 주석의 `GetComponent<Status>().Invincible(0)`을 복원하면 NullReference가 발생한다. 새 체력·피격 모델을 임의로 추가하지 않는다.

별도 확정 오류로 `Scalet`의 ULT2 80% 저주 조건 안에서 빠져 있던 기존 `Status.Curse()` 호출을 복원했다. 이는 `Scarlet.asset.ultra2` 설명과 기존 구현된 API를 연결한다. ULT1의 스킬 피해 감소는 현재 피해 계산에 대응하는 시스템이 없어 별도 미구현으로 남긴다. `UISkill` 장비 스킬 생성 루프도 슬롯 번호를 증가시켜 세 스킬이 모두 `Equip_0`에 덮어씌워지던 문제를 수정했다.

경고 제거가 모든 영웅의 미완성 승급 기획을 완성했다는 뜻은 아니다. 단위와 작동 방식이 불명확한 효과는 별도 기획 결정이 필요하다. SerializedField나 CSV에 있던 전투 수치와 에셋 참조는 이 정리로 삭제하지 않는다.

### 확인 상태

사용되지 않는 필드의 선언·대입·소비자와 해당 프리팹의 직렬화 값을 확인했다. 변경된 게임 파일의 `git diff --check`는 통과했다. `LunaDropRegression`은 실제 보스 단독/혼합 드롭·아이템 지급·원본 보존을 검사한다. `LunaHeroRankRegression`은 실제 등급 초기화, ULT2 소환 조건, Scarlet 스킬 코루틴의 저주 효과, 장비 슬롯의 개별 연결을 검사한다. 또한 Rose의 적군 공격력 감소/복원과 아군 계수 보존, Joe의 세 차례 Entity 표시/숨김 시 이벤트 구독 수를 검사한다. 두 도우미는 `UNITY_EDITOR || DEBUG` 조건에서만 컴파일되며 실행 시 Editor/Development Build인지 확인한다. 게임 코드 검수 담당은 Unity를 별도로 실행하지 않았으며, 전체 컴파일·스킬/보상 시나리오·Windows 빌드 결과는 실행 보고서에 기록한다.

## 프레임워크·Spine C#: 경고 25개

| 경고 | 원인과 조치 | 기존 동작·데이터 보존 |
| --- | --- | --- |
| CS0618 × 5 (2D 물리) | 두 `BoundingBoxFollower`의 `usedByComposite` API를 `compositeOperation`으로, `SkeletonUtility`와 두 래그돌 예제의 `isKinematic` API를 `bodyType`으로 변경한다. | 기존 bool 설정은 유지한다. Composite 사용 여부는 `Merge/None`, Kinematic 여부는 `Kinematic/Dynamic`으로 동일하게 매핑한다. |
| CS0618 × 5 (객체 조회) | `Game`과 `SoundComponent`는 정렬 인자 없는 `FindObjectsByType<T>()`, `Singleton`은 `FindAnyObjectByType<T>()`를 사용한다. | 전체 객체 열거·오디오 리스너 개수 확인과 단일 인스턴스 조회를 유지한다. Unity InstanceID 정렬에 의존하지 않는다. |
| UAC1001 × 11, UAC1009 × 1, UAC1015 × 2 | 런타임 Spine `AnimationState/Skeleton/Slot/Attachment`, 호환성 검사 중 계산한 jagged version 배열, 장착 Attachment 캐시, `UIReference.typeArray`의 CLR Type 배열, `DataManager.pickUpData`에 `NonSerialized`를 명시한다. | 해당 값은 코드로 생성하는 런타임 상태다. 원본 SkeletonData·프리팹·저장 설정을 새로 직렬화하거나 삭제하지 않는다. |
| UAC1001 × 1 | `LanguageDataTable.LanguageValue`에 `Serializable`을 추가한다. | 이 값은 에셋에 보존해야 하는 번역 문자열이다. 기존 `SerializeDictionary`의 키/값 저장 구조를 유지한다. |

Spine 전체 런타임을 다른 버전으로 교체하지 않았으며 원본 에셋 GUID와 직렬화된 설정을 유지한다. C# 소스의 diff 공백 검사는 통과했다. Unity가 재저장한 기존 이미지 meta/머티리얼의 YAML 공백은 이 소스 검사에 포함하지 않는다.

## Spine URP 셰이더: 경고 20개

원인은 Sprite ForwardLit 패스의 중복 `multi_compile_fog` 1건과 URP가 정의하는 두 내부 매크로를 독립 키워드로 컴파일했던 19건이다. `MAIN_LIGHT_CALCULATE_SHADOWS` 9건, `REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR` 10건은 서로 다른 변형에서 반복 보고됐다.

- Fog pragma는 한 줄을 유지해 LINEAR/EXP/EXP2 변형을 보존한다.
- 메인 광원 그림자 키워드는 현재 URP Lit 셰이더와 동일한 배타적 집합 `_`, `_MAIN_LIGHT_SHADOWS`, `_MAIN_LIGHT_SHADOWS_CASCADE`, `_MAIN_LIGHT_SHADOWS_SCREEN`으로 선언한다. 그림자 계산·정점 보간 매크로는 URP `Shadows.hlsl`이 해당 키워드와 그림자 수신 옵션에 따라 결정한다.
- 투명 Spine 블렌드 모드는 `_SURFACE_TYPE_TRANSPARENT`를 조명 헤더 전에 정의한다. 화면 공간 그림자가 켜져도 투명 오브젝트는 불투명 화면 깊이 대신 그림자 아틀라스를 사용한다. 불투명·알파 테스트 모드와 기존 블렌드/조명 설정은 유지한다.
- `InputData`는 0으로 초기화하고 그림자 변환에는 명시적으로 world position의 xyz를 넘긴다.

수정 위치는 프로젝트에 포함된 `Packages/com.esotericsoftware.spine.urp-shaders-4.0-2019.1`이다. `Library/PackageCache` 및 Unity URP 패키지 소스는 수정하지 않았다. 이후 실제 그래픽 빌드 clean06에서 셰이더 경고 0개를 확인했고 세 해상도 오프스크린 PNG를 검사했다. 실제 디스플레이/backbuffer 전체를 검증한 것은 아니다.

## Windows 종료 시 ComputeBuffer 경고

기존 `player-manual-01.log`는 종료 시 `GarbageCollector disposing of ComputeBuffer` 한 건을 기록하지만 할당 스택은 없다. 프로젝트 코드에는 ComputeBuffer 할당이 없으며, 설치된 URP의 종료 경로는 `ShaderData.Dispose()`, 렌더러 해제, `ConstantBuffer.ReleaseAll()`을 호출한다.

설치됐지만 사용하지 않는 `com.unity.2d.animation 16.0.0`에는 별도의 누락 경로가 있다. `Runtime/BatchedDeformation/GpuDeformationSystem.cs:24`의 `AfterSceneLoad` 콜백이 SpriteSkin 사용 여부와 무관하게 static fallback ComputeBuffer를 생성한다. 이 버퍼는 `GpuDeformationSystem.Cleanup()`에서만 해제되어, GPU 변형 시스템이 생성되지 않는 Spine 프로젝트에서는 해당 해제 경로가 실행되지 않는다. 이 코드는 관측된 경고의 유력한 원인이다. 원래 로그에는 할당 스택이 없어 해당 한 건의 할당자를 단정하지 않지만, 제거 후 아래의 새 Windows 프로세스 종료 로그에서는 경고가 재현되지 않았다.

`Logs/Revival/animation-package-usage.json`에 제거 전 검사를 기록했다. 2D Animation 패키지의 GUID 1,001개를 프로젝트 직렬화 파일 15,283개와 비교해 참조 0개였고, 프로젝트 C#/asmdef의 해당 API 참조도 없었다. 다른 설치 패키지 역시 2D Animation에 의존하지 않는다. 이에 `Packages/manifest.json`에서 사용하지 않는 **2D Animation만** 제거했다. authored PSD 28개가 있으므로 PSD Importer를 유지했으며, Sprite/SpriteShape/PSD Importer는 Animation에 의존하지 않는다. 패키지 잠금 파일 갱신은 Unity가 수행한다. 강제 GC, 경고 필터, 리플렉션으로 타 패키지 private 버퍼를 해제하는 우회는 추가하지 않았다.

수정 후 별도 Windows 프로세스 5회의 시작부터 정상 종료까지 확인했다. `warning-review-20261108`의 seed/verify, `warning-review-offscreen-20261108`의 diagnostics, `warning-review-fixed-20261108`의 seed/verify 로그에는 `ComputeBuffer` 및 `JobTemp` 경고가 없다. 로그는 `Logs/Revival/PlayerValidation/<실행 이름>/`에 보존했다. `warning-review-fixed-20261108/player-verify.json`은 저장 재시작·오디오 PCM·입력·세 해상도 오프스크린 렌더링·300초 전투/씬 반복의 7개 사례를 통과했다. 이는 실제 Windows 실행·종료 증거이며 Editor 빌드 종료의 별도 경고까지 해결됐다는 뜻은 아니다.

## 전체 Editor 재컴파일에서 추가 확인한 경고

패키지 의존성을 갱신한 Audit의 Editor 어셈블리 컴파일에서 플레이어 빌드 집계에는 없었던 CS0618 14개가 확인됐다. 각 경고가 컴파일 명령 출력과 Unity 요약에 반복되어 로그 행 수와 원인 수는 다르다. 별도 `LunaValidation` UAC0005 경고도 현재 Unity가 제공하는 어셈블리 열거 API로 수정했다.

- Spine Collider Inspector 2개와 Rigidbody2D Inspector 1개: 런타임과 동일한 `compositeOperation`, `bodyType` 의미로 변경했다. 3D Rigidbody의 정상 `isKinematic` 속성은 유지한다.
- Spine build define 도구 4개: 기존 BuildTargetGroup을 `NamedBuildTarget.FromBuildTargetGroup`으로 변환해 최신 PlayerSettings define API에 전달한다.
- Spine 에디터 데이터 재로딩 4개: 순서에 의존하지 않는 활성 SkeletonRenderer/SkeletonGraphic 전체 열거를 `FindObjectsByType<T>()`로 변경했다.
- Spine Sprite Slice 도구 2개: 지원이 제거된 `TextureImporter.spritesheet` 대신 `ISpriteEditorDataProvider`와 `ISpriteNameFileIdDataProvider`를 사용한다. 기존 slice의 GUID, Sprite local fileID, pivot, border를 유지하고 새로운 slice에만 GUID/name 매핑을 추가한다. 일반 atlas texture도 Sprite/Multiple로 전환해 실제 Sprite subasset을 생성한다. Spine Editor asmdef에 `Unity.2D.Sprite.Editor` 참조를 추가했다.
- iTweenPath scene handle 1개: 변경 감지 후 값 대입 전에 `Undo.RecordObject`를 호출한다. prefab instance 변경 기록과 dirty 표시를 수행해 실제 Undo/저장 의미를 보존한다.

`LunaSpriteSliceRegression.Run()`은 고유 이름의 임시 에셋 폴더에서 실제 Spine 명령을 호출한다. 기존·무관한 slice의 식별자/설정 보존, 좌표 갱신, 90도 회전 slice 추가, 두 번째 실행의 멱등성, 기본 texture의 Sprite 변환을 검사하고 finally에서 해당 임시 폴더만 삭제한다. 검수 Audit에서 실행 결과를 기록한다.

## 추가 자동 검사에서 드러난 진행·입력 오류

- `DataManager.AllStagePrefab`는 원래 스테이지 선택창을 열 때만 채워졌다. 시작 직후 `SetStagePrefab`를 호출하면 아무것도 선택하지 못했고, 선택창을 열지 않고 다음 스테이지로 진행하면 이전 프리팹을 유지할 수 있었다. 이제 매니저 초기화에서 운영 스테이지를 읽고, 존재하지 않는 이름은 명시적으로 거절한다. 자동 검사도 관리자의 표시 이름뿐 아니라 실제 생성된 스테이지 객체 이름을 비교한다.
- `UIPlayer`의 키보드 이동은 누르고 있는 A/D·방향키 상태를 매 프레임 반영한다. 양쪽 동시 입력은 정지하고 한쪽을 놓으면 남은 방향으로 계속 이동하며, 플레이어가 사망하거나 아직 생성되지 않았을 때 안전하게 처리한다. 키보드를 사용하지 않을 때 화면 이동 버튼의 상태를 지우지 않는다.
- PC 자동 검사는 Unity 배치 모드 및 별도 개발 플레이어의 격리 저장 경로에서 실행한다. 2026-11-08 추가 요청 이후 데스크톱 화면·마우스·키보드를 직접 조작하는 검수는 중단했다. 실제 기기 조작감과 스피커 청감은 자동 통과 항목에 포함하지 않는다.

## 실제 실행 중 확인한 Entity·전투 상태 오류

- `LunaEntityBehaviour.EnsureEntityInitialized`는 `OnEntityInitialize`가 객체를 숨기거나 해제한 뒤에도 기본 시작 코루틴을 실행했다. 이제 초기화 직후와 코루틴 시작 직전에 객체 생존·해제 여부와 `isActiveAndEnabled`를 확인한다. 기본 `OnEntityStart`는 빈 코루틴 대신 `null`을 반환한다. 초기화 중 정상적으로 끝나는 일회성 Entity에 다시 작업을 시작하지 않는다.
- `Effect`는 루트 또는 이름이 `Sprite`인 자식의 ParticleSystem만 확인해 `SerenBlessing`의 정상 오라를 즉시 숨겼다. 이 프리팹은 루트 아래 이름 있는 오브젝트에 ParticleSystem과 네 자식 시스템이 있으며 모두 반복 재생하도록 설정돼 있다. 이제 활성 자식의 모든 ParticleSystem을 찾고 하나라도 살아 있는 동안 효과를 유지한다. 강제 Play나 loop 설정 변경은 없으며 버프의 기존 5초 수명과 대상 사망 시 숨김 동작을 유지한다.
- `UnitAI`, `UnitAI_Player`, `DwarfAI`의 마나 충전은 Stamina가 양수일 때만 수행한다. DragonPet처럼 Stamina가 0인 수동/상시 효과 유닛에서 `100 / 0`과 정지 프레임의 deltaTime 0이 결합해 Mp가 NaN이 되던 실제 오류를 수정했다. 양수 Stamina의 충전식과 기존 `Mp > 100` 스킬 발동 조건은 유지하며 100으로 강제 고정하지 않는다. CSV의 0 값이나 원본 프리팹 수치를 바꾸지 않았다.

### 확인된 실행 결과와 남은 종료 진단

| 보고서 | 확인된 범위 | 결과 |
| --- | --- | --- |
| `Logs/WarningReview/build-windows-clean-06.json` | Windows 전체 정리 빌드, 운영 씬 4개 | 성공, 빌드 오류 0·경고 0. 뒤에 추가된 런타임 수정의 최종 빌드 결과는 별도 기록한다. |
| `Logs/WarningReview/matrix-skins-05.json` | 7개 스킨의 실제 진입·HP 0 사망·재출현·Entity 소유권. Dragon은 씬 재진입 시 부모 재사용, 유일한 펫, 유한한 전투 수치와 원본 생성 위치도 확인 | 7/7 통과, 오류 0·경고 0 |
| `Logs/WarningReview/matrix-skills-07.json` | 장착 UI 버튼을 통한 14개 실제 스킬의 마나·쿨다운·전투 효과 | 14/14 통과, 오류 0·경고 0 |
| `Logs/Revival/PlayerValidation/warning-review-fixed-20261108/player-verify.json` | 두 번째 Windows 프로세스의 저장 복원, PCM 음소거 전환, 키 유지/해제, 1280×720·1920×1080·1024×768 렌더링, 300초 반복 | 7/7 통과, 오류 0·경고 0; 정상 종료 로그에 ComputeBuffer 경고 없음 |

SerenBlessing 검사는 버프 수치만 확인하지 않는다. 실제 대상 추적 오라 2개의 중첩 ParticleSystem이 재생 중이며 살아 있는 표본 201개를 기록했고, 게임 시간 약 5초까지 살아 있다가 약 5.305초에 둘 다 자연스럽게 해제됐음을 확인했다. NoGraphics 실행의 이 항목은 파티클 시뮬레이션과 Entity 수명을 검증하며 화면상 색·모양의 판정은 포함하지 않는다.

Editor 빌드 프로세스 종료의 `JobTempAlloc`은 별도 미해결 진단 항목이다. `Logs/WarningReview/build-windows-diag-08.log`와 `build-windows-drained-09.log`의 할당 스택은 네이티브 `ShaderCompilation::CompileVariantJob`, 셰이더 캐시/파라미터/리소스 읽기 경로를 가리키며 프로젝트 C# 할당 스택은 없다. 후자의 전체 빌드는 오류 0·경고 0으로 성공했고, BuildPlayer 반환 후 실제 Editor 업데이트 2회(1.247초) 동안 셰이더·스크립트 컴파일 및 에셋 갱신이 모두 유휴 상태임을 확인한 뒤 종료했다. 그럼에도 종료 시 동일한 5개 할당(504+582+152+64+64 = 1,366바이트)이 age 2/3, lifetime 15, state OK로 두 번 보고됐다. 두 번의 출력은 주소가 같은 할당의 재보고이며 10개 고유 할당으로 합산하지 않는다. 따라서 짧은 유휴 대기는 이 경고를 해결하지 못했다.

Unity의 공개 문서는 TempJob 메모리를 소유자가 해제해야 한다고 명시하며, 허용 프레임 수를 지나면 자동으로 회수한다는 보장은 없다. `Allocator.Temp`의 프레임/잡 끝 일괄 해제와 TempJob의 수명 제한을 혼동하지 않는다. [Unity Collections allocator 설명](https://docs.unity3d.com/Packages/com.unity.collections@2.5/manual/allocator-overview.html) 네이티브 선형 할당기도 메모리가 더 이상 필요 없을 때 블록을 풀로 반환하는 구조이며, 로그의 `lifetime=15`만으로 16번째 Editor 업데이트가 해제를 보장한다고 해석하지 않는다. [Unity 선형 할당기 설명](https://docs.unity.com/en-us/engine/6000.0/manual/analysis/performance-memory/native-memory/allocator-examples/threadsafe-linear-allocator)

확인한 공개 이슈 중 [UUM-115886](https://issuetracker.unity.com/issues/7743/jobtempalloc-memory-leak-warning-is-thrown-when-the-player-is-shut-down)은 GPU Resident Drawer를 쓰는 Windows Player 종료 문제이며 6000.6.0a7에 수정됐다. 현재 6000.6.0f1 Editor의 셰이더 작업 스택과 동일 문제라고 단정할 근거는 없다. 정상 Editor 업데이트를 더 오래 허용하는 단 한 번의 제한된 비교 실험은 가능하지만, 이는 지연 해제 여부를 구분하는 진단이며 검증된 해결책은 아니다. 후속 `build-windows-drain16-12`에서는 실제 16회 유휴 Editor 업데이트(1.177초)를 기다려도 같은 5개 할당이 age 2/3으로 남았다. Editor 업데이트 횟수와 할당기의 프레임 나이는 동일하지 않았다. 이 비교도 경고를 해결하지 못했으며, 더 임의의 대기를 늘리거나 게임 수치를 바꾸지 않고 이 Unity 버전의 네이티브 셰이더 작업 종료 진단으로 남긴다. 강제 GC·경고 필터·비공개 엔진 정리 호출은 추가하지 않았다.

전체 실행 범위와 이후 최종 빌드 결과는 `Docs/Revival-QA.md` 및 실행별 JSON/종료 로그에 기록한다.

최종 영웅 등급 초기화 수정 후 Windows 빌드 `build-windows-drain16-12`도 오류 0·빌드 경고 0으로 성공했다. 이를 실행한 `warning-review-final-gated-20261108`의 seed와 verify는 모두 통과했고, verify는 7개 사례·오류 0·경고 0이다. 약 303초에 걸친 11회 전투/메뉴 전환과 정상 종료에서 ComputeBuffer/JobTemp 경고가 재발하지 않았다. 이 결과와 별도로 위 Editor 종료 진단은 유지된다.

최종 3회 검수 RunId는 `warning-review-final3-20261108`이다. 세 회차의 Audit/Smoke가 모두 통과했고, 3회차 CleanBuildCache 빌드는 05:58:22 UTC에 정상 종료했다. 빌드 오류 0·경고 0 및 로그의 C#/셰이더 경고 0을 다시 확인했다. 종료 후 동일한 네이티브 셰이더 JobTemp 할당 5개/1,366바이트가 두 번 보고되어 미해결 상태로 남겼다. Smoke의 경고 4개는 의도적으로 손상시킨 격리 저장 파일 검사이며 실제 Windows 실행 경고와 구분한다.

3회차의 최종 실행 파일 자체도 `warning-review-final3-exe-20261108`에서 새 seed/verify 프로세스로 다시 실행해 7개 사례가 오류 0·경고 0으로 통과했다. 이 마지막 확인은 짧은 반복(실제 약12.36초/1회)이며 앞선5분/11회 부하 기록과 구분한다. 최종 플레이어 종료 로그에도 ComputeBuffer/JobTemp/예외가 없다.


## 최종 r04의 Windows 빌드 C — 종료 로그 판정

r04 3회차의 새 clean headless Windows 빌드 C는2026-11-08 12:42:18 UTC에 BuildReport 오류0·경고0으로 성공했다. 근거는 `Logs/Revival/FinalReview/native-final3-20261108-r04/pass-03/20261108-122516-306/build-windows.json/.log`이며, 닫힌 전체 로그의 주소별 집계는 `Logs/NativeRewrite/final-r04-build-audit.json`에 보존한다. 이 빌드는 앞선 A/B와 다른 현재 최종 소스의 실행이다.

그러나 C에서도 Editor 종료 시 `ShaderCompilation::CompileVariantJob`의 **서로 다른5개 할당·총1,366바이트**가 남았다. 주소/크기는 `703d7370`/504, `703d79f0`/582, `703d7d00`/152, `703d7e60`/64, `703d7f70`/64이며, 같은5개를 age3/4에서 각각 출력했다(lifetime15). 두 덤프를10개 또는2,732바이트로 중복 계산하지 않는다.4개 스택에는 shader cache의 subprogram/parameter 읽기가 포함된다.16회 정상 Editor 유휴 갱신(0.656초) 및 `-nographics` 후에도 재현됐으므로 native 경고를 해결했다고 판정할 수 없다. 마지막 UTP `MemoryLeaks`의36,158,078바이트는 여러 Editor/엔진 레이블의 별도 집계이며 이1,366바이트나 프로젝트 게임 객체의 누수량으로 해석하지 않는다.

C 전체 로그에서 C#/UAC·셰이더 경고, ComputeBuffer, 게임 코드의 NullReference/MissingReference/InvalidOperation/IO 예외는 관측되지 않았다. 다만 초기 라이선스 서명/토큰 진단 뒤 entitlement/갱신 성공, primary listening socket 실패 뒤 PlayerConnection endpoint/multicast 등록, Bee cache-client `move_path failed` 뒤 같은 Csc 노드 CacheWrite/빌드 성공, 종료 Curl42와 worker session 메시지는 원문에 남는다. 따라서 BuildReport0/0을 전체 프로세스의 모든 진단0으로 표현하지 않는다.

C 빌드 종료 당시에는 Player와 최종 지문이 대기였으므로 완료를2/3회로 유지했다. 이후 Player300초·완전 종료 로그·최종 지문을 확인해 r04는3/3회 완료했다. native 종료 집계는 완료된 기능 검수와 별도로 남는다.


최종 C Player의 runtime JSON은 seed2/verify8·302.3273초11회 순환을 오류·경고0으로 기록했다. 완전 종료 로그에는 JobTemp/ComputeBuffer/관리 예외가 없었지만 `PlayerConnection::Cleanup##utp:`의 native `MemoryLeaks.allocatedMemory`가 seed41,463/verify45,302바이트로 남았다. 프로젝트 객체에 귀속하거나 Editor1,366바이트와 동일 원인으로 단정할 근거는 없다. 이 종료 집계는 미해결·귀속 미확인으로 보존한다.


A의 닫힌 Player 로그도 같은 UTP native 집계(seed41,463/verify45,302바이트)를 갖는다. 따라서 C에서 새로 증가했다고 주장하지 않으며, 같은 값이라는 이유로 무해하거나 해결됐다고 판정하지도 않는다. 주소/소유권 스택이 없어 정확한 귀속은 미확인이다. 비교 근거: `Logs/NativeRewrite/final-r04-player-exit-audit.json`.
