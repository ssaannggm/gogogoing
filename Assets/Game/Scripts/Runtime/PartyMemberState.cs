// Assets/Game/Scripts/Runtime/PartyMemberState.cs
using System.Collections.Generic;
using Game.Items; // EquipSlot�� ����ϱ� ����

[System.Serializable]
public class PartyMemberState
{
    public string unitId;

    // �� ��� ���Կ� � ������ ID�� �����Ǿ����� �����ϴ� Dictionary
    public Dictionary<EquipSlot, string> equippedItemIds = new Dictionary<EquipSlot, string>();

    public PartyMemberState(string id)
    {
        unitId = id;
    }
}