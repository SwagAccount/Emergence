using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public List<GameObject> Displays = new();
    const float MapSize = 41.107f;

    public void SetPoint(Vector3 pos, int index = 0)
    {
        if (index >= Displays.Count)
            return;

        Displays[index].transform.localPosition = new Vector3(pos.x / MapSize, pos.z/ MapSize, 0);
    }
}
