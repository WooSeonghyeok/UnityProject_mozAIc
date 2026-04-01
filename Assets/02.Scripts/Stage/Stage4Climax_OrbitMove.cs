using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Stage4Climax_OrbitMove : MonoBehaviour
{
    public float rotSpd;
    public float dist;
    public GameObject memory;
    void FixedUpdate()
    {
        transform.Rotate(Vector3.up, rotSpd);
    }
}
