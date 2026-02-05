using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public static List<Pickup> AllPickups = new();
    // Start is called before the first frame update
    Rigidbody rigidBody;

    [HideInInspector] public Joint joint;

    public string Name = "Item";

    public float waterLevel = 3.73f;
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        AllPickups.Add(this);
    }

    private void FixedUpdate()
    {
        rigidBody.useGravity = transform.position.y > waterLevel;
    }

    private void OnDestroy()
    {
        AllPickups.Remove(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (!AllPickups.Contains(this))
            AllPickups.Add(this);
    }
}
