# [Unity3D] 루나 디펜스

## 2026 복구 및 Toybox 방식 Game Framework 재작성

`ProjectSettings/ProjectVersion.txt`의 **Unity 6000.6.0f1**으로 엽니다. Play 시작 씬은 `Loading`이며 시작 화면을 클릭하면 로비로 이동합니다. CLI 검사 전에는 같은 프로젝트의 Unity 에디터를 닫습니다.

제공된 GameFramework Core/Unity 0.1.0을 사용하고, `ToyboxNightmare`의 실제 작성 방식에 맞춰 액터별 `EntityLogic`·typed `EntityData`·native FSM, Procedure가 소유하는 일반 C# 판 객체, native `UIFormLogic`으로 재작성했습니다. Everyday 패키지와 12개 관리자, MonoBehaviour 수명 중계는 제거했습니다. 소환 경제·쿨다운·전리품·재생성은 판과 플레이어가 소유합니다.

전투 설정은 유닛76행, 스킬/장비32행·18필드, Entity 매개변수260행과 스테이지/웨이브/보상 CSV를 native DataTable로 읽습니다. 저장된 성장·선택 정보와 아트 에셋 참조는 분리합니다. 아래 원래 소개·영상·이미지는 개발 당시 기록으로 유지하며, 복구 후 확정한 스킬 성장 규칙은 별도 문서를 기준으로 합니다.

- [Toybox 작성 방식과 native 수명 구조](Docs/Framework-Migration.md)
- [인수 기준과 실제 검증 근거](Docs/Toybox-Authoring-Acceptance.md)
- [CSV / DataTable 편집 및 저장 데이터 우선순위](Docs/UnitBalance-DataTable.md)
- [승인된 플레이어 스킬 LV5·10·15·20 복구 규칙](Docs/Player-Skill-Level-Rules.md)
- [현재 검수 상태와 재실행 방법](Docs/Revival-QA.md)
- [역사적 경고 수정 내역과 현재 엔진 종료 진단](Docs/Warning-Review.md)
- [메모리 종료 수정·엔진 비교·빌드 우회책의 실제 근거](Docs/Memory-Investigation.md)
- [원본 설명과 구현의 남은 게임 규칙 검토](Docs/Gameplay-Open-Issues.md)

**native 재작성 단계의 PC 검수는 r04에서3/3회 통과했고, 이후 메모리·초기 종료 수정도 추가 재검수를 마쳤습니다.** 최신 v2는 gated clean 빌드·Start 전 종료·Windows seed3/verify9·302.383초11회 순환·전체 Matrix442를 통과했고 전후 소스 지문도 일치했습니다. 직전 Audit15·Smoke8와 검사기 한 조건의 보정 경위는 QA 문서에 구분해 기록했습니다. 이번 추가 회귀를 이전3회와 합쳐 새 변경의3회 검수라고 표시하지 않습니다. 실행 파일은 `Builds/Revival/Windows/LunaDefense.exe`입니다.

**게임이 소유한 런타임 SO/Sprite·native Entity/FSM·리소스 handle과 대기 요청은 종료 시 실제0으로 정리됩니다.** Editor의 warm shader-cache 경로는5개/1,366바이트가 재발하며6000.6.3f1/6000.6.8f1에서도 같아 엔진을 바꾸지 않았습니다. 명시적 캐시 백업·재생성 빌드 우회책과 완전 종료 로그 게이트는 실제 거부/통과 사례로 검증했습니다. 최신 Player의 귀속 미확인 UTP 집계40,122/43,962바이트는 보존하며 모든 엔진 할당이 해제됐다고 주장하지 않습니다. 중간 R02의 짧은 검사와 최종 v2의 새302초 검사는 QA 문서에서 별도로 구분합니다. Android/iOS·IL2CPP와 모든 확률/조합의 완전 검증은 포함하지 않습니다.

```powershell
# 화면 없는 Editor: 임포트/참조 감사 + Smoke + 전체 콘텐츠 + clean Windows 빌드
& .\Tools\validation\Validate-Luna.ps1 -Mode All -NoGraphics -CleanBuildCache -JobLeakDiagnostics -RebuildShaderCache -CheckNativeMemory
# 특정 전체 콘텐츠 검사만 다시 실행
& .\Tools\validation\Validate-Luna.ps1 -Mode ContentMatrix -MatrixSection all -NoGraphics
# 초기화 시작 전 종료·저장 보존·반복 종료 검사
& .\Tools\validation\Validate-Player.ps1 -Mode Early -CheckNativeMemory
# 실제 그래픽/오디오를 켠 숨김 Windows 프로세스: 별도 저장/입력/DSP/3해상도/300초
& .\Tools\validation\Validate-Player.ps1 -Mode All -Section all -RenderMode offscreen -SoakSeconds 300 -CheckNativeMemory
# 소스가 고정된 뒤 새 RunId로 독립 Editor 검수3회; 3회차에 새 빌드와 Player 검사 포함
& .\Tools\validation\Final-Review.ps1 -NoGraphics -CleanBuildCache -JobLeakDiagnostics -RebuildShaderCache -CheckNativeMemory
```

`-NoGraphics`는 Editor에만 적용하며 실제 Player의 그래픽·오디오는 켭니다. `-RebuildShaderCache`는 정확한 생성 캐시만 복구 가능한 고유 백업으로 옮기는 opt-in 우회책이고, `-CheckNativeMemory`는 완전 종료 로그의 명시적 native 오류를 거부하면서 UTP 원문은 별도 보존합니다. BuildReport의 경고0만으로 종료 검사를 대체하지 않습니다. Windows 실행 파일은 `Builds/Revival/Windows/LunaDefense.exe`, 격리 검사 보고서는 `Logs/Revival/`, 최종 회차별 원본은 `Logs/Revival/FinalReview/`에 생성됩니다. 최종 검사 도구는 소스 지문과 알고리즘 버전을 비교하며, 중간 소스 변경 시 새 RunId로 다시 시작합니다. 검사는 실제 사용자 저장 대신 격리 저장을 사용합니다. Android/IL2CPP·모바일 기기와 상용 배포 밸런스는 검증 완료 범위가 아닙니다.


🖥️ 프로젝트 소개

아트 + 프로그래머 2인개발로 루나 디펜스 게임을 개발하였습니다.

🕰️ 개발 기간

1년

⚙️ 개발 환경

Unity3D, C#

# *동영상*
[동영상](https://youtu.be/8lP2S5Z1PkA?feature=shared) 

# *게임 주요 사진*
<img src="https://github.com/eazuooz/luna_fd/assets/46310910/32fc3eba-6c7a-4b34-9dcf-528155f266bc" width="40%"/>
<img src="https://github.com/eazuooz/luna_fd/assets/46310910/6cba22ce-6ca9-4612-98c5-369fb95933f8" width="40%"/>
<img src="https://github.com/eazuooz/luna_fd/assets/46310910/5082d14c-2fc2-4eef-b7cd-5a63d57eff08" width="40%"/>
<img src="https://github.com/eazuooz/luna_fd/assets/46310910/30e92005-d5c3-4efa-80af-d23b73c2edfe" width="40%"/>
<img src="https://github.com/eazuooz/luna_fd/assets/46310910/972b8c73-1cea-4179-9b64-9353e55fa65e" width="40%"/>
<img src="https://github.com/eazuooz/luna_fd/assets/46310910/a2120d81-60ad-4cc1-9cdf-63657cb435c0" width="40%"/>
<img src="https://github.com/eazuooz/luna_fd/assets/46310910/ef106e53-1c51-476d-a082-69c5f4b56272" width="40%"/>
<img src="https://github.com/eazuooz/luna_fd/assets/46310910/5bb523f5-9abb-4f1f-a546-d67be02cf67f" width="40%"/>
<img src="https://github.com/eazuooz/luna_fd/assets/46310910/cd22332c-82e1-4a20-ba28-da677cb69114" width="40%"/>
<img src="https://github.com/eazuooz/luna_fd/assets/46310910/5c644794-b669-42e4-95a0-380e83352007" width="40%"/>

## 1. 플레이어(루나)

전투시에는 좌우 방향키로 이동 가능 공격은 자동공격이다. 마나가 차게되면 우측 스킬 3개를 사용해 스킬이 사용 가능하며

스킬은 스킬 장비셋에서 여러가지 스킬이 선택가능하다. 캐릭터가 쓰는 거의 모든 스킬을 떄에따라서 장착하여 사용 가능하다.

장비창에서 여러가지 갑옷을 착용 가능하며 갑옷마다 특별한 기능들이 있다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/6cba22ce-6ca9-4612-98c5-369fb95933f8" width="40%"/>
<img src="https://github.com/eazuooz/luna_fd/assets/46310910/e86fc698-853e-4c6a-9940-e4bd470e8440" width="40%"/>
<img src="https://github.com/eazuooz/luna_fd/assets/46310910/4a473faa-d3d8-4a7b-995a-8a727289919e" width="40%"/>
<img src="https://github.com/eazuooz/luna_fd/assets/46310910/afda2437-c5b6-417d-9288-49cfaf63713c" width="40%"/>
<img src="https://github.com/eazuooz/luna_fd/assets/46310910/1a523be3-e11d-4f70-a457-a259fec97d37" width="40%"/>
<img src="https://github.com/eazuooz/luna_fd/assets/46310910/593ff221-0e07-40e2-8536-588f3eec4de6" width="40%"/>

### 장비

#### 렌느의 갑옷
루나가 원거리 공격이 가능해집니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/deed5fd2-946b-4cc3-87db-afc0ccc40e61" width="40%"/>

#### 드래곤의 갑옷
번개와 클로를 내리치는 특수공격을 하게 됩니다. 시간이 지나면 드래곤이 따라다니며 공격을 합니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/c9b273af-b039-4121-97d6-6cd6165f0a84" width="40%"/>

#### 렌느의 갑옷
루나가 원거리 공격이 가능해집니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/deed5fd2-946b-4cc3-87db-afc0ccc40e61" width="40%"/>

#### 숲의 기사의 갑옷
일정시간마다 유닛들을 회복시킵니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/e86fc698-853e-4c6a-9940-e4bd470e8440" width="40%"/>

#### 슬라임의 갑옷
거대 슬라임과 일반 슬라임을 소환하게 됩니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/1a523be3-e11d-4f70-a457-a259fec97d37" width="40%"/>
<img src="https://github.com/eazuooz/luna_fd/assets/46310910/afda2437-c5b6-417d-9288-49cfaf63713c" width="40%"/>

#### 거대 해골 갑옷
스턴 공격과 범위 공격이 가능한 갑옷입니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/d776b489-aecf-4a41-be93-088e5ab3402f" width="40%"/>




### 스킬
기본적으로 영웅들이 사용할 수 있는 모든 스킬을 장착하여 사용이 가능하며 업그레이드가 가능합니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/4a473faa-d3d8-4a7b-995a-8a727289919e" width="40%"/>


## 2. 영웅 캐릭터

### 골디
골디가 전투시 배치 되었을때 골드획득량이 증가합니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/3df35240-7c67-42ce-9933-9c2a3f710077" width="40%"/>

### 티라
티라는 아군 유닛들을 거대화 시킵니다. 거대화가 되면 최대체력과 현제 체력이 증가합니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/c93da428-bfae-4311-a6df-55235d2154c3" width="40%"/>

### 드렌
마법구를 날려 폭발시킵니다. 일정 범위에 있는 적군들이 타격을 입습니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/a4e3e7b5-795f-4977-8128-529e9e310004" width="40%"/>


### 로엔
뒸쪽에 있는 아군 유닛들을 적군에게 가까운쪽으로 순간이동시켜 이동거리를 단축 시킵니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/bd70c3d1-31f4-4a0a-bfcf-508f89a690d4" width="40%"/>

### 조
철퇴를 내려찍어 주위 적군들에게 데미지를 입힙니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/8aad6695-1bbc-4e68-9b27-7f59827466aa" width="40%"/>

###클레어
적군 쪽으로 순간이동 하여 빠르게 전투에 합류합니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/2d3bc3a0-838e-4d83-acb7-9af4752fd297" width="40%"/>

###로즈
아군 유닛들에게 공격력 증가 및 이동 속도 증가 버프를 줍니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/9ca32a16-dc71-4331-838d-b688ffdd89a6" width="40%"/>

### 리아
해골 병사들과 해골 왕을 소환합니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/aaf6cccb-21db-443e-b5bd-aab41d06c2dd" width="40%"/>

### 메이
일직선상에 적에게 피해를 입히는 얼음을 날리는 스킬을 사용합니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/5c94e5f6-e416-4daa-96bf-4d982c947624" width="40%"/>

### 마틸다
자폭이 가능한 마틴, 미니마틴을 소환합니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/6f2fd1db-d279-4acf-8c80-54a755837974" width="40%"/>

### 렌느
아군 유닛들에게 공격속도 이동속도 증가 버프를 걸어 줍니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/bfbc3617-84bf-47bc-9563-a0de64231e13" width="40%"/>

### 포치
아군 유닛(영웅 한정)에게 일정 시간동안 무적이 되는 방어막을 걸어줍니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/67da58f5-039d-470b-a0e5-8c08aa9ccf4c" width="40%"/>

### 앨리스
일정 범위 안에 유닛들에게 데미지를 주는 스킬을 발동합니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/38a873f1-67dd-4f29-8ad3-01a53a6de5b9" width="40%"/>

### 라쿤
공격 당하는 적군을 뒤로 밀어주는 넉백 스킬을 사용합니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/f3714cce-dc86-4e1d-9d2c-ef1ff485be5e" width="40%"/>

### 미우, 프링, 로니
아군 유닛들을 회복시키는 마법을 발동합니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/fcd4f1c0-2bd4-4145-aa95-48ce7812e80f" width="40%"/>
<img src="https://github.com/eazuooz/luna_fd/assets/46310910/fa86ad0b-5938-4485-851c-72be6efd4b0c" width="40%"/>
<img src="https://github.com/eazuooz/luna_fd/assets/46310910/a70f6272-f2d1-4563-b63a-3b8c9e5299a8" width="40%"/>

### 레나
일반 슬라임, 킹슬라임을 소환하여 전투에 도움을 줍니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/ba8fa0da-8797-43f5-83a2-c622ee7a2aac" width="40%"/>

### 레논
검의 정령을 소환 합니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/f29127b3-6f72-4302-a930-0f89af6b681e" width="40%"/>

### 포비
분신을 소환 하여 공격합니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/0acf0a3c-45ca-49ee-bb1d-82e3e1d74427" width="40%"/>

### 엘라나 
방어력이 뛰어나고 공격기능이 없는 임프 전사를 소환합니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/94b1232d-c838-45d9-90b7-25b8aee9f8f5" width="40%"/>


## 3. 상점
아이템과 영웅을 뽑아 능력치를 업그레이드 시킬수 있습니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/2f8651ef-8dc5-4afb-9e92-7aae62e7f54c" width="40%"/>
<img src="https://github.com/eazuooz/luna_fd/assets/46310910/c75174a7-50be-49c1-bb36-8eb0c0a8d899" width="40%"/>

## 4. 던전

### 무한의 던전
몬스터가 끊임없이 리젠되며 최대한 많은 횟수를 클리어하면 됩니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/2505c075-5025-40e7-b0ff-5161d9dc8ee2" width="40%"/>

### 드래곤 레이드
강력한 용이 출현하며 클리어 할시 특별한 아이템들을 떨굽니다.

<img src="https://github.com/eazuooz/luna_fd/assets/46310910/f6b321c2-25de-4e92-a5cb-46fab8962ff5" width="40%"/>

# *기술적 요소*
자세한 소스코드 설명은 깃허브를 통해서 확인이 가능합니다.

URP (라이팅은 유니버셜 렌더 파이프라인을 활용 하였습니다.)

ui (everyday frame work를 사용 하였습니다.)


