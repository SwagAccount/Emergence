using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PositionListMaker : MonoBehaviour
{
    public bool Get;
    public List<GameObject> Objects;
    public List<Vector3> OutputPositions;
    void Update()
    {
        if (!Get)
            return;

        Get = false;
        OutputPositions = new();
        foreach (var go in Objects)
        {
            OutputPositions.Add(go.transform.position);
        }
    }
}
