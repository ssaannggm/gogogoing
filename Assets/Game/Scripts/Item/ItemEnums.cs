// Assets/Game/Scripts/Items/ItemEnums.cs
namespace Game.Items
{
    public enum EquipSlot { RightHand, LeftHand, Helmet, Armor, Accessory }

    public enum StatKind { Attack, Defense, Evasion, CritChance, MoveSpeed, AttackSpeed, MaxHP }
    public enum StatOp { Add, Mul }

    public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }
}
