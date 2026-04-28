using System;
using UnityEngine;

public class Ep3_3InteractPoint : MonoBehaviour
{
    private readonly string playerTag = "Player";
    private PlayerInput user;

    public SaveDataObj CurData;
    public int memoryRateUp;  // 기억 재구성 점수 값

    private bool isContact = false;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
        if (user != null)
        {
            user.Interact += GetMemoryPoint;
        }
    }

    private void OnDisable()
    {
        if (user != null)
        {
            user.Interact -= GetMemoryPoint;
        }
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

    private void GetMemoryPoint()
    {
        if (!ResolveReferences())
        {
            return;
        }

        if (!isContact || CurData.isFirstEnterAtEP3FinalTable)
        {
            return;
        }

        int newPoint = CurData.memory_reconstruction_rate[7] + memoryRateUp;
        CurData.memory_reconstruction_rate[7] = Math.Clamp(newPoint, 0, 10);
        CurData.isFirstEnterAtEP3FinalTable = true;

        if (SaveManager.instance != null)
        {
            SaveManager.instance.curData = CurData;
        }

        SaveManager.WriteCurJSON(CurData);
    }

    private bool ResolveReferences()
    {
        if (user == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                user = player.GetComponent<PlayerInput>();
            }
        }

        if (CurData == null)
        {
            CurData = SaveManager.instance != null
                ? SaveManager.instance.curData
                : SaveManager.ReadCurJSON();
        }

        return user != null && CurData != null;
    }
}
