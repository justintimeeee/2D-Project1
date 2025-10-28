using UnityEngine;
using System.Collections;

public class ScreenShake2D : MonoBehaviour
{
    public static ScreenShake2D I;
    public Transform target;
    public float defaultIntensity = 0.05f;
    public float defaultDuration = 0.15f;

    Vector3 _origin;
    Coroutine _running;

    void Awake()
    {
        if (I) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (!target && Camera.main) target = Camera.main.transform;
        if (target) _origin = target.position;
    }

    public void Shake(float intensity, float duration)
    {
        if (!target) return;
        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(DoShake(intensity, duration));
    }

    public void Shake() => Shake(defaultIntensity, defaultDuration);

    IEnumerator DoShake(float intensity, float duration)
    {
        if (!target) yield break;
        _origin = target.position;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float x = Random.Range(-intensity, intensity);
            float y = Random.Range(-intensity, intensity);
            target.position = _origin + new Vector3(x, y, 0f);
            yield return null;
        }
        target.position = _origin;
        _running = null;
    }
}
