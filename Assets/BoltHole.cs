using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltHole : MonoBehaviour
{
    public Bolt Bolt;

    public bool DoFreeBolt;

    public float RandomForce = 10f;

    public Submarine Player;

    public float EmissionRate = 30f;

    public ParticleSystem ParticleSystem;

    public float Heal = 10f;

    void Update()
    {
        if (DoFreeBolt)
        {
            FreeBolt();
            DoFreeBolt = false;
        }

        var yourParticleEmission = ParticleSystem.emission;

        yourParticleEmission.enabled = Bolt == null && Player.Health <= 50 && Player.transform.position.y < Player.WaterLevel ;
    }

    public void FreeBolt()
    {
        if (Bolt == null)
            return;

        Bolt.Free = true;
        var rb = Bolt.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.AddForce(Random.Range(0, RandomForce), Random.Range(0, RandomForce), 0);
        Bolt = null;
    }

    public void PlaceBolt(Bolt bolt)
    {
        if (bolt == null)
            return;
        bolt.Free = false;
        Bolt = bolt;
        var rb = Bolt.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        bolt.transform.position = transform.position;
        Player.Health += Heal;
        Player.Health = Mathf.Clamp(Player.Health, 0, 100);
        Player.lastBoltHealth = Player.Health;
    }
}
