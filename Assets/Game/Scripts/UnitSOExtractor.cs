// Assets/Game/Scripts/UnitSOExtractor.cs
using UnityEngine;
using Game.Data; // UnitSO�� �ִ� ���ӽ����̽�

// �� ������Ʈ�� �����Ϳ����� ���˴ϴ�.
[RequireComponent(typeof(SpumVisualApplier))]
public class UnitSOExtractor : MonoBehaviour
{
    [Header("1. ������ �����Ͱ� �ִ� ������Ʈ")]
    [Tooltip("�� ������Ʈ�� ����� ��������Ʈ ������ �о�ɴϴ�.")]
    public SpumVisualApplier visualApplier;

    [Header("2. ������ UnitSO ����")]
    [Tooltip("���� ���� UnitSO ���� ������ �̸��� �˴ϴ�.")]
    public string unitId = "new_unit";
    public string displayName = "New Unit";

    void OnValidate()
    {
        // ���Ǹ� ���� ������Ʈ�� ���� �� �ڵ����� visualApplier�� ã�� �����մϴ�.
        if (visualApplier == null)
        {
            visualApplier = GetComponent<SpumVisualApplier>();
        }
    }
}