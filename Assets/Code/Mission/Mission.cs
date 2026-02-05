using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mission")]
public class Mission : ScriptableObject
{
    public string MissionID;
    public string MissionName;
    public string MissionDescription;
    public int Reward;
    public GameObject ItemPrefab;
    public List<Vector3> Positions;
    public int StartingMoney;
    public int MissionDelay;

    public Vector3 RollPosition()
    {
        return Positions[Random.Range(0, Positions.Count)];
    }
}

[System.Serializable]
public class MissionSaveData
{
    public string missionID;
    public Vector3 currentItemPosition;
    public int lastCompletedAt;
}

[System.Serializable]
public class MissionManagerSave
{
    public int totalMissionsCompleted;
    public List<MissionSaveData> missions = new();
    public string[] activeMissionSlots = new string[2];
}

