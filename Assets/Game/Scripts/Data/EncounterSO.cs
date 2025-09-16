// Assets/Game/Scripts/Data/EncounterSO.cs (수정 또는 파일 생성)
using UnityEngine;

[CreateAssetMenu(fileName = "Encounter_", menuName = "Game/Data/Encounter")]
public class EncounterSO : ScriptableObject
{
    public string encounterId;

    // [수정] 적 프리팹 배열 대신, 적 UnitSO 배열을 사용합니다.
    // 변수 이름은 BattleManager에서 사용하는 'enemyUnits'로 맞춰주는 것이 좋습니다.
    public UnitSO[] enemyUnits;

    // (선택) 전투 배경음악이나 보상 등 추가 정보
    // public AudioClip battleMusic;
    // public RewardTableSO rewardTable;
}