using System;
using UnityEngine;

public class Ep3_3InteractPoint : MonoBehaviour
{
    private readonly string playerTag = "Player";
    private PlayerInput user;

    [NonSerialized] private SaveDataObj currentSaveData;
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

        if (!isContact || currentSaveData.isFirstEnterAtEP3FinalTable)
        {
            return;
        }

        int newPoint = currentSaveData.memory_reconstruction_rate[7] + memoryRateUp;
        currentSaveData.memory_reconstruction_rate[7] = Math.Clamp(newPoint, 0, 10);
        currentSaveData.isFirstEnterAtEP3FinalTable = true;

        if (SaveManager.instance != null)
        {
            SaveManager.instance.curData = currentSaveData;
        }

        SaveManager.WriteCurJSON(currentSaveData);
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

        if (SaveManager.instance != null)
        {
            if (SaveManager.instance.curData == null)
            {
                SaveManager.instance.curData = SaveManager.ReadCurJSON();
            }

            currentSaveData = SaveManager.instance.curData;
        }
        else
        {
            currentSaveData = SaveManager.ReadCurJSON();
        }

        return user != null && currentSaveData != null;
    }
}
