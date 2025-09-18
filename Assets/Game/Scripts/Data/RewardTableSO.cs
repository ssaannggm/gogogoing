using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Linq�� ����ϱ� ���� �߰��մϴ�.
using Game.Items; // EquipSlot ���� ����ϱ� ���� �߰�

// [CreateAssetMenu] ��Ʈ����Ʈ�� Unity �����Ϳ��� ���� �� SO ������ ������ �� �ְ� ���ݴϴ�.
// Assets â���� ��Ŭ�� -> Create -> Game Data -> Reward Table ��η� ������ �� �ֽ��ϴ�.
[CreateAssetMenu(fileName = "New Reward Table", menuName = "Game Data/Reward Table")]
public class RewardTableSO : ScriptableObject
{
    [Header("Ȯ�� ����")]
    [Tooltip("�������� ���޵� �ּ� ���")]
    public int minGold;
    [Tooltip("�������� ���޵� �ִ� ���")]
    public int maxGold;

    [Tooltip("�������� ���޵� �ּ� ��")]
    public int minFame;
    [Tooltip("�������� ���޵� �ִ� ��")]
    public int maxFame;

    [Header("������ ���� ��Ģ")]
    [Tooltip("�� ���̺��� �������� ���� �� �ִ� ������ �ĺ� ���")]
    public List<ItemDropInfo> itemDropList;

    [Tooltip("���� �ִ� ������ �ĺ� �߿��� ������ ����� �������� ����")]
    public int numberOfItemsToDrop = 1;

    /// <summary>
    /// �� ���� ���̺��� ��Ģ�� ���� ���� ������ �����Ͽ� ��ȯ�ϴ� �Լ��Դϴ�.
    /// </summary>
    /// <returns>������ ���� ������ ����</returns>
    public Reward GenerateReward()
    {
        // 1. ���, �� ���
        int gold = Random.Range(minGold, maxGold + 1);
        int fame = Random.Range(minFame, maxFame + 1);

        // 2. ������ ��÷
        List<ItemSO> rewardItems = new List<ItemSO>();

        // ��� ���̺�� ����� ������ ������ ��ȿ���� Ȯ��
        if (itemDropList != null && itemDropList.Count > 0 && numberOfItemsToDrop > 0)
        {
            // ����ġ ���� ���
            float totalWeight = itemDropList.Sum(item => item.dropWeight);

            // ������ Ƚ��(numberOfItemsToDrop)��ŭ ������ ��÷ �ݺ�
            for (int i = 0; i < numberOfItemsToDrop; i++)
            {
                float pick = Random.Range(0, totalWeight);
                float currentWeight = 0;

                foreach (var itemInfo in itemDropList)
                {
                    currentWeight += itemInfo.dropWeight;
                    if (pick <= currentWeight)
                    {
                        rewardItems.Add(itemInfo.item);
                        break; // ������ �ϳ��� �̾����� ���� ��÷���� �Ѿ
                    }
                }
            }
        }

        // 3. ���� ���� ������ ���� �� ��ȯ
        return new Reward(gold, fame, rewardItems);
    }
}

/// <summary>
/// �����۰� ��� ����ġ�� ��� �����ϱ� ���� ���� Ŭ�����Դϴ�.
/// [System.Serializable] ��Ʈ����Ʈ�� �־�� Unity �ν����� â�� ����˴ϴ�.
/// </summary>
[System.Serializable]
public class ItemDropInfo
{
    public ItemSO item;
    [Tooltip("��� ����ġ. �������� ���� Ȯ���� �������ϴ�. (�������� Ȯ���� �ƴ�)")]
    [Min(0)]
    public float dropWeight = 1f;
}

/// <summary>
/// ������ ���� ���� �����͸� ��� ���� ����ü�Դϴ�.
/// ���, ��, ������ ����Ʈ�� �ϳ��� ���� �����ϴ� �뵵�Դϴ�.
/// </summary>
public struct Reward
{
    public int gold;
    public int fame;
    public List<ItemSO> items;

    public Reward(int gold, int fame, List<ItemSO> items)
    {
        this.gold = gold;
        this.fame = fame;
        this.items = items;
    }
}