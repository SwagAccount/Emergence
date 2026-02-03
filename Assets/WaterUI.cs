using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterUI : MonoBehaviour
{
    public RectTransform RectTransform;
    public float OutPos = 1130f;
    public float Level = 0;
    public float FloatSpeed = 1f;
    public float FloatDistance = 10f;
    void Start()
    {
        
    }
    void Update()
    {
        var x = Mathf.Cos(Time.time);
        var pos = (new Vector3(Mathf.Cos((Time.time+10000) * FloatSpeed), Mathf.Cos(Time.time * FloatSpeed), 0.5f) - Vector3.one * 0.5f) * FloatDistance;
        RectTransform.localPosition = new Vector3(0, Mathf.Lerp(-OutPos, 0, Level), 0) + pos;
    }
}
