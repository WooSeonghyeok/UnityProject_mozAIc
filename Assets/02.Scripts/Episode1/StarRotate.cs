using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarRotate : MonoBehaviour
{
    public float rotateSpeed = 90f;

    private void Update()
    {
        // Y축으로 계속 회전
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
    }
}
