using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MenuButtons : MonoBehaviour
{
    public GameObject ButtonPrefab;
    public float waterLevel = 3.73f;
    void Start()
    {
        var availableMissions = MissionManager.Instance.GetAvailableMissions();
        for (int i = 0; i < MissionManager.Instance.missionStates.Values.Count; i++)
        {
            var mission = MissionManager.Instance.missionStates.Values.ElementAt(i);
            
            var button = Instantiate(ButtonPrefab);
            button.transform.SetParent(transform);
            var pos = Vector3.zero;
            pos.y = i * -131f;
            button.transform.localPosition = pos;
            button.transform.localRotation = Quaternion.identity;
            button.transform.localScale = Vector3.one;
            var missionButton = button.GetComponent<MissionButton>();
            missionButton.mission = mission.mission;
            missionButton.selectable = availableMissions.Any(x => x.mission == mission.mission);

            missionButton.Name.text = mission.mission.name.ToUpper();
            missionButton.Depth.text = $"{(Mathf.Abs(mission.currentItemPosition.y) + waterLevel) * 50f}m";
            missionButton.Reward.text = $"${mission.mission.Reward}";
            missionButton.Requirement.text = "";
            if (!missionButton.selectable)
            {
                if (Economy.Money < mission.mission.StartingMoney)
                {
                    int missingMoney = mission.mission.StartingMoney - Economy.Money;
                    missionButton.Requirement.text = $"REQUIRES ${missingMoney}";
                }
                else
                {
                    if (mission.Cooldown > 0)
                    {
                        missionButton.Requirement.text =
                            $"COOLDOWN: {mission.Cooldown}";
                    }
                }
            }

        }
    }
}
