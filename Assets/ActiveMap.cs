using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveMap : MonoBehaviour
{
    MapDisplay MapDisplay;
    void Start()
    {
        MapDisplay = GetComponent<MapDisplay>();
    }

    // Update is called once per frame
    void Update()
    {
        MapDisplay.SetPoint(Submarine.Instance.transform.position);

        for (int i = 0; i < 2; i++)
        {
            var activeSlot = MissionManager.Instance.activeSlots[i];
            MapDisplay.SetPoint( activeSlot?.spawnedItem?.gameObject.transform.position ?? Vector3.one * 10000, i + 1);
        }
    }
}
