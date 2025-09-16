// Assets/Game/Scripts/Data/RecruitmentInfoSO.cs (새 파일)
using UnityEngine;

[CreateAssetMenu(fileName = "RecruitInfo_", menuName = "Game/Data/Recruitment Info")]
public class RecruitmentInfoSO : ScriptableObject
{
    [Tooltip("소개할 유닛")]
    public UnitSO unit;

    [Tooltip("유닛의 배경 스토리 (500자 내외)")]
    [TextArea(5, 10)]
    public string story;

    [Tooltip("선택지에 표시될 배경 이미지")]
    public Sprite backgroundImage;
}