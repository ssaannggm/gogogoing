// AppBootstrap.cs (��Ʈ ������Ʈ���� ����)
using UnityEngine;

public sealed class AppBootstrap : MonoBehaviour
{
    static bool s_Inited;
    void Awake()
    {
        if (s_Inited) { Destroy(gameObject); return; }
        s_Inited = true;
        DontDestroyOnLoad(gameObject); // �ڽĵ鵵 ��°�� ����
    }
}
