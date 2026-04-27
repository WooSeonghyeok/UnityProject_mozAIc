using UnityEngine;

public class OrbitEffect : MonoBehaviour
{
    public Transform center;
    public float speed = 100f;

    void Update()
    {
        transform.RotateAround(center.position, Vector3.forward, speed * Time.deltaTime);
    }
}