// SimpleCameraShaker.cs
using UnityEngine;
using System.Collections;

public sealed class SimpleCameraShaker : MonoBehaviour
{
    Vector3 _origin; Coroutine _co;
    void Awake() { _origin = transform.localPosition; }

    public void Shake(float amp, float dur)
    {
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(Co_Shake(amp, dur));
    }

    IEnumerator Co_Shake(float a, float d)
    {
        float t = 0f;
        while (t < d)
        {
            t += Time.unscaledDeltaTime;
            var off = new Vector3((Random.value * 2 - 1) * a, (Random.value * 2 - 1) * a, 0);
            transform.localPosition = _origin + off;
            yield return null;
        }
        transform.localPosition = _origin;
        _co = null;
    }
}
