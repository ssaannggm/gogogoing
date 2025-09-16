// Assets/Game/Scripts/Data/StartingKitSO.cs (�� ����)
using UnityEngine;
using Game.Items;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StartingKit_", menuName = "Game/Data/Starting Kit")]
public class StartingKitSO : ScriptableObject
{
    public List<ItemSO> startingItems;
}