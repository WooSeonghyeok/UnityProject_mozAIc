using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Vector3 offset = new Vector3(0f, 3f, -5f);

    public bool isChatCamMode = false;
    public Transform isCamModePos;

    [SerializeField] private float movespeed = 4f;
    [SerializeField] private float rotationSpeed = 4f;


    void Update()
    {
        if(isChatCamMode)
        {
            // 채팅모드
            ChatCamMove();
        }

    }
    private void ChatCamMove()
    {
        if (isCamModePos == null) return;

        this.transform.position = Vector3.Lerp(this.transform.position, isCamModePos.position, movespeed*Time.deltaTime);

        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, isCamModePos.rotation, rotationSpeed*Time.deltaTime);
    }
}
