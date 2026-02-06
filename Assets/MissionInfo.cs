using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionInfo : MonoBehaviour
{
    public int slot;
    public Text Depth;
    void Update()
    {
        var missionManager = MissionManager.Instance;

        if (missionManager == null)
            return;

        var mission = missionManager.activeSlots[slot];

        if (mission == null)
        {
            Destroy(gameObject);
            return;
        }

        Depth.text = $"{(Mathf.Abs(mission.currentItemPosition.y) + 3.73f) * 50f}m";
    }
}
