using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    public List<Mission> AllMissions;

    public Dictionary<string, MissionInstance> missionStates = new();
    public MissionInstance[] activeSlots = new MissionInstance[2];

    string SavePath => Application.persistentDataPath + "/missions.json";

    int totalMissionsCompleted;

    void Awake()
    {
        Economy.Initialize();

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeOrLoad();

        foreach(var instance in missionStates.Values)
        {
            if (instance == null)
                return;
            instance.currentItemPosition = instance.mission.RollPosition();
        }
    }

    void InitializeOrLoad()
    {
        if (File.Exists(SavePath))
        {
            Load();
            return;
        }

        foreach (var mission in AllMissions)
        {
            var instance = new MissionInstance
            {
                mission = mission,
                currentItemPosition = mission.RollPosition(),
                lastCompletedAt = 0
            };

            missionStates.Add(mission.MissionID, instance);
        }
    }

    public void SelectMission(Mission mission)
    {
        var instance = missionStates[mission.MissionID];

        if (IsMissionActive(mission))
            return;

        int slot = GetFreeSlot();
        if (slot == -1)
        {
            activeSlots[0] = activeSlots[1];
            slot = 1;
        }

        activeSlots[slot] = instance;
        Save();
    }

    public void DeselectMission(Mission mission)
    {
        for (int i = 0; i < activeSlots.Length; i++)
        {
            if (activeSlots[i]?.mission == mission)
            {
                activeSlots[i] = null;
                Save();
                return;
            }
        }
    }

    bool IsMissionActive(Mission mission)
    {
        foreach (var slot in activeSlots)
            if (slot?.mission == mission)
                return true;

        return false;
    }

    int GetFreeSlot()
    {
        for (int i = 0; i < activeSlots.Length; i++)
            if (activeSlots[i] == null)
                return i;

        return -1;
    }

    public void SpawnActiveMissionItems()
    {
        InitializeOrLoad();
        foreach (var instance in activeSlots)
        {
            if (instance == null || instance.spawnedItem != null)
                continue;

            instance.spawnedItem = Instantiate(
                instance.mission.ItemPrefab,
                instance.currentItemPosition,
                Quaternion.identity
            );

        }
    }

    public void ItemsReceived(List<Item> items)
    {
        for (int i = 0; i < activeSlots.Length; i++)
        {
            var instance = activeSlots[i];
            if (instance == null) continue;

            foreach (var item in items)
            {
                if (item.name == instance.mission.ItemPrefab.GetComponent<Item>().name)
                {
                    CompleteMission(i);
                    break;
                }
            }
        }
    }

    void CompleteMission(int slot)
    {
        var instance = activeSlots[slot];

        Economy.AddMoney(instance.mission.Reward);

        totalMissionsCompleted++;

        instance.lastCompletedAt = totalMissionsCompleted;

        instance.currentItemPosition = instance.mission.RollPosition();

        activeSlots[slot] = null;

        Save();
    }

    void Save()
    {
        MissionManagerSave save = new MissionManagerSave
        {
            totalMissionsCompleted = totalMissionsCompleted
        };

        foreach (var kvp in missionStates)
        {
            var instance = kvp.Value;

            save.missions.Add(new MissionSaveData
            {
                missionID = instance.mission.MissionID,
                currentItemPosition = instance.currentItemPosition,
                lastCompletedAt = instance.lastCompletedAt
            });
        }

        for (int i = 0; i < save.activeMissionSlots.Length; i++)
        {
            save.activeMissionSlots[i] =
                activeSlots[i]?.mission.MissionID;
        }

        string json = JsonUtility.ToJson(save, true);
        File.WriteAllText(SavePath, json);
    }

    void Load()
    {
        var json = File.ReadAllText(SavePath);
        var save = JsonUtility.FromJson<MissionManagerSave>(json);

        totalMissionsCompleted = save.totalMissionsCompleted;

        missionStates.Clear();

        foreach (var mission in AllMissions)
        {
            var data = save.missions.Find(m => m.missionID == mission.MissionID);

            MissionInstance instance;

            if (data != null)
            {
                instance = new MissionInstance
                {
                    mission = mission,
                    currentItemPosition = data.currentItemPosition,
                    lastCompletedAt = data.lastCompletedAt
                };
            }
            else
            {
                instance = new MissionInstance
                {
                    mission = mission,
                    currentItemPosition = mission.RollPosition(),
                    lastCompletedAt = -mission.MissionDelay
                };
            }

            missionStates.Add(mission.MissionID, instance);
        }

        for (int i = 0; i < activeSlots.Length; i++)
        {
            activeSlots[i] = null;
            Debug.Log(save.activeMissionSlots[i]);
            string id = save.activeMissionSlots[i];
            if (string.IsNullOrEmpty(id))
                continue;

            if (missionStates.TryGetValue(id, out var instance))
                activeSlots[i] = instance;
        }
    }

    public class MissionInstance
    {
        public Mission mission;
        public Vector3 currentItemPosition;
        public GameObject spawnedItem;
        public int lastCompletedAt;
    }

    public bool MissionActive(Mission mission)
    {
        if (activeSlots == null)
            return false;
        return activeSlots.Any(x => x?.mission == mission);
    }

    public List<MissionInstance> GetAvailableMissions()
    {
        List<MissionInstance> result = new();

        foreach (var instance in missionStates.Values)
        {
            var mission = instance.mission;

            if (Economy.Money < mission.StartingMoney)
                continue;

            int missionsSinceLast =
                totalMissionsCompleted - instance.lastCompletedAt;

            if (missionsSinceLast < mission.MissionDelay)
                continue;

            result.Add(instance);
        }

        return result;
    }

    private void Update()
    {
        if (Instance == null)
            Instance = this;
    }
}

