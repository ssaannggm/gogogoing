// Assets/Game/Scripts/Data/EncounterSO.cs (���� �Ǵ� ���� ����)
using UnityEngine;
using System.Collections.Generic;
// using Game.Items; // ���ӽ����̽��� �ִٸ� �����ϼ���.

[CreateAssetMenu(fileName = "Encounter_", menuName = "Game/Data/Encounter")]
public class EncounterSO : ScriptableObject
{
    public string encounterId;

    // [����] �� ������ �迭 ���, �� UnitSO �迭�� ����մϴ�.
    // ���� �̸��� BattleManager���� ����ϴ� 'enemyUnits'�� �����ִ� ���� �����ϴ�.
    public UnitSO[] enemyUnits;

    // ���� �� �κ��� �߰����ּ��� ����
    [Header("���� ����")]
    [Tooltip("���� �¸� �� �÷��̾�� ������ ���� ������ ���")]
    public List<RewardTableSO> rewardChoices;

    // (����) ���� �������
    // public AudioClip battleMusic;
}