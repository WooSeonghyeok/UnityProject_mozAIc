using System;
using UnityEngine;
public class Ep3_3InteractPoint : MonoBehaviour
{
    readonly string playerTag = "Player";
    private PlayerInput user;
    public SaveDataObj CurData;
    public int memoryRateUp;  //기억 재구성 점수 값
    bool isContact = false;
    private void Awake()
    {
        user = GameObject.FindGameObjectWithTag(playerTag).GetComponent<PlayerInput>();
        CurData = SaveManager.instance.curData;
    }
    private void OnEnable()
    {
        if (user != null) user.Interact += GetMemoryPoint;
    }
    private void OnDisable()
    {
        if (user != null) user.Interact -= GetMemoryPoint;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            isContact = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            isContact = false;
        }
    }
    void GetMemoryPoint()
    {
        if (!isContact || CurData.isFirstEnterAtEP3FinalTable) return;
        int newPoint = CurData.memory_reconstruction_rate[7] + memoryRateUp;
        CurData.memory_reconstruction_rate[7] = Math.Clamp(newPoint, 0, 10);
        CurData.isFirstEnterAtEP3FinalTable = true;
    }
}