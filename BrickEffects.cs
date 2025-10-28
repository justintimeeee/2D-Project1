using UnityEngine;

public class BrickEffects : MonoBehaviour
{
    public bool shakeOnDestroy = true;
    public float shakeIntensity = 0.06f;
    public float shakeDuration = 0.1f;

    void OnDestroy()
    {
        if (shakeOnDestroy && ScreenShake2D.I != null)
            ScreenShake2D.I.Shake(shakeIntensity, shakeDuration);
    }
}
