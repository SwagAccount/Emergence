using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UIElements.Experimental;
using Unity.VisualScripting;

public class Gulper : MonoBehaviour
{
    public float DepthSpring = 5f;
    public float DepthDamp = 6f;
    public float Speed = 5;
    public SoundEvent GulperDeath;

    private DetectionArea detectionArea;
    Vector3 startPos;

    Rigidbody rigidBody;
    NavPathSolver navPathSolver;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        detectionArea = GetComponent<DetectionArea>();
        navPathSolver = GetComponent<NavPathSolver>();
        rigidBody = GetComponent<Rigidbody>();
    }
    public GameObject Target => Submarine.Instance.gameObject;
    void FixedUpdate()
    {
        var chasing = detectionArea.Inside;

        var targetPos = chasing ? Target.transform.position : startPos;
        var targetY = -100f;
        Move(targetPos);
        targetY = targetPos.y;

        Depth(targetY - 0.5f);

        if (transform.position.y > Target.transform.position.y + 0.5f)
            transform.position = new Vector3(transform.position.x, Target.transform.position.y - 0.5f, transform.transform.position.z);
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
        var dir = targetPos - transform.position;
        rigidBody.AddForce(dir.normalized * Speed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Player"))
            return;

        if (Submarine.Instance.Eaten)
            return;

        Submarine.Instance.Eaten = true;
        GulperDeath.Play();
    }
}
