// Assets/Game/Scripts/Combat/HitInfo.cs (���� �Ϸ�� �ڵ�)
using UnityEngine;

namespace Game.Combat
{
    // ������ ���� ����� ��Ÿ���� ������(enum)
    public enum HitOutcome
    {
        Evade,  // ȸ��
        Block,  // ����
        Hit,    // �Ϲ� Ÿ��
        Crit    // ġ��Ÿ
    }

    public enum DamageType { Physical, Magical, True }

    public struct HitInfo
    {
        // [�߰�] ������ ���� ���
        public HitOutcome outcome;

        public Vector3 point;
        public Vector3 normal;
        public int amount;
        public bool critical; // outcome�� Crit�� �� true�� �����Ͽ� ���Ǽ� ����
        public DamageType damageType;
        public GameObject instigator; // ���� ��ü(����/����ü)
    }
}