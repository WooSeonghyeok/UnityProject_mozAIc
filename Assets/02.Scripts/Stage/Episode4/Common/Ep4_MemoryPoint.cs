using System;
using System.Collections;
using System.Net;
using UnityEngine;
public class Ep4_MemoryPoint : MonoBehaviour
{
    readonly string playerTag = "Player";
    public SaveDataObj CurData;
    enum TagNumber { 동료, 하모니, 삶 } ;
    [SerializeField] TagNumber TagText;
    public int memoryRateUp;  //기억 재구성 점수 값
    public TextboxManager cutscene;
    public string tagHint;
    private void Awake()
    {
        CurData = SaveManager.instance.curData;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            switch (TagText)
            {
                case TagNumber.동료:
                    if (CurData.isFirstEnterAtS3Tag1) return;
                    StartCoroutine(MemoryGet());
                    CurData.isFirstEnterAtS3Tag1 = true;
                    break;
                case TagNumber.하모니:
                    if (CurData.isFirstEnterAtS3Tag2) return;
                    StartCoroutine(MemoryGet());
                    CurData.isFirstEnterAtS3Tag2 = true;
                    break;
                case TagNumber.삶:
                    if (CurData.isFirstEnterAtS3Tag3) return;
                    StartCoroutine(MemoryGet());
                    CurData.isFirstEnterAtS3Tag3 = true;
                    break;
            }
        }
    }
    private IEnumerator MemoryGet()
    {
        int oldPoint = CurData.memory_reconstruction_rate[12];  //기억 획득 전 점수
        int newPoint = oldPoint + memoryRateUp;  //기억 재구성 점수 획득
        CurData.memory_reconstruction_rate[12] = Math.Clamp(newPoint, 0, 5);  //기억 획득 후 점수를 0점 ~ 5점 범위 내로 제한
        yield return cutscene.TalkSay(TextboxManager.TalkType.player, tagHint);  //컷신 말풍선 출력 연출
        gameObject.SetActive(false);
    }
}