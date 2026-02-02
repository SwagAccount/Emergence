using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class VisualManager : MonoBehaviour
{
    public Renderer WaterRenderer;
    public Light Light;
    public float WaterLevel = 3.8f;
    public float LightPowerBelow = 0.01f;
    public float LightPowerAbove = 0.5f;
    public float TransitionSpeed = 10;

    float t;
    void Update()
    {
        bool above = transform.position.y > WaterLevel;
        t = Mathf.Lerp(t, above ? 1 : 0, Time.deltaTime * TransitionSpeed);
        Light.intensity = Mathf.Lerp(LightPowerBelow, LightPowerAbove, t);
        WaterRenderer.material.SetColor("_BaseColor", Color.white.WithAlpha(t));
    }
}
