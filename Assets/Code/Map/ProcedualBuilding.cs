using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class ProcedualBuilding : MonoBehaviour
{
    public bool Randomise;
    public BuildingSide[] Sides;
    public Gradient ColourGradient;
    public GameObject[] Roofs;
    public float Saturation = 0.5f;
    [HideInInspector] public Color colour;
    public Vector2 LevelsCount = new Vector2(1, 2);
    [Serializable]
    public struct BuildingSide
    {
        public GameObject Side;
        public float Height;
    }
    private void Start()
    {
        if (!Application.isPlaying)
            return;
        foreach(Transform child in transform)
        {
            var renderer = child.GetComponent<Renderer>();

            renderer?.material?.SetColor("_BaseColor", colour);
        }
        
    }
    private void Update()
    {
        if (!Randomise)
            return;
        Randomise = false;

        List<Transform> remove = new();

        foreach(Transform trans in transform)
        {
            remove.Add(trans);
        }

        foreach(var trans in remove)
        {
            DestroyImmediate(trans.gameObject);
        }
        colour = Color.Lerp(ColourGradient.Evaluate(UnityEngine.Random.Range(0f, 1f)), Color.white, Saturation);
        var side = Sides[UnityEngine.Random.Range(0, Sides.Count())];
        var roof = Roofs[UnityEngine.Random.Range(0, Roofs.Count())];
        var level = 0f;
        var levels = UnityEngine.Random.Range(LevelsCount.x, LevelsCount.y);
        for (int i = 0; i < levels; i++)
        {
            var sideObject = Instantiate(side.Side, transform.position + Vector3.up * level, Quaternion.Euler(-90f, 0, 0));
            sideObject.transform.SetParent(transform);
            level += side.Height;
            var renderer = sideObject.GetComponent<Renderer>();

            if (Application.isPlaying)
                renderer?.material?.SetColor("_BaseColor", colour);
        }
        var roofObject = Instantiate(roof, transform.position + Vector3.up * level, Quaternion.Euler(-90f, UnityEngine.Random.Range(0,5) * 90f, 0));
        roofObject.transform.SetParent(transform);
        var roofRenderer = roofObject.GetComponent<Renderer>();
        if (Application.isPlaying)
            roofRenderer?.material?.SetColor("_BaseColor", colour);
    }
}
