// Assets/Game/Scripts/Data/RecruitmentInfoSO.cs (�� ����)
using UnityEngine;

[CreateAssetMenu(fileName = "RecruitInfo_", menuName = "Game/Data/Recruitment Info")]
public class RecruitmentInfoSO : ScriptableObject
{
    [Tooltip("�Ұ��� ����")]
    public UnitSO unit;

    [Tooltip("������ ��� ���丮 (500�� ����)")]
    [TextArea(5, 10)]
    public string story;

    [Tooltip("�������� ǥ�õ� ��� �̹���")]
    public Sprite backgroundImage;
}