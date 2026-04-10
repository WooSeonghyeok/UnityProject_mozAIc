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
    private const float DatabaseWaitTimeout = 3f;

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

    private Coroutine initializeRoutine;

    private void Start()
    {
        Affinity = startAffinity;
        initializeRoutine = StartCoroutine(CoInitializeNpcData());
    }

    private void OnDisable()
    {
        if (initializeRoutine != null)
        {
            StopCoroutine(initializeRoutine);
            initializeRoutine = null;
        }
    }

    private IEnumerator CoInitializeNpcData()
    {
        float elapsed = 0f;

        while (GameDialogueDatabase.EnsureAvailable() == null && elapsed < DatabaseWaitTimeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        ApplyNpcProfile();
        RefreshPrompt();
        initializeRoutine = null;
    }

    private void ApplyNpcProfile()
    {
        var database = GameDialogueDatabase.EnsureAvailable();

        if (database == null)
        {
            Debug.LogWarning($"[NPCData] GameDialogueDatabase를 찾지 못해 기본 이름을 사용함: {npcId}");
            NpcName = string.IsNullOrWhiteSpace(NpcName) ? "Unknown NPC" : NpcName;
            return;
        }

        var profile = database.GetNpcProfile(npcId);

        if (profile != null)
        {
            NpcName = profile.displayName;
            return;
        }

        Debug.LogWarning($"[NPCData] npcId에 해당하는 프로필을 찾지 못함: {npcId}");
        NpcName = "Unknown NPC";
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
        foreach (NPCInfo npc in SaveManager.instance.curData.npcInformations)  //호감도가 변경된 NPC를 세이브 데이터에서 찾아 호감도 갱신
        {
            if (npc.npcId == npcId) npc.Affinity = Affinity;
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
