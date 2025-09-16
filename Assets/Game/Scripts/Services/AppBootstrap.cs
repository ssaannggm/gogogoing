// AppBootstrap.cs (루트 오브젝트에만 부착)
using UnityEngine;

public sealed class AppBootstrap : MonoBehaviour
{
    static bool s_Inited;
    void Awake()
    {
        if (s_Inited) { Destroy(gameObject); return; }
        s_Inited = true;
        DontDestroyOnLoad(gameObject); // 자식들도 통째로 영속
    }
}
