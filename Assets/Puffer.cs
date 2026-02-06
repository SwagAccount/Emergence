using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using static UnityEngine.GraphicsBuffer;

public class Puffer : MonoBehaviour
{
    public float RotationSpeed = 20;
    public float RotationDamp = 1;

    public float DepthSpring = 5f;
    public float DepthDamp = 6f;

    public float Speed = 5;

    public float AttackDistance = 2f;

    public float PuffDistance = 0.8f;
    public float PuffSpeed = 0.5f;

    public float Damage = 15f;
    public float ExplosionDistance = 0.8f;
    public float ExplosionForce= 100f;

    public AudioSource PuffSound;
    public SoundEvent ExplosionSound;

    public float WaterLevel = 3.74f;

    public GameObject Explosion;

    Rigidbody rigidBody;
    Animator animator;

    public void Explode()
    {
        Instantiate(Explosion, transform.position, transform.rotation);
        var dir = Submarine.Instance.transform.position - transform.position;
        if (dir.magnitude < ExplosionDistance)
        {
            Submarine.Instance.Health -= Damage;
            Submarine.Instance.rigidBody.AddForce(dir.normalized * ExplosionForce);
        }
        ExplosionSound.Play(transform.position);
        Destroy(gameObject);
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
    }
    float puff;
    private void FixedUpdate()
    {
        PuffSound.volume = puff;
        var targetPos = Submarine.Instance.transform.position;
        targetPos.y = 0;
        var pos = transform.position;
        pos.y = 0;

        rigidBody.useGravity = transform.position.y > WaterLevel;

        
        var dis = Vector3.Distance(pos, targetPos);

        animator.SetBool("Puff", dis < PuffDistance);

        puff += (dis < PuffDistance ? 1 : -1) * PuffSpeed * Time.deltaTime;
        puff = Mathf.Clamp(puff, 0, 100);

        if (puff > 1)
        {
            Explode();
            return;
        }

        if (Vector3.Distance(pos, targetPos) > AttackDistance)
            return;

        float displacement = Mathf.Clamp( Submarine.Instance.transform.position.y, 2.6f, 100) - transform.position.y;

        float force =
            (displacement * DepthSpring) -
            (rigidBody.velocity.y * DepthDamp);

        rigidBody.AddForce(force * Vector3.up);

        var prevRot = transform.rotation;
        transform.LookAt(targetPos);
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
