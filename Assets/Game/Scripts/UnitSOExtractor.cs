// Assets/Game/Scripts/UnitSOExtractor.cs
using UnityEngine;
using Game.Data; // UnitSO가 있는 네임스페이스

// 이 컴포넌트는 에디터에서만 사용됩니다.
[RequireComponent(typeof(SpumVisualApplier))]
public class UnitSOExtractor : MonoBehaviour
{
    [Header("1. 추출할 데이터가 있는 컴포넌트")]
    [Tooltip("이 컴포넌트에 연결된 스프라이트 정보를 읽어옵니다.")]
    public SpumVisualApplier visualApplier;

    [Header("2. 생성할 UnitSO 정보")]
    [Tooltip("새로 만들 UnitSO 에셋 파일의 이름이 됩니다.")]
    public string unitId = "new_unit";
    public string displayName = "New Unit";

    void OnValidate()
    {
        // 편의를 위해 컴포넌트를 붙일 때 자동으로 visualApplier를 찾아 연결합니다.
        if (visualApplier == null)
        {
            visualApplier = GetComponent<SpumVisualApplier>();
        }
    }
}