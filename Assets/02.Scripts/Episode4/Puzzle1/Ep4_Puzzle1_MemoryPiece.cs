using System;
using UnityEngine;
public class Ep4_Puzzle1_MemoryPiece : MonoBehaviour
{
    public Ep4_Puzzle1Manager manager;
    private readonly string playerTag = "Player";
    private bool isCollected = false;
    public event Action collectMemory;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag(playerTag))
        {
            if (isCollected) return;
            isCollected = true;
            collectMemory?.Invoke();
            gameObject.SetActive(false);
        }
    }
}