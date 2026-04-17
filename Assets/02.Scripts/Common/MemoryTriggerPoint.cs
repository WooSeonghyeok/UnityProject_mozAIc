using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryTriggerPoint : MonoBehaviour
{
    readonly string playerTag = "Player";
    public SaveDataObj CurData;
    private bool thisTagGet = false;
    public string TagName;
    public int memoryNumber;  //기억 재구성 점수 번호
    public int memoryRateUp;  //기억 재구성 점수 값
    public bool isTagUsing;
    public GameObject tagObj;
    public TextboxManager cutscene;
    public string tagHint;
    private void Awake()
    {
        if(tagObj != null) tagObj.SetActive(true);
        CurData = SaveManager.instance.curData;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            if (thisTagGet) return;
            CurData.memory_reconstruction_rate[memoryNumber] += memoryRateUp;  //기억 재구성 점수 업
            if (isTagUsing)
            {
                if (tagObj == null || cutscene == null) return;
                tagObj.SetActive(false);
                StartCoroutine(cutscene.TalkSay(TextboxManager.TalkType.player, tagHint));
            }
            thisTagGet = true;
        }
    }
}
