# Game Framework (YamYamCoding Edition)

Unity용 모듈형 게임 프레임워크. Procedure / FSM, Entity, UI, Event, ObjectPool, ReferencePool, Sound, Scene, Setting, DataTable, Localization, Network 등을 제공한다.

[Game Framework](https://github.com/EllanJiang/GameFramework) 와 [Unity Game Framework](https://github.com/EllanJiang/UnityGameFramework) (Jiang Yin, MIT) 를 기반으로 하며, 이 에디션은 다음이 다르다.

- 리소스 파이프라인을 **Addressables** 로 교체했다.
- Unity 6 에서 동작한다.

## 패키지

세 개로 나뉜다. 필요한 것 하나만 받으면 된다. 아래 것은 위의 것을 전부 포함한다.

| 파일 | 내용 | 넣는 곳 |
|---|---|---|
| `GameFramework-Core-<버전>.unitypackage` | 코어 DLL(`Assets/Plugins/GameFramework.dll`)과 라이선스 문서. Unity 를 모르는 순수 C# 라이브러리만 쓰고 싶을 때 | 아무 프로젝트 |
| `GameFramework-Unity-<버전>.unitypackage` | 코어 DLL + Unity Game Framework(`Assets/Scripts`) + `GameFramework.prefab` + 시작 예제(`Assets/Scripts/Starter`). **자기 게임을 이 프레임워크로 만들 때** | 아무 프로젝트 |
| `GameFramework-Example-<버전>.unitypackage` | 위 전부 + 예제 게임(ToyboxNightmare)의 코드·에셋·씬과 프로젝트 설정. **튜토리얼을 따라 공부할 때** | **빈 프로젝트**(프로젝트 설정을 덮어쓴다) |

경로는 전부 같다(`Assets/…`). 같은 파일은 같은 GUID 라서, 위의 패키지를 넣은 뒤 아래 패키지를 넣어도 겹치지 않고 그대로 덮인다.

## 구성

세 층이다. 코어는 Unity 와 무관한 별도 .NET 어셈블리이고, Unity Game Framework 가 그것을 Unity 에 연결하며, 게임은 그 둘을 쓴다.

| 경로 | 내용 |
|---|---|
| `Assets/Plugins/GameFramework.dll` | **Game Framework (코어).** 엔진에 의존하지 않는다. 별도 C# 프로젝트(netstandard2.1)를 빌드한 결과물 |
| `Assets/Scripts/` | **Unity Game Framework.** `~Component`, Helper, `EntityLogic`, `UIFormLogic` |
| `Assets/Scripts/Editor/` | 커스텀 인스펙터와 `Game Framework` 메뉴 |
| `Assets/Scripts/Starter/` | 시작 예제. 시작 절차 `ProcedureStarter` 와 그것으로 시작하는 `GameFrameworkStarter` 씬 |
| `Assets/Prefabs/GameFramework.prefab` | 프레임워크 컴포넌트가 모두 붙은 프리팹. 씬에 하나만 둔다. 기본 진입 절차는 `ProcedureStarter` 다 |

## 요구 사항

| 항목 | 값 |
|---|---|
| Unity | **6000.6 이상.** 6000.6 에서 만들고 검증했다. 그보다 낮은 버전(예: 6000.3 LTS)에서 열면 URP 설정 파일 때문에 `Missing types referenced from component UniversalRenderPipelineGlobalSettings …` 경고가 난다 |
| Addressables | `com.unity.addressables` — `.unitypackage` 임포트 시 자동 설치 |
| SharpZipLib | `com.unity.sharp-zip-lib` — `.unitypackage` 임포트 시 자동 설치 |

## 시작하기 (GameFramework-Unity 패키지)

1. `GameFramework-Unity-<버전>.unitypackage` 를 임포트한다. Package Manager 의존성 설치를 묻으면 수락한다.
2. **`Assets/Scripts/Starter/GameFrameworkStarter.unity` 를 열고 Play** 를 누른다. 콘솔에 `Game Framework is running (ProcedureStarter).` 가 찍히면 프레임워크가 돌고 있는 것이다.
3. 자기 게임을 시작한다.
   - 자기 씬에 `Assets/Prefabs/GameFramework.prefab` 을 놓는다(시작 씬을 복사해 써도 된다). 프리팹은 씬에 하나만 둔다.
   - `GameFramework.Procedure.ProcedureBase` 를 상속한 절차(Procedure)를 만든다. 오버라이드는 `protected override` 다(코어 DLL 의 클래스라서).
   - Hierarchy 에서 `GameFramework → Procedure` 를 고르고, 인스펙터에서 만든 절차에 체크한 뒤 진입 절차(Entrance Procedure)로 고른다. `ProcedureStarter` 는 체크를 푼다.
   - 이 설정은 **씬의 인스턴스에** 둔다(씬을 저장한다). 프리팹에 Apply 하지 않는다 — 프리팹은 어느 씬에서나 쓰는 공통 설정이고, 게임의 절차는 게임 씬의 것이다. 예제 게임도 이렇게 MainScene 에 둔다.
4. 메뉴 **Game Framework → Log Scripting Define Symbols → Enable All Logs** 로 프레임워크 로그를 켠다.
   켜지 않으면 `Log.Info` / `Log.Error` 가 컴파일에서 빠져 아무것도 찍히지 않는다.

## 예제 게임 실행하기 (GameFramework-Example 패키지)

예제 패키지는 **빈 프로젝트**에 임포트한다(프로젝트 설정을 덮어쓴다). 임포트한 뒤에는:

1. **`Assets/Scenes/MainScene.unity` 를 열고** Play 를 누른다.
   새 씬(예: 템플릿의 `SampleScene`)에 `GameFramework.prefab` 만 놓고 실행하면 게임이 시작되지 않는다. 게임의 절차 설정(`ToyBoxNightmare.ProcedureMain`)과 방(Environment), NavMesh, 카메라 추종(`PlayerCameraFollow`), EventSystem 이 전부 MainScene 에 놓여 있기 때문이다. 그 경우 프리팹의 기본 절차인 `ProcedureStarter` 가 돌며 콘솔에 `This project has other procedures: ToyBoxNightmare.ProcedureGameOver, ToyBoxNightmare.ProcedureMain …` 가 찍힌다. 다른 씬에서 게임 절차를 직접 골라 실행하면 적이 움직이지 않고(`Failed to create agent because there is no valid NavMesh`) 카메라가 따라오지 않으며, 콘솔에 `이 씬에는 예제 게임에 필요한 것이 없다` 에러가 찍힌다.
2. 캐릭터(Girl / Boy)를 클릭해 고른다. WASD 이동, 마우스 조준, 왼쪽 버튼 발사, Tab 무기 전환, 1 아군 소환, Esc 일시정지, 죽은 뒤 R 재시작.
3. 캐릭터가 하나도 안 나오면 Addressables Groups 창(Window → Asset Management → Addressables → Groups)의 **Play Mode Script** 를 `Use Asset Database (fastest)` 로 둔다. 이 설정은 프로젝트마다 따로 저장돼 패키지로 전달되지 않는다.

## 코어 소스 받기 (선택)

코어는 DLL 로 들어 있어서 그대로 써도 된다. 코어를 디버깅하거나 고치고 싶을 때만 소스를 받는다.

1. 메뉴 **Game Framework → Download Core Source** 를 고르고 확인 창에서 **Download** 를 누른다.
   `https://github.com/eazuooz/GameFramework` 를 `<프로젝트>/External/GameFramework` 에 받는다. git 이 있으면 클론하고, 없거나 실패하면 GitHub 의 ZIP 아카이브를 받아 푼다(인터넷이 필요하다. ZIP 으로 받으면 git 기록은 없다).
   **`Assets` 밖이라 Project 창에는 보이지 않는다.** 완료 창의 **Show Folder** 로 연다. (Assets 안에 두면 DLL 과 같은 이름의 어셈블리가 둘이 되어 컴파일이 깨진다.)
2. `External/GameFramework/GameFramework.sln` 을 열어 빌드한다. 빌드 후 이벤트가 `GameFramework.dll` 을 `Assets/Plugins` 로 복사하고, Unity 가 새 DLL 을 읽는다.

이미 받아 둔 소스가 있으면 다시 받지 않고 그 위치를 알려 준다(git 으로 받은 것이면 **Update (git pull)** 로 갱신할 수 있다). 메뉴를 고르기 전에는 아무것도 받지 않는다. 빌드 후 복사는 Windows 의 XCOPY 를 쓴다.

## 게임 코드에서 지킬 것

- `EntityLogic` / `UIFormLogic` 의 훅은 **`protected internal override`** 로 오버라이드한다(프레임워크와 게임 코드가 같은 어셈블리다).
- 엔티티 프리팹에는 `EntityLogic` 을 붙이지 않는다(런타임에 붙는다). UI 폼 프리팹에는 `UIFormLogic` 을 **반드시** 붙인다.
- 엔티티는 풀에서 재사용된다. Unity 의 `Awake` / `Start` 대신 `OnInit` / `OnShow` / `OnHide` 를 쓴다.
- 프리팹의 Addressables 주소는 `ShowEntity` / `OpenUIForm` 에 넘기는 문자열과 정확히 같아야 한다.

## 라이선스

MIT. `LICENSE.md` 와 `Third Party Notices.md` 를 볼 것.
원저작자 표기(각 소스 파일 헤더 포함)는 재배포 조건이므로 지우면 안 된다.
