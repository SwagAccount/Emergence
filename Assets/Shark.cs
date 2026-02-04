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

    public float DepthSpring = 5f;
    public float DepthDamp = 6f;


    // Start is called before the first frame update
    void Start()
    {
        navPathSolver = GetComponent<NavPathSolver>();
        rigidBody = GetComponent<Rigidbody>();
    }

    public GameObject Target => Submarine.Instance.gameObject;

    // Update is called once per frame
    void FixedUpdate()
    {
        var targetPos = Target.transform.position;
        Move(targetPos);
        float rot = Mathf.Cos(Time.time * TailSpeed) * TailRotation * rigidBody.velocity.magnitude;
        Tail.transform.localRotation = Quaternion.Euler(0, 0, rot);

        Depth(targetPos.y);
    }

    void Depth(float targetY)
    {
        float displacement = targetY - transform.position.y;

        float force =
            (displacement * DepthSpring) -
            (rigidBody.velocity.y * DepthDamp);

        rigidBody.AddForce(force * Vector3.up);
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

        rigidBody.AddForce(transform.forward * Speed);
    }
}
