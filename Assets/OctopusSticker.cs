using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctopusSticker : MonoBehaviour
{
    public Octopus Octopus;
    public Joint Joint;
    public float LastStick;
    private void OnCollisionEnter(Collision collision)
    {
        if (Joint != null)
            return;

        if (!collision.body.CompareTag("Player"))
            return;
        Octopus.Stick(this);
    }

    private bool lastStuck;
    private void Update()
    {
        if (Joint == null && lastStuck)
        {
            Octopus.UnStick(this);
        }
        lastStuck = Joint != null;
    }
}
