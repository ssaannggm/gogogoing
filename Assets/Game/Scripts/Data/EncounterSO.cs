// Assets/Game/Scripts/Data/EncounterSO.cs (���� �Ǵ� ���� ����)
using UnityEngine;

[CreateAssetMenu(fileName = "Encounter_", menuName = "Game/Data/Encounter")]
public class EncounterSO : ScriptableObject
{
    public string encounterId;

    // [����] �� ������ �迭 ���, �� UnitSO �迭�� ����մϴ�.
    // ���� �̸��� BattleManager���� ����ϴ� 'enemyUnits'�� �����ִ� ���� �����ϴ�.
    public UnitSO[] enemyUnits;

    // (����) ���� ��������̳� ���� �� �߰� ����
    // public AudioClip battleMusic;
    // public RewardTableSO rewardTable;
}