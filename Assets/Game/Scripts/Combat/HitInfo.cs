// Assets/Game/Scripts/Combat/HitInfo.cs (수정 완료된 코드)
using UnityEngine;

namespace Game.Combat
{
    // 공격의 최종 결과를 나타내는 열거형(enum)
    public enum HitOutcome
    {
        Evade,  // 회피
        Block,  // 막기
        Hit,    // 일반 타격
        Crit    // 치명타
    }

    public enum DamageType { Physical, Magical, True }

    public struct HitInfo
    {
        // [추가] 공격의 최종 결과
        public HitOutcome outcome;

        public Vector3 point;
        public Vector3 normal;
        public int amount;
        public bool critical; // outcome이 Crit일 때 true로 설정하여 편의성 유지
        public DamageType damageType;
        public GameObject instigator; // 가한 주체(유닛/투사체)
    }
}