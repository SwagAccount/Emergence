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
    [HideInInspector] public Renderer renderer;

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

        renderer?.material?.SetColor("_BaseColor", colour);
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

        var side = Sides[UnityEngine.Random.Range(0, Sides.Count())];
        var roof = Roofs[UnityEngine.Random.Range(0, Roofs.Count())];

        var sideObject = Instantiate(side.Side, transform.position, Quaternion.Euler(-90f, 0, 0));
        sideObject.transform.SetParent(transform);
        
        var roofObject = Instantiate(roof, transform.position + Vector3.up * side.Height, Quaternion.Euler(-90f, UnityEngine.Random.Range(0,5) * 90f, 0));
        roofObject.transform.SetParent(transform);

        colour = Color.Lerp( ColourGradient.Evaluate(UnityEngine.Random.Range(0f, 1f)), Color.white, Saturation);
        renderer = sideObject.GetComponent<Renderer>();

        if (Application.isPlaying)
            renderer?.material?.SetColor("_BaseColor", colour);
    }
}
