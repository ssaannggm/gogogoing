// Assets/Game/Scripts/Data/EncounterSO.cs (수정 또는 파일 생성)
using UnityEngine;
using System.Collections.Generic;
// using Game.Items; // 네임스페이스가 있다면 유지하세요.

[CreateAssetMenu(fileName = "Encounter_", menuName = "Game/Data/Encounter")]
public class EncounterSO : ScriptableObject
{
    public string encounterId;

    // [수정] 적 프리팹 배열 대신, 적 UnitSO 배열을 사용합니다.
    // 변수 이름은 BattleManager에서 사용하는 'enemyUnits'로 맞춰주는 것이 좋습니다.
    public UnitSO[] enemyUnits;

    // ▼▼▼ 이 부분을 추가해주세요 ▼▼▼
    [Header("전투 보상")]
    [Tooltip("전투 승리 시 플레이어에게 제시할 보상 선택지 목록")]
    public List<RewardTableSO> rewardChoices;

    // (선택) 전투 배경음악
    // public AudioClip battleMusic;
}