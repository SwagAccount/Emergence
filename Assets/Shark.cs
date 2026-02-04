using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shark : MonoBehaviour
{
    public GameObject Tail;
    public float TailSpeed;
    public float TailRotation;
    public float Speed = 50;
    private Rigidbody rigidBody;
    private NavPathSolver navPathSolver;
    public float RotationSpeed = 20;
    public float RotationDamp = 1;
    // Start is called before the first frame update
    void Start()
    {
        navPathSolver = GetComponent<NavPathSolver>();
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Move(Submarine.Instance.transform.position);
        float rot = Mathf.Cos(Time.time * TailSpeed) * TailRotation * rigidBody.velocity.magnitude;
        Tail.transform.localRotation = Quaternion.Euler(0, 0, rot);
        rigidBody.AddForce( transform.forward * Speed * Time.deltaTime );
    }

    private void Move(Vector3 targetPos)
    {
        navPathSolver.SetTarget(targetPos);

        if (!navPathSolver.TryGetNextPoint(out Vector3 corner))
            return;

        var prevRot = transform.rotation;
        transform.LookAt(corner);
        var targetRot = transform.rotation;
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
