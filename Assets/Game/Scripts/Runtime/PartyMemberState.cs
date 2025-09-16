// Assets/Game/Scripts/Runtime/PartyMemberState.cs
using System.Collections.Generic;
using Game.Items; // EquipSlot을 사용하기 위해

[System.Serializable]
public class PartyMemberState
{
    public string unitId;

    // 각 장비 슬롯에 어떤 아이템 ID가 장착되었는지 저장하는 Dictionary
    public Dictionary<EquipSlot, string> equippedItemIds = new Dictionary<EquipSlot, string>();

    public PartyMemberState(string id)
    {
        unitId = id;
    }
}