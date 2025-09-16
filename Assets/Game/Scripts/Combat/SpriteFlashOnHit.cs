// SpriteFlashOnHit.cs
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class SpriteFlashOnHit : MonoBehaviour
{
    SpriteRenderer _sr;
    Color _orig;
    Coroutine _co;

    void Awake() { _sr = GetComponent<SpriteRenderer>(); _orig = _sr.color; }

    public void Flash(Color c, float dur)
    {
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(Co_Flash(c, dur));
    }

    IEnumerator Co_Flash(Color c, float d)
    {
        _sr.color = c;
        yield return new WaitForSeconds(d);
        _sr.color = _orig;
        _co = null;
    }
}
