using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTester : MonoBehaviour
{
    public Vector3 Speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Speed * Time.deltaTime;
    }
}
