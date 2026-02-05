using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Light))]
public class LightFlickerCurve : MonoBehaviour
{
    [Header("Flicker Settings")]
    public AnimationCurve flickerCurve = AnimationCurve.Linear(0, 1, 1, 1);

    [Tooltip("How fast the curve is played")]
    public float speed = 1f;

    [Tooltip("How strong the flicker effect is")]
    public float intensityMultiplier = 1f;

    [Tooltip("Base light intensity")]
    public float baseIntensity = 1f;

    [Tooltip("Random offset so multiple lights don't sync")]
    public bool randomStartOffset = true;

    [Header("Additional Lights")]
    [Tooltip("Extra lights to flicker in addition to this GameObject's light")]
    public List<Light> additionalLights = new List<Light>();

    [Header("Emission Flicker")]
    [Tooltip("Renderers whose materials will have emissive flicker applied")]
    public List<Renderer> emissiveRenderers = new List<Renderer>();

    [Tooltip("Base emission intensity")]
    public float baseEmission = 1f;

    [Tooltip("Emission flicker strength")]
    public float emissionMultiplier = 1f;

    private Light mainLight;
    private float timeOffset;

    void Awake()
    {
        mainLight = GetComponent<Light>();

        if (randomStartOffset && flickerCurve.length > 0)
            timeOffset = Random.value * flickerCurve.keys[^1].time;
    }

    void Update()
    {
        if (flickerCurve.length == 0)
            return;

        float curveTime = flickerCurve.keys[^1].time;
        float t = (Time.time * speed + timeOffset) % curveTime;
        float curveValue = flickerCurve.Evaluate(t);

        float finalLightIntensity = baseIntensity + curveValue * intensityMultiplier;

        // Apply to main light
        if (mainLight != null)
            mainLight.intensity = finalLightIntensity;

        // Apply to additional lights
        foreach (var light in additionalLights)
        {
            if (light != null)
                light.intensity = finalLightIntensity;
        }

        // Apply to emissive materials
        float finalEmission = baseEmission + curveValue * emissionMultiplier;

        foreach (var renderer in emissiveRenderers)
        {
            if (renderer == null)
                continue;

            foreach (var mat in renderer.materials)
            {
                if (mat.HasProperty("_EmissionColor"))
                {
                    Color emissionColor = mat.GetColor("_EmissionColor");
                    float max = Mathf.Max(emissionColor.r, emissionColor.g, emissionColor.b, 0.0001f);
                    emissionColor /= max;
                    mat.SetColor("_EmissionColor", emissionColor * finalEmission);
                }
            }
        }
    }
}
