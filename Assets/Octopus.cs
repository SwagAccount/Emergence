using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Octopus : MonoBehaviour
{
    private Rigidbody rigidBody;
    public float Speed;
    public float RotationSpeed;
    public float RotationDamp;
    public float ArmSpeed = 50f;
    public float MaxArmDis = 1;
    public float StickBreakForce = 1.7f;
    public float StickSpring = 100;
    public float DepthSpring = 5f;
    public float DepthDamp = 6f;
    public float MaxY = 2;
    public float DragDownSpeed = 1;
    public SoundEvent StickSound;
    public AudioSource StickLoop;
    public SoundEvent UnStickSound;
    public float StickLoopSmooth = 3f;
    public float StickDelay = 1f;
    public float cooldown;
    private DetectionArea DetectionArea;

    Vector3 startPoint;
    NavPathSolver navPathSolver;
    void Start()
    {
        navPathSolver = GetComponent<NavPathSolver>();
        startPoint = transform.position;
        rigidBody = GetComponent<Rigidbody>();
        foreach (var armEnd in ArmEnds)
            armEnd.transform.SetParent(null);

        DetectionArea = GetComponent<DetectionArea>();
    }

    public List<Rigidbody> ArmEnds = new();

    public List<OctopusSticker> Stickers = new();
    public GameObject Target => Submarine.Instance.gameObject;

    public void Stick(OctopusSticker sticker)
    {
        if (cooldown > 0)
            return;

        if (Time.time < sticker.LastStick + StickDelay)
            return;
        sticker.LastStick = Time.time;
        var joint = sticker.AddComponent<SpringJoint>();
        sticker.Joint = joint;

        joint.connectedBody = Target.GetComponent<Rigidbody>();
        joint.spring = StickSpring;
        joint.breakForce = StickBreakForce;
        cooldown = 1f;
        StickSound?.Play(sticker.transform.position);
    }

    public void UnStick(OctopusSticker sticker)
    {
        UnStickSound?.Play(sticker.transform.position);
        cooldown = 1;
    }

    bool chasing;
    void FixedUpdate()
    {
        chasing = DetectionArea.Inside;

        cooldown -= Time.deltaTime;
        var stickCount = Stickers.Count(x => x.Joint != null);

        Move(chasing ? Target.transform.position : startPoint);

        Arms();

        float targetsY = Mathf.Clamp( navPathSolver.targetPos.y, -100, DetectionArea.MaxY);
        float targetY = stickCount >= 1 ? transform.position.y - DragDownSpeed : targetsY;

        Depth(targetY);

        StickLoop.volume = Mathf.Lerp(StickLoop.volume, stickCount > 0 ? 1 : 0, StickLoopSmooth * Time.deltaTime);
    }

    void Depth(float targetY)
    {
        float displacement = targetY - transform.position.y;

        float force =
            (displacement * DepthSpring) -
            (rigidBody.velocity.y * DepthDamp);

        rigidBody.AddForce(force * Vector3.up);
    }

    void Arms()
    {
        if (!chasing)
            return;

        var armsDistantOrdered = ArmEnds.OrderBy(x => Vector3.Distance(x.transform.position, Target.transform.position));
        for (int i = 0; i < Mathf.Min( ArmEnds.Count, 2); i++)
        {
            if (cooldown > 0)
                break;
            var armEnd = armsDistantOrdered.ElementAt(i);

            var dir = (Target.transform.position - armEnd.transform.position).normalized;
            if (Vector3.Distance(transform.position, armEnd.transform.position) > MaxArmDis)
                continue;

            armEnd.AddForce(dir * ArmSpeed);
        }
    }
    public float MoveInterval = 0.5f;
    float nextMove;
    void Move(Vector3 targetPos)
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

        if (Time.time < nextMove)
            return;

        nextMove = Time.time + MoveInterval;

        rigidBody.AddForce(transform.forward * Speed);

        foreach(var end in ArmEnds)
        {
            end.AddForce(-transform.forward * Speed);
        }
    }
}
