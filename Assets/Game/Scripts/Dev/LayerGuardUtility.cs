using UnityEngine;

public static class LayerGuardUtility
{
    // 앱 시작 전에 한 번 전체 레이어 존재 여부 확인
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void CheckLayersAtBoot()
    {
        string[] required = { "Ally", "Enemy", "Projectile" };
        foreach (var name in required)
        {
            if (LayerMask.NameToLayer(name) < 0)
                Debug.LogError($"[LayerGuard] 필수 레이어 '{name}' 가 없습니다. Project Settings > Tags and Layers에서 추가하세요.");
        }
    }
}

// 필요 시 컴포넌트로도 사용 가능(에디터에서 경고 표시)
#if UNITY_EDITOR
public sealed class LayerGuardProbe : MonoBehaviour
{
    [SerializeField] string[] requiredLayers = { "Ally", "Enemy", "Projectile" };
    void OnValidate()
    {
        foreach (var name in requiredLayers)
        {
            if (LayerMask.NameToLayer(name) < 0)
                Debug.LogError($"[LayerGuardProbe] 필수 레이어 '{name}' 가 없습니다.", this);
        }
    }
}
#endif
