using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryTriggerPoint : MonoBehaviour
{
    readonly string playerTag = "Player";
    public SaveDataObj CurData;
    private bool isThisGet = false;  //1번만 획득하도록 하는 태그
    public int memoryNumber;  //기억 재구성 점수 번호
    public int memoryRateUp;  //기억 재구성 점수 값
    public string TagName;
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
            if (isThisGet) return;
            CurData.memory_reconstruction_rate[memoryNumber] += memoryRateUp;  //기억 재구성 점수 업
            isThisGet = true;
            if (isTagUsing)  //태그 획득 컷신 연출을 사용하는 경우
            {
                if (tagObj != null)  tagObj.SetActive(false);  //오브젝트 비활성화 연출
                if (cutscene != null)  StartCoroutine(cutscene.TalkSay(TextboxManager.TalkType.player, tagHint));  //컷신 말풍선 출력 연출
            }
        }
    }
}
