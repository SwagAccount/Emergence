using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightVisual : MonoBehaviour
{
    Vector3 lastPos;
    public float RotSpeed = 1;
    void Update()
    {
        var vel = (transform.position - lastPos);
        vel.y = 0;
        var speed = vel.magnitude;
        lastPos = transform.position;

        var rot = transform.localEulerAngles;
        rot.z += speed * RotSpeed;
        transform.localEulerAngles = rot;
    }
}
