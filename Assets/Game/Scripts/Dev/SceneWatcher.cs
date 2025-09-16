// Assets/Game/Scripts/Dev/SceneWatcher.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using System.Linq;

public sealed class SceneWatcher : MonoBehaviour
{
    void Awake()
    {
        //DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnLoaded;
        SceneManager.sceneUnloaded += OnUnloaded;
        SceneManager.activeSceneChanged += OnActiveChanged;
        Debug.Log("[SceneWatcher] Ready");
        Dump();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnLoaded;
        SceneManager.sceneUnloaded -= OnUnloaded;
        SceneManager.activeSceneChanged -= OnActiveChanged;
    }

    void OnLoaded(Scene sc, LoadSceneMode m)
    {
        Debug.Log($"[SceneWatcher] Loaded: {sc.name} ({m})");
        Dump();
    }

    void OnUnloaded(Scene sc)
    {
        Debug.Log($"[SceneWatcher] Unloaded: {sc.name}");
        Dump();
    }

    void OnActiveChanged(Scene prev, Scene next)
    {
        Debug.Log($"[SceneWatcher] Active: {prev.name} ¡æ {next.name}");
        Dump();
    }

    void Dump()
    {
        var sb = new StringBuilder();
        sb.Append("[SceneWatcher] Loaded: ");
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            sb.Append(s.name);
            if (s == SceneManager.GetActiveScene()) sb.Append(" (Active)");
            if (i < SceneManager.sceneCount - 1) sb.Append(", ");
        }
        Debug.Log(sb.ToString());
        var f = FadeCanvas.TryGet();
        if (f != null)
        {
            var cg = f.GetComponent<CanvasGroup>();
            Debug.Log($"[SceneWatcher] FadeCanvas alpha={cg?.alpha:F2}, blocksRaycasts={cg?.blocksRaycasts}");
        }
        Debug.Log($"[SceneWatcher] Time.timeScale={Time.timeScale}");
    }
}
