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
    public TextboxManager cutscene;
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
            SaveManager.instance.curData.memory_reconstruction_rate[12] += memoryRateUp;  //에피소드 4 감정 점수 태그
            tagBubble.SetActive(false);
            StartCoroutine(cutscene.TalkSay(TextboxManager.TalkType.player, tagHint));
            thisTagGet = true;
        }
    }
}
