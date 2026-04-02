using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MemoryRevealStage
{
    None = 0,          // 아무것도 확신하지 못함
    FaintFeeling = 1,  // 낯설지만 이상하게 익숙함
    Partial = 2,       // 일부 장면과 감정만 떠오름
    Strong = 3,        // 관계를 거의 눈치챔
    Full = 4           // 이름/관계/핵심 기억을 전부 복원
}

public class NPCData : MonoBehaviour
{
    [Header("JSON 연결 ID")]
    public string npcId;
    public string sceneId;

    [Header("호감도 설정")]
    public bool UseAffinity = false;
    public int startAffinity = 50;
    public int Affinity;

    [Header("기억 공개 단계")]
    public MemoryRevealStage revealStage = MemoryRevealStage.FaintFeeling;

    [Header("현재 NPC 상태")]
    public string NpcName;

    [TextArea(5, 20)]
    public string CurrentPrompt;

    private void Start()
    {
        var profile = GameDialogueDatabase.Instance.GetNpcProfile(npcId);

        if (profile != null)
        {
            // 내부적으로는 실제 이름을 들고 있어도 됨
            NpcName = profile.displayName;
        }
        else
        {
            Debug.LogWarning($"[NPCData] npcId에 해당하는 프로필을 찾지 못함: {npcId}");
            NpcName = "Unknown NPC";
        }

        Affinity = startAffinity;

        // 시작 시 현재 공개 단계에 맞는 프롬프트 생성
        RefreshPrompt();
    }

    public void ChangeAffinity()
    {
        if (Affinity <= 0)
        {
            if (ChatNPCManager.instance != null)
            {
                ChatNPCManager.instance.EndNPCChat();
            }
            return;
        }

        // 호감도 변경 후 현재 단계 기준 프롬프트 재생성
        RefreshPrompt();
    }

    public void SetRevealStage(MemoryRevealStage newStage)
    {
        // 단계는 뒤로 가지 않게 처리
        if ((int)newStage < (int)revealStage)
            return;

        revealStage = newStage;
        RefreshPrompt();
    }

    public void RefreshPrompt()
    {
        CurrentPrompt = PromptBuilder.BuildPrompt(this);
    }
}
