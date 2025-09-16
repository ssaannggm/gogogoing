using UnityEngine;

public static class LayerGuardUtility
{
    // �� ���� ���� �� �� ��ü ���̾� ���� ���� Ȯ��
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void CheckLayersAtBoot()
    {
        string[] required = { "Ally", "Enemy", "Projectile" };
        foreach (var name in required)
        {
            if (LayerMask.NameToLayer(name) < 0)
                Debug.LogError($"[LayerGuard] �ʼ� ���̾� '{name}' �� �����ϴ�. Project Settings > Tags and Layers���� �߰��ϼ���.");
        }
    }
}

// �ʿ� �� ������Ʈ�ε� ��� ����(�����Ϳ��� ��� ǥ��)
#if UNITY_EDITOR
public sealed class LayerGuardProbe : MonoBehaviour
{
    [SerializeField] string[] requiredLayers = { "Ally", "Enemy", "Projectile" };
    void OnValidate()
    {
        foreach (var name in requiredLayers)
        {
            if (LayerMask.NameToLayer(name) < 0)
                Debug.LogError($"[LayerGuardProbe] �ʼ� ���̾� '{name}' �� �����ϴ�.", this);
        }
    }
}
#endif
