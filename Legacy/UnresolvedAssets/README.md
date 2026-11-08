# 원본이 누락된 미사용 프로토타입 보관

2026-11-08 복구 감사에서 발견했다. 아래 파일은 원본 `.meta`와 함께 보존하며 Unity의 `Assets` 폴더 밖에 둔다. 현재 30개 스테이지, 4개 빌드 씬과 게임 코드에는 참조가 없음을 GUID 및 이름으로 확인했다. 사용 중인 콘텐츠의 검사를 건너뛰기 위한 제외가 아니다.

| 원래 경로 | 누락 원본 및 판단 |
|---|---|
| `Assets/Resources/Prefabs/08StageBackground/Background_Grassfield.prefab` | 현재 스테이지/씬에서 사용하지 않는 이전 배경. 아래 두 나무 프리팹을 24곳에 배치한다. |
| `Assets/Resources/Prefabs/08StageBackground/Object/Tree_G.prefab` | Spine Material GUID `a9083aa6726964d43a67fec01c861c25`, SkeletonData GUID `1d02c4d99f94e404899adad38c32a3aa`가 없다. |
| `Assets/Resources/Prefabs/08StageBackground/Object/Tree_P.prefab` | Spine Material GUID `3320b18945cbf0f4cb9fc6aa9bd4e9cf`, SkeletonData GUID `ff56f9bf3ded8a54ab2ec4d87323040f`가 없다. |
| `Assets/Resources/UIReference/UIAdventure.asset` | 대상 프리팹 GUID `bc9e18131df32f145b568b0b267d50f2`가 없다. 이 참조 에셋의 GUID `8323e71f5767b8b4aa54ab7ae7981bcb`를 사용하는 다른 에셋/코드도 없다. |

누락 원본은 현재 저장소뿐 아니라 최초 소스 커밋 `582ae4c3`에도 포함되지 않았다. 이름만 비슷한 캐릭터나 임의 이미지를 연결하지 않았다. 원본 나무 Spine 내보내기 자료와 Adventure 프리팹을 확보한 뒤 다시 연결해야 원래 모습을 복원할 수 있다. 현재 감사 통과는 이 네 미사용 원본이 복원되었다는 의미가 아니다.

별도로 여섯 스테이지에 남은 삭제된 선택형 DungeonTable 참조를 정리했다. 누락 GUID는 `15306cd38cfc7e64eb786ec916bcb0cc`, `3bee5010d0834a147ab6d71268307a5f`, `6cd89443a29411b4f8fcf9cd39f71c7e`이다. 현재 실제로 작성된 해금 테이블 `Stage01-10`(Dungeon01), `Stage02-05`(Dungeon02)는 그대로 유지한다. 삭제된 테이블의 내용을 추측하여 새로운 해금 조건을 만들지 않았다.
