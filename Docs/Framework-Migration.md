# Game Framework 전환 구조

## 실행 구조

제공된 `GameFramework-Core-0.1.0.unitypackage`와 `GameFramework-Unity-0.1.0.unitypackage`의 코어 DLL·Unity 컴포넌트를 사용한다. Unity 패키지가 포함하는 `Packages/manifest.json` 전체를 덮어쓰지 않고 Addressables와 SharpZipLib 의존성을 병합했다.

- `Assets/Plugins/GameFramework.dll`: 순수 C# 코어.
- `Assets/Scripts`: `UnityGameFramework.Runtime`와 Editor 어셈블리. 외부 프레임워크 소스와 라이선스.
- `Assets/LunaFramework`: 루나 게임의 호환 어댑터와 실행 서비스. 이전 embedded package를 이 위치로 옮겼으며 기존 스크립트·프리팹·데이터의 `.meta` GUID를 유지했다.
- `Assets/Script`와 기존 프리팹 스크립트: 게임 규칙, AI, 스킬, UI, 저장 데이터.

`EverydayDevup.Framework` 네임스페이스와 `Game.Instance`/`PoolManager` 호출 표면은 기존 씬·프리팹 참조를 보존하는 호환 인터페이스다. 실행 서비스는 새 Game Framework 모듈을 사용한다. 어셈블리 이름은 `Luna.Framework`다.

시작 씬은 `Assets/Scenes/Loading.unity`다. 영속 Game 오브젝트가 루나 소유 GameFramework 프리팹을 생성한다. 프레임워크 Helper의 Start 이후 Entity 서비스를 준비하고 `Game.BeforeInitialize`를 호출한다. 이 이벤트에서 런타임 데이터·저장값을 복원한 뒤 매니저와 Procedure를 시작한다.

## 모듈의 실제 책임

| 모듈 | 루나에서 관리하는 동작 |
| --- | --- |
| Procedure/FSM | Loading/Lobby/Battle 상태 진입, UI와 선택된 스테이지 생성. 씬 핸들로 중복 진입을 막는다. |
| Entity | 플레이어·유닛·몬스터·발사체·이펙트·스테이지·게임플레이 보조 객체의 표시, 숨김, 초기화, 업데이트, 인스턴스 풀. |
| UI/Event | 기존 UIAddressable 레지스트리를 사용하는 폼 로드·열기·닫기·실패 이벤트. |
| Resource | UI와 음악의 Addressables 로드/해제, Entity가 사용하는 명시적 resident prefab 키. |
| ObjectPool | Entity/UI가 내부 인스턴스 풀을 소유한다. 기존 독립 GameObject 풀은 실행 경로에서 사용하지 않는다. |
| Sound | `Luna/Audio/Battle` 배경음과 사운드 그룹. |
| Setting | 격리된 런타임 진행 상태와 소리 설정의 저장·복원. |
| DataTable | CSV 기반 UnitBalance 행. 상세 내용은 `UnitBalance-DataTable.md`를 참고한다. |

씬 자체는 기존 Build Settings에 등록된 씬을 `LoadSceneMode.Single`로 전환한다. 이 경로는 Addressables SceneComponent로 바뀐 것이 아니다. Procedure가 게임 상태 진입을 관리하고 `LunaSceneManager`가 씬 전환·페이드·정리를 연결한다.

## Entity 생성과 수명

```csharp
// 일회성 객체. 매번 새로운 수명과 초기 상태를 가진다.
var effect = LunaEntities.Spawn(prefab, position, rotation);

// 반복 사용하는 전투 객체. 기존 PoolManager도 이 경로를 호출한다.
var actor = LunaEntities.Spawn(prefab, position, rotation, parent: null, reusable: true);

// 반환/제거는 GF Entity.HideEntity를 거친다.
LunaEntities.Hide(actor);
```

`LunaEntities.Spawn`은 실제 `EntityComponent.ShowEntity`를 호출한다. 기존 동기식 호출자가 반환된 객체에 타깃·타워·스킬 데이터를 바로 설정할 수 있도록, 이미 직렬화된 프리팹 참조를 ResourceManager의 resident 키로 등록한다. Entity helper가 해당 프리팹을 인스턴스화하며 `.meta` GUID와 기존 프리팹 필드는 바꾸지 않는다. Addressables 외부 키는 기존 Addressables 구현을 그대로 사용한다.

반복 객체는 `LunaGameplay` 그룹의 안정적인 프리팹 키를 사용한다. 일회성 객체는 `LunaTransient` 그룹의 생성별 고유 키를 사용하므로 이전 인스턴스 상태가 재사용되지 않는다. 이 그룹의 용량·만료·해제 주기는 0이며, GF가 숨긴 객체를 다음 업데이트에서 recycle한 후 인스턴스와 resident 등록을 해제한다. `WarmPool`은 프리팹과 용량을 등록하며, 게임플레이 초기화를 실행하는 미리 생성은 하지 않는다.

`LunaEntityLogic` 하나가 같은 GameObject에 붙은 여러 `LunaEntityBehaviour` 모듈을 관리한다. Status·AI·플레이어 상태 머신이 같은 오브젝트에 있어도 GF Entity는 하나다. 기존 씬이나 UI에서 생성한 오브젝트의 모듈은 기본 Start에서 `LunaEntities.Adopt`로 기존 오브젝트를 등록한다. 이미 Entity로 생성된 오브젝트는 다시 등록하지 않는다.

## 게임 코드의 라이프사이클

게임플레이 모듈은 `LunaEntityBehaviour`를 상속한다. 기존 MonoBehaviour에서 이름만 바꾼 구조가 아니라, 아래 콜백을 `LunaEntityLogic`의 GF OnShow/OnUpdate/OnHide와 Entity helper의 Release가 호출한다.

| 콜백 | 호출 시점 |
| --- | --- |
| `OnEntityAwake()` | 최초 표시 시 한 번. 네이티브 활성화 후라 Spine 등 Unity 컴포넌트의 Awake가 준비되어 있다. |
| `OnEntityShow()` | 매 표시 시. 최초 여부는 `IsFirstEntityShow`로 구분한다. |
| `OnEntityInitialize()` | 최초 GF 업데이트 직전 한 번. Spawn 직후 호출자의 타깃·타워 설정이 먼저 끝난다. |
| `OnEntityStart()` | 필요한 경우 기존 coroutine Start를 옮기는 초기 코루틴. |
| `OnEntityUpdate(elapsed, realElapsed)` | GF Entity 업데이트. 같은 객체의 활성 모듈을 먼저 초기화한 뒤 업데이트한다. |
| `OnEntityHide()` | 매 숨김 시. 코루틴을 중지하고 전투 상태·구독·표시 자원을 정리한다. |
| `OnEntityRelease()` | 실제 인스턴스 폐기 시 한 번. |

`EnsureEntityInitialized()`는 반환 직후 명시적인 초기화가 필요한 호출을 위한 API다. 플레이어의 `PrepareForSpawn`은 타워 참조와 초기화 여부를 보존하며 재사용 시 초기 보너스를 중복 적용하지 않는다. 사망 시 부활 큐는 이전 객체가 해제되기 전에 필요한 데이터를 확보한다.

재사용 객체는 `OnEntityShow`에서 매 출현 상태를 초기화해야 한다. `OnEntityInitialize`에서 한 번 만드는 상태를 출현마다 새로 만든다고 가정하면 안 된다. 기존 AI/Status의 재사용 재설정은 이 계약에 맞춰 연결되어 있다.

## 씬·UI·부모 오브젝트 정리

씬 전환 전에 열린 UI와 대기 로드를 닫고 모든 활성 Luna Entity를 숨긴다. `sceneUnloaded`에서도 정리를 수행해 직접적인 씬 전환을 방어한다. UI 폼은 `OnClose`에서 하위 Entity를 먼저 숨긴다. 숨긴 Entity는 입양한 UI/씬 자식까지 GF 그룹 루트로 옮겨, 부모 Canvas나 일회성 부모가 먼저 파괴해 recycle 대기 객체를 잃지 않게 한다.

UI 탭처럼 부모만 비활성화되고 자식의 activeSelf가 유지되는 경우는 임시 정지다. Entity 등록과 부모 계층을 그대로 유지하고 비활성 모듈의 업데이트만 건너뛴다. 탭을 다시 열면 같은 미리보기 객체가 돌아온다. UI 폼 자체가 닫힐 때에만 명시적으로 하위 Entity를 Hide한다.

GF는 Hide와 recycle을 다른 업데이트에서 수행할 수 있다. Unity가 외부에서 이미 파괴한 입양 객체도 recycle에 필요한 관리 객체의 동일성을 보존하도록 Unity Entity 컴포넌트가 GameObject handle을 캐시한다. 이미 파괴된 로직에 OnRecycle을 다시 실행하지 않으며, 풀과 resident 자원 해제는 계속 완료한다.

게임 코드에서 등록된 Entity를 직접 `Destroy`하거나 활성 토글로 재생성하는 것은 지원 계약이 아니다. 반드시 `LunaEntities.Hide`/`Spawn`을 사용한다. 특히 재사용 풀의 객체를 직접 파괴하면 해당 인스턴스를 다시 사용할 수 없다. handle 캐시는 UI/씬의 외부 파괴 경계에서 정리를 완료하기 위한 방어이지, 직접 Destroy를 권장하는 API가 아니다. 새 프리팹·플러그인이 자체 파괴 기능을 추가할 때는 Entity helper와 수명을 연결해야 한다.

## 남겨 둔 Unity 연결부

Unity의 물리 충돌·트리거, FixedUpdate/LateUpdate가 필요한 엔진 단계, UI 버튼·캔버스·포인터 이벤트, 렌더링·Spine·서드파티 컴포넌트는 해당 네이티브 콜백을 사용할 수 있다. UI는 `UIFormLogic`을 통해 관리하며 버튼마다 별도 Entity를 만들지 않는다. 에디터 도구, 데이터 ScriptableObject, 입력·카메라·저장 이벤트 연결부도 역할에 맞는 Unity 타입을 유지한다. 일반 게임 규칙의 매 프레임 업데이트는 Entity 또는 UI 모듈의 업데이트 콜백을 사용한다.

## 검증

검증 명령과 범위는 `Revival-QA.md`를 참고한다. 실제 판정은 `Logs/Revival`의 Audit/Smoke/Build 및 회차별 보고서에 남는다. Smoke는 활성 Luna Entity 수를 native EntityManager 수와 비교하고 활성 게임 모듈의 등록, 씬 전환, 메뉴의 미리보기 객체 정리와 플레이어 사망·부활·재사용을 확인한다. 이 문서 자체는 전체 콘텐츠 통과 판정이 아니다.
