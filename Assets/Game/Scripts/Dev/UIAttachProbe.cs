// Assets/Game/Scripts/Dev/UIAttachProbe.cs
using UnityEngine;

public sealed class UIAttachProbe : MonoBehaviour
{
    public RectTransform parent;     // Slots 또는 Roster
    public GameObject prefab;        // SlotButton 또는 RosterButton
    public int count = 3;

    [ContextMenu("Spawn Test")]
    void SpawnTest()
    {
        if (!parent || !prefab) { Debug.LogError("[UIAttachProbe] parent/prefab NULL"); return; }
        Debug.Log($"[UIAttachProbe] before childCount={parent.childCount}");

        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(prefab);
            var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            if (rt.sizeDelta == Vector2.zero) rt.sizeDelta = new Vector2(160, 40);
            go.name = $"{prefab.name}_Probe_{i}";
            go.SetActive(true);
        }

        Debug.Log($"[UIAttachProbe] after childCount={parent.childCount}");
    }
}
