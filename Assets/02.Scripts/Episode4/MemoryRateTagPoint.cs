using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryRateTagPoint : MonoBehaviour
{
    readonly string playerTag = "Player";
    private bool thisTagGet = false;
    public string TagName;
    public int memoryRateUp;
    public GameObject tagBubble;
    public CutsceneManager cutscene;
    public string tagHint;
    private void Awake()
    {
        tagBubble.SetActive(true);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            if (thisTagGet) return;
            SaveManager.instance.curData.memory_reconstruction_rate += memoryRateUp;
            foreach (IsTagGet tag in SaveManager.instance.curData.RateTag)
            {
                if (tag.TagName == TagName) tag.tagGet = true;
            }
            tagBubble.SetActive(false);
            StartCoroutine(cutscene.TalkSay(CutsceneManager.TalkType.player, tagHint));
            thisTagGet = true;
        }
    }
}
