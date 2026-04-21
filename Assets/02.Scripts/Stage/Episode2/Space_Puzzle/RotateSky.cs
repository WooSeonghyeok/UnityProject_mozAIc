using UnityEngine;

public class RotateSky : MonoBehaviour
{
    public float speed = 2f;

    void Update()
    {
        transform.Rotate(0, speed * Time.deltaTime, 0);
    }
}