// Assets/Game/Scripts/Data/UnitSO.cs (���� �Ϸ�� ��ü �ڵ�)
using UnityEngine;
using Game.Items; // ItemSO�� ����ϱ� ���� �߰�

[CreateAssetMenu(fileName = "Unit_", menuName = "Game/Data/Unit Definition")]
public class UnitSO : ScriptableObject
{
    [Header("�⺻ ����")]
    public string unitId;
    public string displayName;

    [Header("UI")]
    [Tooltip("�κ��丮�� UI�� ǥ�õ� ĳ���� �ʻ�ȭ")]
    public Sprite portrait; // <-- �ʻ�ȭ �ʵ� �߰�

    [Header("�⺻ ��ü ���� (�˸�)")]
    public BodyParts bodyParts;
    public EyeParts eyeParts;

    [Header("�ʱ� ���")]
    [Tooltip("�� ������ ������ �� �⺻���� ������ �����۵�")]
    public DefaultEquipment defaultEquipment; // [����] �ʱ� ��� ����ü

    [System.Serializable]
    public struct BodyParts
    {
        [Header("��/��")]
        public Sprite Head, Hair, FaceHair, Body;
        [Header("��/�ٸ�")]
        public Sprite L_Arm, R_Arm, L_Foot, R_Foot;
        // [����] Shadow �ʵ� ����
    }

    [System.Serializable]
    public struct EyeParts
    {
        [Tooltip("Ȱ��ȭ�� ���� �޺κ� ��������Ʈ")]
        public Sprite back;
        [Tooltip("Ȱ��ȭ�� ���� �պκ�(������) ��������Ʈ")]
        public Sprite front;
    }

    // [�߰�] �ʱ� ��� ���� ����ü
    [System.Serializable]
    public struct DefaultEquipment
    {
        public ItemSO rightHand;
        public ItemSO leftHand;
        public ItemSO helmet;
        public ItemSO armor;
    }
}