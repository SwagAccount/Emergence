using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Submarine : MonoBehaviour
{
    public float WheelRotation = 0;
    public float RotationPower = 100f;

    public float Thrust = 0;
    public float ThrustPower = 10f;

    public GameObject Camera;

    public float CameraHeight = 1.4f;

    public float WaterLevel = 3.74f;
    public float TargetDepth = 0;

    public Wheel wheel;
    public Slider Thruster;

    Rigidbody rigidBody;
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Camera.transform.position = transform.position + Vector3.up * CameraHeight;
    }

    void FixedUpdate()
    {
        Input();

        Rotation();

        DoThrust();

        Depth();
    }

    void Input()
    {
        WheelRotation = -wheel.rot;
        Thrust = Thruster.value;
    }

    void Depth()
    {
        float targetDepth = WaterLevel - TargetDepth;
        float distance = transform.position.y - targetDepth;
        rigidBody.AddForce(-Vector3.up * distance);
    }

    void DoThrust()
    {
        rigidBody.AddForce(transform.forward * ThrustPower * Thrust);
    }

    float lastRotation;
    void Rotation()
    {
        var delta = (WheelRotation - lastRotation);
        lastRotation = WheelRotation;

        if (Mathf.Abs(delta) <= 0)
            return;

        rigidBody.AddTorque(-Vector3.forward * 100 * delta * RotationPower);
    }
}
