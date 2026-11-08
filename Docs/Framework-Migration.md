# Game Framework 전환 구조

이 문서는 ToyboxNightmare의 실제 작성 방식에 맞춰 다시 작성한 현재 소스 구조를 설명한다. 이전 `LunaEntityBehaviour` 호환 브리지의 구조와 그 시기의 검수 결과는 현재 구현의 완료 증거가 아니다. 이 재작성 이후의 Unity Audit·런타임·Windows 검증은 별도 보고서로 판정한다.

## 패키지와 게임 코드의 경계

- `Assets/Plugins/GameFramework.dll`은 제공된 `GameFramework-Core-0.1.0.unitypackage`의 DLL과 SHA-256이 같다. 해시는 `b42d5cd6343c8a5f9ff0d4a5d9219fadf34b5068cad66d75387052c6e0fe943b`이다.
- `Assets/Scripts`는 제공된 Unity 패키지의 `UnityGameFramework.Runtime` / Editor 코드다. 패키지 라이선스를 유지한다. 필요한 프로젝트 의존성을 병합하며 제공 패키지의 manifest 전체로 프로젝트 설정을 덮어쓰지 않는다.
- `Assets/Script/Runtime`과 `Assets/Script/Contents`는 루나 게임의 네이티브 Procedure·EntityLogic·EntityData·UIFormLogic 및 게임 규칙이다. 대부분의 새 공통 게임 타입은 `Luna.Gameplay` 네임스페이스를 사용한다.
- 이전 `Assets/LunaFramework`, embedded Everyday 패키지, 12개 매니저 레지스트리, `Game` 서비스 파사드, 사용자 정의 BehaviourTree/State/Subject/Singleton, 독립 GameObject 풀은 새 실행 경로에 없다. `LunaSession`은 선택한 스테이지·플레이어·임시 획득 정보 등의 일반 게임 상태이며 프레임워크 서비스 레지스트리가 아니다.

Toybox에서 적용한 것은 **각 액터가 실제 EntityLogic을 소유하고, 타입이 있는 EntityData를 받은 뒤 native FSM을 시작하며, Procedure가 일반 C# 게임 세션을 소유하는 작성 패턴**이다. Toybox의 게임 규칙이나 데이터 표현을 루나에 복사한 것이 아니다. 루나의 밸런스·스테이지 데이터는 승인된 CSV/DataTable 방식으로 읽는다.

## 시작과 씬 수명

`Assets/Scenes/Loading.unity`가 유일한 Build Settings 시작 씬이다. GUID를 보존한 `Assets/Prefabs/LunaBootstrap.prefab`에는 카메라·Canvas·페이드 및 일곱 플레이어 프리팹 연결이 있다. `LunaBootstrap : GameFrameworkComponent`가 제공 GameFramework 프리팹을 만들고, 활성화된 native `ProcedureComponent`가 `LunaLaunchProcedure`를 시작한다.

Launch는 Entity 카탈로그, 세션, 런타임 저장 데이터, 스테이지 테이블과 UI 정의를 준비한다. 이후 흐름은 다음과 같다.

```text
LunaLaunchProcedure → LunaLoadingProcedure
                            ↓ 요청
                    LunaTransitionProcedure
                       ↙             ↘
           LunaLobbyProcedure    LunaBattleProcedure
              LunaLobbyGame       BasicDungeon / BossDungeon / EndlessDungeon
```

`LunaScenes`는 이동 요청과 현재 상태 조회를 제공한다. 실제 비동기 씬 로드·언로드는 supplied `SceneComponent`와 Resource/Addressables가 수행한다. Lobby/Game/Dungeon은 각각 정확한 `Assets/Scenes/*.unity` 주소를 사용한다. 일반 플레이 경로에서 Unity SceneManager로 직접 단일 씬을 교체하지 않는다.

이전 Procedure의 `OnLeave`에서 게임 세션을 종료한 후 Transition이 씬을 언로드한다. 성공·실패 native 이벤트는 요청자와 에셋 주소를 함께 확인한다. 실패는 로그와 `LunaScenes.LastError`에 남기고 Loading으로 돌아간다.

로비도 `LunaLobbyGame`이 배경과 일곱 로스터 슬롯의 native 요청 ID·버전을 소유한다. 배경이 표시된 뒤 UI를 열며, `UILobby.OnOpen`은 배우를 다시 생성하지 않는다. 덱·스킨·유닛 성장값·플레이어 장비/스킬 구성을 비교해 실제로 바뀐 슬롯만 갱신한다. 늦은 생성 콜백은 현재 로비 세션, 슬롯 버전과 현재 `LunaSession`을 확인한다. 로비 종료 시 요청과 배우를 취소한다. 이전 `MyUnits` 정적 생성/정리 파사드는 삭제했다.

로비 배우의 클릭 가능 여부는 native UI의 로드 중/열린 화면에서 계산한다. Lobby·Goods·GameOption만 열려 있으면 허용하고, 팝업이나 다른 화면이 있으면 차단한다. 중첩 팝업 하나를 닫았다고 아래 Upgrade 화면의 차단까지 풀리지 않으며, 마지막 차단 화면을 닫을 때 변경된 성장/장비 데이터를 로스터에 반영한다. UI가 개별 Collider를 임의로 켰다 껐다 하는 호출은 제거했다.

## 실제 액터 작성

`EntityLogicBase : UnityGameFramework.Runtime.EntityLogic`는 생성 데이터의 소유권, 기본 transform 복원, 안전한 Hide, ReferencePool 반환만 공통 처리한다. 같은 GameObject의 MonoBehaviour 모듈들을 수집해서 Start/Update를 대신 호출하는 브리지는 없다.

| 종류 | 실제 소유자와 데이터 |
| --- | --- |
| 일반 유닛·몬스터 | `UnitLogic` + `UnitData`, `IFsm<UnitLogic>` |
| 영웅 | `HeroUnitLogic`의 구체 영웅 클래스가 등급·스킬 규칙을 구현 |
| 플레이어 | `PlayerLogic`의 구체 스킨 클래스 + `UnitData`, `IFsm<PlayerLogic>` |
| 타워·포탑 | `TowerLogic` + `TowerData` |
| 발사체 | `ProjectileLogic` 계열 + `ProjectileData` |
| 이펙트·체력바·피해 숫자·버프 아이콘·드롭 | 각각의 구체 EntityLogic과 생성 데이터 |
| 스테이지 배경 | `LunaStageBackdropLogic` + `LunaStageEntityData` |

`UnitView`는 프리팹의 Definition, 발사체·이펙트·소리 등 authored 연결을 제공하는 데이터 컴포넌트다. 전투 Update를 실행하지 않는다. 기존 Status/AI/스킬 MonoBehaviour 조합의 참조는 프리팹 변환에서 이 연결로 옮긴다. Entity 카탈로그의 `LogicType`이 프리팹별 구체 클래스를 정하며 native Entity helper가 해당 클래스를 인스턴스에 추가한다.

생성 시에는 요청 전에 데이터를 완성한다.

```csharp
var data = UnitData.CreateFrom(prefab.GetComponent<UnitView>(), position);
data.RoundId = currentGame.RoundId;
data.Level = level;
data.InitialMode = UnitSpawnMode.Battle;
int entityId = LunaSpawn.Show<UnitLogic>(prefab, data,
    shown: actor => { /* 이 시점에 native OnShow가 완료된 actor 사용 */ },
    failed: error => Debug.LogError(error));
```

`Show`의 반환값은 요청 Entity ID다. GameObject나 초기화가 끝난 유닛을 동기 반환하지 않는다. 이후에는 `LunaSpawn.Find<T>(id)`로 해당 수명의 활성 객체를 조회한다. 타깃·부모 소유자·라운드도 가능한 곳에서 인스턴스 참조 대신 ID로 추적한다.

`UnitLogic.OnShow`는 새 데이터와 초기 상태·등급을 적용한 후 native FSM을 만든다. `PlayerLogic`도 자신을 소유자로 하는 별도 native FSM을 만든다. Hide에서 FSM, 구독, 소유한 보조 객체와 능력 상태를 정리한다. 같은 풀 인스턴스를 다시 표시해도 이전 수명의 FSM·타깃·코루틴 상태를 재사용하지 않는다. 프리뷰 모드는 전투 FSM을 시작하지 않는다.

플레이어의 장착 스킬·장비 능력 값은 `PlayerLoadoutSnapshot`으로 비동기 Show 요청 전에 복사한다. 로드가 끝나기 전에 선택 화면의 설정이 바뀌어도 이미 요청한 배우의 구성은 바뀌지 않는다.

배경의 문·스크롤 판·장식 악마·효과 방출기·로비 Sparkles는 `LunaStageBackdropView`의 명시적 참조를 받아 `LunaStageBackdropLogic`이 갱신한다. 이전 LobbySparkles MonoBehaviour는 제거했다. Sparkles의 원래 60 FPS 기준 속도는 초당 이동량으로 변환했고 이동 범위를 제한한다. 원본 로비에서 비활성이던 Sparkles는 그대로 비활성으로 시작한다.

## 비동기 생성, 취소와 데이터 반환

`LunaSpawn`은 native ShowEntity 성공·실패 이벤트를 요청 ID로 연결하는 게임 코드다. 게임 로직 업데이트나 인스턴스 생성을 직접 수행하지 않는다. `LunaEntityData : IReference`는 생성 요청마다 획득하며, 성공적인 base OnShow에서 Entity가 소유권을 넘겨받는다.

- 정상 Hide는 생성 데이터를 ReferencePool에 한 번 반환한다.
- 로드 실패 또는 OnShow 이전 실패는 아직 Entity가 소유하지 않은 데이터를 요청 쪽에서 반환한다.
- OnShow 내부 실패는 supplied Entity의 `InitializationError`로 관찰하고 native Hide를 통해 정리한다. 성공 이벤트만으로 초기화 성공을 가정하지 않는다.
- 파생 OnHide가 예외를 내더라도 base OnRecycle에 남은 데이터 반환 경로가 있다. 예외 자체는 숨기지 않는다.
- `Cancel` / `CancelRound`는 아직 완료되지 않은 로드도 취소 표시한다. 늦게 완료된 객체는 게임 콜백에 전달하지 않고 정리한다. 자원 로드 핸들이 끝나기 전에 데이터를 임의로 풀에 돌리지 않는다.

게임 코드가 활성 Entity를 직접 Destroy하거나 SetActive로 풀에 반환하는 것은 지원 계약이 아니다. `SafeHide` 또는 요청 취소를 사용한다. Hide 이후 native recycle은 다음 업데이트에 일어날 수 있으므로 부모 UI가 먼저 파괴되기 전에 Entity를 native 그룹 루트로 분리한다.

## 자원과 풀

UI·음악·세 씬은 일반 Addressables 에셋이다. 기존 `Assets/Resources` 액터 프리팹은 GUID와 authored 에셋 연결을 유지하면서 `LunaResourcesProvider : ResourceProviderBase`가 `Resources.LoadAsync<GameObject>`로 공급한다. 게임이 등록한 ResourceLocationMap은 카탈로그의 정확한 `Assets/Resources/...prefab` 키를 Resources 경로로 해석한다.

이 provider는 Addressables의 공개 확장점이다. supplied ResourceManager는 계속 Addressables 로드 핸들을 관리하고, native Entity helper가 인스턴스를 생성·풀링·폐기한다. 동기 resident-prefab 사전이나 기존 인스턴스 입양 경로는 제거했다. Resources 액터를 Addressables 번들에도 중복 등록하지 않는다.

Entity 인스턴스 풀은 native Entity/ObjectPool 모듈이 소유한다. 원본 프리팹 자원은 카탈로그의 에셋 참조와 함께 유지되며 GameObject 에셋에 지원되지 않는 `Resources.UnloadAsset`을 호출하지 않는다. 원본 prefab 수명과 runtime instance 수명은 다르다.

## 일반 C# 라운드와 데이터 테이블

`BasicDungeon`, `BossDungeon`, `EndlessDungeon`은 MonoBehaviour가 아닌 `LunaGame` 파생 클래스다. `LunaBattleProcedure`가 하나를 생성하고 `Initialize/Update/Shutdown`을 호출한다. 라운드는 증가하는 RoundId와 비동기 대기 집합을 갖는다. 종료 시 먼저 비활성으로 표시하고 해당 라운드의 대기 요청과 활성 Entity를 취소한다.

라운드는 스테이지 인트로·웨이브·승패·보상·다음 층 연출을 진행하며, 플레이어의 native `PlayerDiedEventArgs`를 받아 현재 라운드와 플레이어 ID를 확인한 후 부활을 예약한다. UIPlayer가 부활이나 전투 능력의 수명을 소유하지 않는다. 플레이어 능력은 `PlayerLogic.Abilities`가 소유하고 UI는 입력과 표시를 연결한다.

`LunaBattleRoster`는 라운드가 소유하는 일반 C# 객체다. 여섯 덱 슬롯, 전투 Gold/Mana(0–100), 소환 대기 ID, 재소환 대기 시간과 소수 단위 수입 누적을 보관한다. 영웅의 살아 있는 ID와 대기 요청을 함께 확인하고, 실패한 생성 요청의 금액은 한 번만 환불한다. Dungeon04의 무료 초기 파티도 StartCombat에서 한 번만 생성한다. UIPlayer를 닫았다 열어도 새 로스터를 만들거나 보상을 지우지 않는다. HUD가 사라진 동안의 생성 완료 역시 화면의 Available 상태로 취소하지 않는다.

전투 시간의 수입·재소환 대기는 라운드 Update에서 진행한다. 입력과 표시를 위한 UIPlayer의 Money/Mana는 로스터 값으로 연결된다. 능력의 준비 상태와 실행·마나 결제는 PlayerAbilityController가 현재 라운드/플레이어와 연결해 처리하며, 타깃 선택 취소로 마나를 차감하지 않는다.

스테이지 프리팹의 `LunaStageDefinition`은 TableKey와 기존 DungeonTable 연결만 보관한다. 설정과 생성 규칙은 다음 테이블에서 읽는다.

| CSV | 런타임 책임 |
| --- | --- |
| `UnitBalance.csv` | 유닛 기본 스탯·성장·분류와 밸런스 값 |
| `SkillBalance.csv` | 액티브 스킬 14개·장비 능력 18개의 전투 설정 18필드: 마나·피해·범위·쿨다운·지속시간·확률·계수·소환 한도 등 |
| `EntityParameters.csv` | 프리팹별 수치 매개변수와 플레이어 능력 설정 |
| `Stages.csv` | 36개 스테이지 정의, 종류·폭·제한시간·배경·타워·보스·금화 |
| `StageWaves.csv` | 155개 시작·반복·무한 웨이브 행 |
| `StageRewards.csv` | 83개 승리·몬스터·무한 보상 행 |

이 CSV들은 native DataTable 행으로 파싱한다. 중복 ID·키, 잘못된 참조·확률·비정상 수치를 검사한다. `Tools/export_luna_stage_tables.py`는 기존 스테이지 설정에서 최초 CSV를 만든 이관 도구이며 런타임에서 예전 ScriptableObject의 생성 메서드를 호출하지 않는다. 남아 있는 BasicDungeonData/BossDungeonData 클래스는 이전 직렬화 자료를 읽기 위한 데이터 형태이고 Spawn/Drop 실행 메서드가 없다.

`SkillBalance`는 원본 GUID와 Resources 경로로 32개 스킬 자료에 연결하며 실제 native DataTable이 런타임 수치 원본이다. 기존 SkillData의 이미지·효과 프리팹·설명 참조는 유지하고, 플레이용 복제본의 전투 설정에 CSV 값을 적용한 뒤 저장된 `level`을 별도로 덮어쓴다. 장비 목록의 에셋 포인터도 같은 런타임 스킬 자료로 해석한 후 `PlayerLoadoutSnapshot`에 복사한다. 최초 이관은 기존 직렬화 값과 생략된 필드의 기존 C# 초기값을 그대로 사용했다. 초기 내보내기는 이미 존재하는 CSV를 덮어쓰지 않으며, 이후 소스 동기화는 기존 수치를 보존하고 새 에셋·이동 경로만 반영한다. SkillData 인스펙터의 CSV 소유 필드는 편집을 막아 수치 원본이 둘로 갈라지지 않게 했다. 이 추가 변경은 런타임/에디터 소스 컴파일을 통과했고, `Logs/NativeRewrite/skills-runtime03.json`에서 native 테이블의 32개 행·576개 필드와 실제 스킬 동작 검사가 통과했다. 최종 세 차례 검수와 배포 빌드 결과는 별도로 판정한다.

## UI와 저장

각 화면은 `UILuna : UIFormLogic`를 상속하고 native OnOpen/OnUpdate/OnClose를 구현한다. `LunaUI`는 26개 게임 키와 native UI 요청을 연결한다. 24개 원본 프리팹 GUID를 유지하며 기존 계정 상점/PVP 키는 원래와 같이 일반 팝업을 가리킨다. 이 두 키의 등록이 새 상점이나 PVP 기능 완성을 뜻하지 않는다.

Main/Hud/Popup native 그룹이 깊이와 화면 덮임을 관리한다. UILuna는 OnInit에서 authored 자식 계층을 기록하고, OnClose에서 하위 Entity 및 대기 프리뷰를 취소한 후 생성된 행과 runtime Button listener를 정리한다. inspector의 persistent listener는 유지한다. 다음 OnOpen에서 authored 활성 상태와 Transform/RectTransform 배치를 복원한다. 따라서 부족한 Gold 때문에 내려간 소환 카드 위치가 재개방마다 누적되지 않는다. 탭의 일시적 비활성화는 Entity Hide로 해석하지 않는다. 반복해서 여는 아이템·장비·인벤토리·추첨 화면의 Texture 기반 Sprite는 `LunaUIViews.CreateSprite` 캐시를 공유한다.

저장값은 native Setting을 사용하는 `LunaSaveSettingsHelper`의 `LunaSave.dat`에 기록한다. 임시 파일과 교체/백업을 사용하며, 잘못된 헤더·잘린 파일·뒤에 추가된 데이터는 정상 빈 저장으로 취급하지 않는다. 런타임 데이터와 진행 상태는 원본 authored ScriptableObject를 직접 변경하지 않도록 분리한다.

`LunaRuntimeData`는 복제한 ScriptableObject와 별도로 만든 `LunaDataTable`을 명시적으로 추적한다. `DontSave`만 지정한 채 참조를 버리지 않고 세션 초기화/저장 호스트 종료에서 소유 객체를 파괴한다. 플레이 중에는 그 프레임의 Procedure 정리와 저장이 끝날 수 있도록 지연 파괴를 사용한다. 원본 Resources 에셋은 이 소유 집합에 넣지 않는다. `SaveNow`는 플레이 모드 밖에서는 실행하지 않아 Editor 종료 때 authored 테이블을 진행 상태로 저장하지 않는다. 이 정리 구현은 아래 네이티브 셰이더 종료 경고와 별개의 C# 객체 수명 수정이다.

## 남아 있는 Unity 컴포넌트

native EntityLogic과 UIFormLogic 자체도 Unity 컴포넌트다. 데이터용 UnitView/StageDefinition, Spine·ParticleSystem·Canvas·물리·렌더링 컴포넌트, UI 버튼/포인터 이벤트, Bootstrap과 저장 생명주기 연결, 에디터 도구는 역할에 맞는 Unity 타입을 유지한다. 이는 이전 MonoBehaviour 업데이트를 호출하는 호환 런타임을 남긴다는 뜻이 아니다.

직렬화 GUID와 코드 참조가 없던 빈 네트워크 MonoBehaviour 아홉 개는 제거했다. `ChatManager`와 통신 자료형은 별도 미사용 채팅 프리팹에 남아 있다. 현재 게임 경로는 이를 로드하지 않고 자동 연결 기본값도 꺼져 있다. 외부 채팅 서버 연결·PVP·계정 상점은 이번 오프라인 PC 복구 검증으로 구현 또는 검증됐다고 주장하지 않는다.

## supplied Unity 코드의 제한된 수정

`Assets/link.xml`은 문자열로 선택하는 57개 엔티티 타입, 설정된 Procedure 5개·helper 19개, Core 모듈 구현 18개 및 reflection으로 생성되는 generic pool/table 구현 2개를 명시적으로 보존한다. CSV가 이름으로 참조하는 ObjectData/SkillData 필드 52개도 개별 지정한다. 에디터 검수는 실제 타입·필드 존재와 카탈로그/프리팹 설정의 보존 범위를 확인한다. 전체 게임 어셈블리를 보존하거나 stripping 설정을 끄지 않았으며, 이 구조 검사는 Android·IL2CPP 실행 검증을 대신하지 않는다.

Core DLL은 수정하지 않았다. Unity 소스의 변경은 domain reload 없는 재시작 정리, batch에서도 진행되는 Procedure 시작 대기, 실패한 자원 핸들 처리, 엄격한 저장 파싱, Unity 버전 API 대응, Entity 초기화 실패 관찰과 지연 recycle 안전성에 한정한다. 네이티브 모듈의 오류를 단순 경고 필터로 숨기지 않는다.

## 검증 진입점과 현재 증거의 범위

- 최초 에셋 설정 및 구조 검수: `LunaMigrationSetup.PrepareAndAudit`.
- 읽기 중심 구조 검수: `LunaValidation.Audit`.
- 현재 native 구조 런타임: `LunaValidation.Smoke` 및 `LunaValidation.ContentMatrix` (`LunaContentMatrix.Run`은 후자의 별칭).
- Windows 빌드: `LunaValidation.BuildWindows` / `BuildWindowsClean`.

CLI는 별도 프로세스와 격리 저장 경로를 사용한다. outer 결과는 `Logs/Revival/{operation}.json`, 실제 native runtime case 결과는 `native-runtime-smoke.json` 또는 `native-runtime-content-matrix.json`이다. 타임스탬프가 맞는 새 보고서만 판정에 사용한다. matrix는 소유권·등급·이동·버프·스킨·스킬·액터·발사체·보상·조합·던전·캠페인·UI·HUD 수명 검사를 연결하며, 알 수 없는 section이나 일치하는 사례가 없는 filter는 실패다. 정의한 사례 수가 곧 전체 동작의 통과를 뜻하지 않는다.

r04의 동일 소스 최종 검수3회가 완료됐다. 각 회차 Audit15/Smoke8/Matrix442 및 마지막 clean Windows 빌드·Player seed2/verify8·302.3273초11회 순환을 통과했다. 이전 개별 실패와 r01~r03 불인정/중단 근거는 [Revival-QA.md](Revival-QA.md)에 보존한다. 강한 미실행 인수 조건과 native 종료 할당은 통과 범위 밖의 남은 항목으로 구분한다.

검수 전에 현재 BuildTarget의 공개 scripting define이 supplied Game Framework의 Warning/Error 호출을 실제로 포함하는지도 검사한다. 해당 패키지는 조건부 로그를 사용하므로, 로그가 컴파일에서 제외된 상태의 “오류 0개”를 통과 근거로 인정하지 않는다.

의도적 소유권 실패 검사는 `LUNA_INJECTED_OWNERSHIP` 오류를 실제로 발생시킨다. 데이터 반환·대기 요청·native ID 정리가 모두 증명된 경우만 runtime 보고서가 정확한 오류 메시지를 expectedDiagnostics로 기록한다. Editor는 관찰한 메시지와 이 목록을 양방향 대조한다. 나머지 오류는 모두 실패다.

현재 재작성의 Windows A Editor 종료에서도 Shader CompileVariantJob 임시 할당5개/1,366바이트가 재현됐다. 16회 idle과 -nographics로 해결되지 않았다. 같은 A 실행 파일의 실제 graphical Player 두 프로세스에서는 JobTemp/ComputeBuffer 경고가 관측되지 않았다. 보고서 작성 뒤 native 종료 단계에서 출력된 메시지는 JSON의 0 warnings 수치만으로 제외하지 않는다.

현재 구현의 Windows 진단은 같은 소스에서 `Validate-Luna.ps1 -Mode BuildWindows -CleanBuildCache -JobLeakDiagnostics`를 `-NoGraphics` 유무로 각각 실행하고 완전한 종료 로그를 따로 보존하는 비교로 준비한다. 그 뒤 그래픽·오디오를 켠 숨김 Windows Player를 `Validate-Player.ps1 -Mode All -Section all -RenderMode offscreen -SoakSeconds 300`으로 실행한다. A의 빌드/새 Player와 graphics B 빌드 비교를 완료했다. 두 Editor 빌드 모두 같은 native JobTemp 경고가 남았다. 이 결과가 최종3회 검수를 대체하지 않는다. Editor 그래픽 장치 초기화를 생략한 빌드와 실제 Player의 그래픽 검증을 구분한다.

최종 세 차례 검수 전에 Addressables 준비 설정과 생성물 경로도 안정화해야 한다. `PrepareBuild`는 주소·그룹·Build Settings를 저장하는 설정 작업이며 읽기 전용 검사가 아니다. 특히 기본 ContentStateBuildPath는 추적 중인 `Assets/AddressableAssetsData/Windows/addressables_content_state.bin`을 덮어쓰므로, 새 빌드 상태 출력은 `Builds/Revival/AddressablesState/<BuildTarget>`로 옮긴 뒤 최종 소스 지문을 잡는다. 기존 추적 상태 파일은 이전 빌드 기록으로 보존한다. 이후 `Final-Review.ps1`은 같은 RunId의 세 회차에서 소스 지문이 같아야 통과한다.

Addressables의 패키지 생성 보존 파일은 직접 작성한 `Assets/link.xml`과 구분한다. 설치 소스 `Library/PackageCache/com.unity.addressables@42676f2a154e/Editor/Build/AddressablesPlayerBuildProcessor.cs:35–49`는 Editor 초기화 때 설정 폴더의 `link.xml`과 meta를 삭제하고, 같은 파일의118–124행은 빌드 출력의 XML을 다시 복사/임포트한다. `Build/DataBuilders/BuildScriptPackedMode.cs:545–572`는 provider와 번들 직렬화 타입에 대한 XML을 생성한다. 따라서 정확히 `Assets/AddressableAssetsData/link.xml`과 `.meta`만 생성물로 별도 기록하는 fingerprint v2를 게시했으며, 직접 작성한 `Assets/link.xml` 및 다른 모든 소스/설정/에셋은 계속 해시에 포함한다. B raw 전후 지문은 동일했고 v2 도구가 게시됐다. 알고리즘/해시를 함께 비교하는 새 최종 RunId를 시작했으며 그 실행 완료와 native 경고 해결은 아직 별개의 미완료 판정이다.

설정에 남은 `templateDefaultScene: Assets/Scenes/SampleScene.unity`도 감사한다. 기본 템플릿에서 이전 Game 고아 인스턴스와 그 stripped Canvas만 제거하고 ShaderTest·Square·카메라·조명과 씬 GUID를 유지했다. `LunaValidation.Audit`는 운영4씬 수와 템플릿1개 수를 별도 기록하며 같은 hierarchy/YAML 검사를 적용한다. 템플릿을 운영 Build/Addressables 목록으로 승격하지 않는다. 이 변경은 Runtime/Editor 소스 컴파일과 실제 Unity Audit08(운영4씬+템플릿1개,15검사,오류/경고0)을 통과했다. r03의 d62dce4b…/22,366개 실행은1회차 저장 경고 때문에 불인정/중단했다. 이 지문을 수정 후 새 최종 기준선으로 혼용하지 않는다.


최종 r04 3회차의 clean headless 빌드 C도 BuildReport0/0으로 성공했으나, 닫힌 Editor 로그에서 같은 shader-cache native 할당5개/1,366바이트가 재현됐다. A/B 이후 해결된 것으로 취급하지 않는다. C의 정확한 주소/스택·별도 엔진 메모리 집계와 startup/cache/종료 진단은 [Revival-QA.md](Revival-QA.md)에 기록했다. 최종 Player와 지문 확인도 완료돼 r04는3/3회다. Player 종료의 별도 UTP native 집계41,463/45,302바이트는 귀속 미확인으로 남는다.
