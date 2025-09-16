// Assets/Game/Scripts/Combat/MultiSpriteFlashOnHit.cs
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering; // SortingGroup

[DisallowMultipleComponent]
public sealed class MultiSpriteFlashOnHit : MonoBehaviour
{
    [SerializeField] Color defaultFlashColor = Color.white;
    [SerializeField] float defaultDuration = 0.05f;

    SortingGroup _sg;
    SpriteRenderer[] _renders;  // ĳ��: �׷� �ҼӸ�
    Color[] _orig;
    Coroutine _co;

    void Awake()
    {
        _sg = GetComponent<SortingGroup>();
        RebuildCache();
    }

    // ��Ų ��ü �� ��Ÿ�� ���� �� ȣ���� ĳ�� ����
    [ContextMenu("Rebuild Cache")]
    public void RebuildCache()
    {
        var all = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        if (_sg)
        {
            // �� SortingGroup�� �Ҽӵ� �������� ����
            var list = new System.Collections.Generic.List<SpriteRenderer>(all.Length);
            for (int i = 0; i < all.Length; i++)
            {
                var sr = all[i];
                if (!sr) continue;
                if (sr.GetComponentInParent<SortingGroup>() == _sg)
                    list.Add(sr);
            }
            _renders = list.ToArray();
        }
        else
        {
            _renders = all;
        }

        _orig = new Color[_renders.Length];
        for (int i = 0; i < _renders.Length; i++)
            _orig[i] = _renders[i] ? _renders[i].color : Color.white;
    }

    public void Flash(Color? color = null, float? duration = null)
    {
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(Co_Flash(color ?? defaultFlashColor, duration ?? defaultDuration));
    }

    IEnumerator Co_Flash(Color c, float d)
    {
        for (int i = 0; i < _renders.Length; i++)
            if (_renders[i]) _renders[i].color = c;

        yield return new WaitForSeconds(d);

        for (int i = 0; i < _renders.Length; i++)
            if (_renders[i]) _renders[i].color = _orig[i];

        _co = null;
    }
}
