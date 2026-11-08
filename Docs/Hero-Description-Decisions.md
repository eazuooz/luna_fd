# 빈 영웅 설명의 확정 문구

네이티브 전환의 실제 구현에 맞춘 표시 문구다. 숫자나 신규 능력을 추가하지 않는다. `Resources/ScriptableObjects/Heroes` 아래 원본 SO의 해당 문자열 필드에 적용하며, CSV의 전투 숫자를 다시 SO에서 가져오라는 지시가 아니다. 기존에 비어 있던 Pobi/Lennon/Tira의 기본·중간 등급 설명도 함께 제시해 UI에 빈 효과를 남기지 않는다.

| 리소스 경로 | 필드 | 표시할 문구 |
| --- | --- | --- |
| `ScriptableObjects/Heroes/Tira` | `skillStr` | 10초마다 플레이어를 5초 동안 거대화합니다. 크기가 기본의 1.5배가 됩니다. |
| `ScriptableObjects/Heroes/Tira` | `start3` | 기존 거대화 효과를 유지합니다. |
| `ScriptableObjects/Heroes/Tira` | `start5` | 20초마다 현재 공격 대상을 4초 동안 축소합니다. 크기가 기본의 65%가 됩니다. |
| `ScriptableObjects/Heroes/Tira` | `ultra1` | 거대화 대상을 아군 근거리 유닛 전체로 확대합니다. 타워는 제외합니다. |
| `ScriptableObjects/Heroes/Tira` | `ultra2` | 기존 거대화·축소 효과를 유지합니다. |
| `ScriptableObjects/Heroes/Pobi` | `skillStr` | 5초마다 소환수 3마리를 소환합니다. |
| `ScriptableObjects/Heroes/Pobi` | `start3` | 공격력이 5% 증가합니다. |
| `ScriptableObjects/Heroes/Pobi` | `start5` | 자신이 소환하는 소환수의 체력·공격력·방어력이 10% 증가합니다. |
| `ScriptableObjects/Heroes/Pobi` | `ultra1` | 75% 확률로 소환수를 1마리 추가하여 총 4마리를 소환합니다. |
| `ScriptableObjects/Heroes/Pobi` | `ultra2` | 기존 소환 강화와 추가 소환 확률을 유지합니다. |
| `ScriptableObjects/Heroes/Lennon` | `skillStr` | 20초마다 소환수 3마리를 소환합니다. |
| `ScriptableObjects/Heroes/Lennon` | `start3` | 공격력이 5% 증가합니다. |
| `ScriptableObjects/Heroes/Lennon` | `start5` | 자신이 소환하는 소환수의 체력·공격력·방어력이 10% 증가합니다. |
| `ScriptableObjects/Heroes/Lennon` | `ultra1` | 75% 확률로 소환수를 1마리 추가하여 총 4마리를 소환합니다. |
| `ScriptableObjects/Heroes/Lennon` | `ultra2` | 기존 소환 강화와 추가 소환 확률을 유지합니다. |
| `ScriptableObjects/Heroes/Dren` | `ultra2` | 기존 스킬 강화와 파티 피해 감소 효과를 유지합니다. |
| `ScriptableObjects/Heroes/Pochi` | `ultra1` | 기존 5초 보호막과 30% 피해 반사 효과를 유지합니다. |
| `ScriptableObjects/Heroes/Pochi` | `ultra2` | 기존 5초 보호막과 30% 피해 반사 효과를 유지합니다. |

“기존 효과 유지”는 미구현 예정 효과를 숨기거나 상위 등급의 추가 수치가 있는 것처럼 표현하지 않는다는 뜻이다. STAR2/STAR4 역시 별도 능력을 만들지 않고 직전 설명의 효과를 상속한다.
