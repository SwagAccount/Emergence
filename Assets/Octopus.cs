using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Octopus : MonoBehaviour
{
    private Rigidbody rigidBody;
    public float Speed;
    public float RotationSpeed;
    public float RotationDamp;
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        foreach (var armEnd in ArmEnds)
            armEnd.transform.SetParent(null);

    }

    public GameObject Target;

    public List<Rigidbody> ArmEnds = new();

    void FixedUpdate()
    {
        var prevRot = transform.rotation;
        transform.LookAt(Target.transform.position);
        var targetRot= transform.rotation;
        transform.rotation = prevRot;

        Quaternion q = targetRot * Quaternion.Inverse(transform.rotation);

        if (q.w < 0)
        {
            q.x = -q.x;
            q.y = -q.y;
            q.z = -q.z;
            q.w = -q.w;
        }

        q.ToAngleAxis(out float angle, out Vector3 axis);
        axis.Normalize();

        float angleRad = angle * Mathf.Deg2Rad;

        Vector3 torque =
            axis * (angleRad * RotationSpeed)
            - rigidBody.angularVelocity * RotationDamp;

        torque = new Vector3(0, torque.y, 0);

        rigidBody.AddTorque(torque, ForceMode.Acceleration);

        rigidBody.AddForce(transform.forward * Speed * Time.deltaTime);
    }
}
