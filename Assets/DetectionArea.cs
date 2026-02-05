using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectionArea : MonoBehaviour
{
    public float Distance = 2.7f;
    public float MinY = 1;
    public float MaxY = 2;
    public GameObject Target => Submarine.Instance.gameObject;
    public bool Inside => 
        Vector3.Distance( new Vector3( Target.transform.position.x, 0, Target.transform.position.z), new Vector3(transform.position.x, 0, transform.position.z) ) < Distance
        && Target.transform.position.y < MaxY
        && Target.transform.position.y > MinY;
}
