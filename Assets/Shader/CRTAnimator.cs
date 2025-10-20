using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class CRTAnimator : MonoBehaviour
{
    public Material crtMaterial;
    [Range(0f, 2f)] public float intensity = 1f;
    [Tooltip("Random seed for subtle variation")] public float seed = 0f;

    Graphic uiGraphic;
    Material instanceMat;

    void Awake()
    {
        uiGraphic = GetComponent<Graphic>();
        if (crtMaterial == null)
        {
            Debug.LogWarning("CRTAnimator: crtMaterial is not assigned.");
            return;
        }

        // Create a runtime instance so multiple terminals can have different settings
        instanceMat = Instantiate(crtMaterial);
        uiGraphic.material = instanceMat;

        // randomize a bit
        seed = Random.Range(0f, 9999f);
    }

    void Update()
    {
        if (instanceMat == null) return;

        // Slight time-based subtle offsets (these drive _Time in shader already but we can set extras)
        float t = Time.time + seed;

        // Make flicker a bit irregular
        float flicker = 0.5f + Mathf.PerlinNoise(t * 1.5f, seed) * 0.5f;
        float extraFlick = Mathf.Lerp(0.6f, 1.0f, flicker);

        // apply subtle global intensity (scale alpha + glow)
        instanceMat.SetFloat("_Alpha", Mathf.Clamp01(1.0f * intensity));
        instanceMat.SetFloat("_FlickerAmount", instanceMat.GetFloat("_FlickerAmount") * extraFlick);

        // Optionally nudge chromatic aberration slightly
        instanceMat.SetFloat("_Chromatic", 0.006f + Mathf.PerlinNoise(t * 0.3f, seed + 1f) * 0.006f);

        // move the scanlines by changing _Time - shader uses _Time.y itself; no need to pass explicitly.
        // If you want to offset the scanline texture uv, add property like _ScanOffset; for simplicity relying on _Time.
    }

    private void OnDestroy()
    {
        // cleanup instantiated material
        if (instanceMat != null)
            Destroy(instanceMat);
    }
}