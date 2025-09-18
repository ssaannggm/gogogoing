using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Linq를 사용하기 위해 추가합니다.
using Game.Items; // EquipSlot 등을 사용하기 위해 추가

// [CreateAssetMenu] 어트리뷰트는 Unity 에디터에서 쉽게 이 SO 에셋을 생성할 수 있게 해줍니다.
// Assets 창에서 우클릭 -> Create -> Game Data -> Reward Table 경로로 생성할 수 있습니다.
[CreateAssetMenu(fileName = "New Reward Table", menuName = "Game Data/Reward Table")]
public class RewardTableSO : ScriptableObject
{
    [Header("확정 보상")]
    [Tooltip("보상으로 지급될 최소 골드")]
    public int minGold;
    [Tooltip("보상으로 지급될 최대 골드")]
    public int maxGold;

    [Tooltip("보상으로 지급될 최소 명성")]
    public int minFame;
    [Tooltip("보상으로 지급될 최대 명성")]
    public int maxFame;

    [Header("아이템 보상 규칙")]
    [Tooltip("이 테이블에서 보상으로 나올 수 있는 아이템 후보 목록")]
    public List<ItemDropInfo> itemDropList;

    [Tooltip("위에 있는 아이템 후보 중에서 실제로 드랍할 아이템의 개수")]
    public int numberOfItemsToDrop = 1;

    /// <summary>
    /// 이 보상 테이블의 규칙에 따라 실제 보상을 생성하여 반환하는 함수입니다.
    /// </summary>
    /// <returns>생성된 보상 데이터 묶음</returns>
    public Reward GenerateReward()
    {
        // 1. 골드, 명성 계산
        int gold = Random.Range(minGold, maxGold + 1);
        int fame = Random.Range(minFame, maxFame + 1);

        // 2. 아이템 추첨
        List<ItemSO> rewardItems = new List<ItemSO>();

        // 드랍 테이블과 드랍할 아이템 개수가 유효한지 확인
        if (itemDropList != null && itemDropList.Count > 0 && numberOfItemsToDrop > 0)
        {
            // 가중치 총합 계산
            float totalWeight = itemDropList.Sum(item => item.dropWeight);

            // 정해진 횟수(numberOfItemsToDrop)만큼 아이템 추첨 반복
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
                        break; // 아이템 하나를 뽑았으면 다음 추첨으로 넘어감
                    }
                }
            }
        }

        // 3. 최종 보상 데이터 생성 및 반환
        return new Reward(gold, fame, rewardItems);
    }
}

/// <summary>
/// 아이템과 드랍 가중치를 묶어서 관리하기 위한 보조 클래스입니다.
/// [System.Serializable] 어트리뷰트가 있어야 Unity 인스펙터 창에 노출됩니다.
/// </summary>
[System.Serializable]
public class ItemDropInfo
{
    public ItemSO item;
    [Tooltip("드랍 가중치. 높을수록 나올 확률이 높아집니다. (절대적인 확률이 아님)")]
    [Min(0)]
    public float dropWeight = 1f;
}

/// <summary>
/// 생성된 최종 보상 데이터를 담기 위한 구조체입니다.
/// 골드, 명성, 아이템 리스트를 하나로 묶어 전달하는 용도입니다.
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