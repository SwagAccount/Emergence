using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shark : MonoBehaviour
{
    public GameObject Tail;
    public float TailSpeed;
    public float TailRotation;
    public float Speed = 5;
    public float NonChasingSpeed = 1;
    private Rigidbody rigidBody;
    private NavPathSolver navPathSolver;
    public float RotationSpeed = 20;
    public float RotationDamp = 1;

    public float DepthSpring = 5f;
    public float DepthDamp = 6f;

    public float DamageForce;
    public float Damage = 5f;

    public float cooldown;

    public float AttackCoolDown;

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        if (cooldown >= 0)
            return;

        cooldown = AttackCoolDown;

        Submarine.Instance.rigidBody.AddForce(rigidBody.velocity * 200);
        Submarine.Instance.Health -= Damage;
    }

    private DetectionArea detectionArea;
    Vector3 startPos;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        detectionArea = GetComponent<DetectionArea>();
        navPathSolver = GetComponent<NavPathSolver>();
        rigidBody = GetComponent<Rigidbody>();
    }

    public GameObject Target => Submarine.Instance.gameObject;
    private void Update()
    {
        cooldown -= Time.deltaTime;
    }
    bool chasing;
    // Update is called once per frame
    void FixedUpdate()
    {
        chasing = detectionArea.Inside;

        var targetPos = chasing ? Target.transform.position : startPos;

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

        var adjustedTarget = corner;

        if (cooldown > 0f)
        {
            adjustedTarget = transform.position + (transform.position - corner);
        }

        var prevRot = transform.rotation;
        transform.LookAt(adjustedTarget);
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
        var speed = chasing ? Speed : NonChasingSpeed;
        rigidBody.AddForce(transform.forward * Speed);
    }
}
