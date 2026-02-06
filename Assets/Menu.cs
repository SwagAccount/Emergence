using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public MissionManager MissionManager => MissionManager.Instance;
    public static Menu Instance;
    public Mission SelectedMission;

    public Text money;

    public Text MissionName;
    public Text MissionReward;
    public Text Description;
    public Text Depth;
    public MapDisplay MapDisplay;
    public Image IconDisplay;

    public Text SelectMissionText;
    public GameObject SelectMissionActiveVisual;

    public float waterLevel = 3.73f;

    private void Start()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Instance == null)
            Instance = this;

        var missionValid = SelectedMission != null;

        money.text = $"${Economy.Money}";
        MissionName.text = missionValid ? SelectedMission.name.ToUpper() : "NO MISSION SELECTED";
        MissionReward.text = missionValid ? $"${SelectedMission.Reward}" : "";
        Description.text = missionValid ? SelectedMission.MissionDescription : "";

        IconDisplay.sprite = SelectedMission?.MissionIcon;
        IconDisplay.color = missionValid ? Color.white : Color.white.WithAlpha(0);

        if (IconDisplay.sprite != null)
        {
            var ratio = (float)IconDisplay.sprite.texture.width / (float)IconDisplay.sprite.texture.height;
            var scale = IconDisplay.rectTransform.localScale;
            IconDisplay.rectTransform.localScale = new Vector3(scale.y * ratio, scale.y, scale.z);
        }
        

        var active = MissionManager.MissionActive(SelectedMission);
        SelectMissionActiveVisual.SetActive(active);
        SelectMissionText.text = active ? "DESELECT MISSION" : "SELECT MISSION";

        foreach (var mission in MissionManager.GetAvailableMissions())
        {
            if (mission?.mission != SelectedMission)
                continue;
            Depth.text = missionValid ? $"DEPTH:{(Mathf.Abs(mission.currentItemPosition.y) + waterLevel) * 50f}m" : "DEPTH:";
            MapDisplay.SetPoint(mission.currentItemPosition, 0);
            break;
        }
    }

    public void SelectMission(Mission mission)
    {
        SelectedMission = mission;
    }

    public void ToggleMission()
    {
        var mission = SelectedMission;  
        if (MissionManager.MissionActive(mission))
            MissionManager.DeselectMission(mission);
        else
            MissionManager.SelectMission(mission);
    }
}
