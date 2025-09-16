// Assets/Game/Scripts/Data/UnitSO.cs (수정 완료된 전체 코드)
using UnityEngine;
using Game.Items; // ItemSO를 사용하기 위해 추가

[CreateAssetMenu(fileName = "Unit_", menuName = "Game/Data/Unit Definition")]
public class UnitSO : ScriptableObject
{
    [Header("기본 정보")]
    public string unitId;
    public string displayName;

    [Header("UI")]
    [Tooltip("인벤토리나 UI에 표시될 캐릭터 초상화")]
    public Sprite portrait; // <-- 초상화 필드 추가

    [Header("기본 신체 외형 (알몸)")]
    public BodyParts bodyParts;
    public EyeParts eyeParts;

    [Header("초기 장비")]
    [Tooltip("이 유닛이 생성될 때 기본으로 장착할 아이템들")]
    public DefaultEquipment defaultEquipment; // [수정] 초기 장비 구조체

    [System.Serializable]
    public struct BodyParts
    {
        [Header("얼굴/몸")]
        public Sprite Head, Hair, FaceHair, Body;
        [Header("팔/다리")]
        public Sprite L_Arm, R_Arm, L_Foot, R_Foot;
        // [삭제] Shadow 필드 제거
    }

    [System.Serializable]
    public struct EyeParts
    {
        [Tooltip("활성화된 눈의 뒷부분 스프라이트")]
        public Sprite back;
        [Tooltip("활성화된 눈의 앞부분(눈동자) 스프라이트")]
        public Sprite front;
    }

    // [추가] 초기 장비를 위한 구조체
    [System.Serializable]
    public struct DefaultEquipment
    {
        public ItemSO rightHand;
        public ItemSO leftHand;
        public ItemSO helmet;
        public ItemSO armor;
    }
}