using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionButton : MonoBehaviour
{
    public GameObject ActiveVisual;
    public GameObject SelectedVisual;
    public GameObject SelectableVisual;
    public Button Button;
    public Mission mission;
    public UISounds UISounds;

    public Text Name;
    public Text Depth;
    public Text Reward;
    public Text Requirement;

    public bool selectable = false;

    public MissionManager MissionManager => MissionManager.Instance;
    public Menu Menu => Menu.Instance;

    private void Update()
    {
        Button.enabled = selectable;
        UISounds.enabled = selectable;
        SelectableVisual.SetActive(!selectable);
        var isSelected = Menu.SelectedMission == mission;
        SelectedVisual.SetActive(isSelected);

        var isActive = MissionManager.MissionActive(mission);
        ActiveVisual.SetActive(isActive);
    }

    public void Select()
    {
        Menu.SelectMission(mission);
    }
}
