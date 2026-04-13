using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameDialogueDatabase : MonoBehaviour
{
    public static GameDialogueDatabase Instance;

    [Header("로드된 데이터")]
    public List<NpcProfileData> npcProfiles = new List<NpcProfileData>();
    public List<SceneContextData> sceneContexts = new List<SceneContextData>();
    public List<NpcPersonalityBuildData> personalityBuilds = new List<NpcPersonalityBuildData>();
    public List<DialogueData> dialogues = new List<DialogueData>();
    public AiPromptRuleData promptRule;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 씬이 바뀌어도 같은 데이터를 유지하고 싶으면 유지
        DontDestroyOnLoad(gameObject);

        LoadAllData();
    }

    private void LoadAllData()
    {
        npcProfiles = LoadArrayJson<NpcProfileDataList, NpcProfileData>("Data/npc_profile");
        sceneContexts = LoadArrayJson<SceneContextDataList, SceneContextData>("Data/scene_context");
        personalityBuilds = LoadArrayJson<NpcPersonalityBuildDataList, NpcPersonalityBuildData>("Data/npc_personality_build");
        dialogues = LoadArrayJson<DialogueDataList, DialogueData>("Data/dialogue");
        promptRule = LoadSingleJson<AiPromptRuleData>("Data/ai_prompt_rule");

        Debug.Log($"[GameDialogueDatabase] NPCProfile 로드 수: {npcProfiles.Count}");
        Debug.Log($"[GameDialogueDatabase] SceneContext 로드 수: {sceneContexts.Count}");
        Debug.Log($"[GameDialogueDatabase] PersonalityBuild 로드 수: {personalityBuilds.Count}");
        Debug.Log($"[GameDialogueDatabase] Dialogue 로드 수: {dialogues.Count}");
        Debug.Log($"[GameDialogueDatabase] PromptRule 로드 완료: {promptRule != null}");
    }

    private List<TItem> LoadArrayJson<TWrapper, TItem>(string resourcePath) where TWrapper : class
    {
        TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);

        if (textAsset == null)
        {
            Debug.LogError($"[GameDialogueDatabase] JSON 파일을 찾을 수 없음: {resourcePath}");
            return new List<TItem>();
        }

        // JsonUtility는 루트 배열을 바로 파싱하지 못하므로 items로 감싸서 파싱
        string wrappedJson = $"{{\"items\":{textAsset.text}}}";
        TWrapper wrapper = JsonUtility.FromJson<TWrapper>(wrappedJson);

        // 래퍼 타입별로 리스트 꺼내기
        if (wrapper is NpcProfileDataList npcList)
            return npcList.items as List<TItem>;

        if (wrapper is SceneContextDataList sceneList)
            return sceneList.items as List<TItem>;

        if (wrapper is NpcPersonalityBuildDataList buildList)
            return buildList.items as List<TItem>;

        if (wrapper is DialogueDataList dialogueList)
            return dialogueList.items as List<TItem>;

        Debug.LogError($"[GameDialogueDatabase] 지원하지 않는 래퍼 타입: {typeof(TWrapper).Name}");
        return new List<TItem>();
    }

    private T LoadSingleJson<T>(string resourcePath) where T : class
    {
        TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);

        if (textAsset == null)
        {
            Debug.LogError($"[GameDialogueDatabase] JSON 파일을 찾을 수 없음: {resourcePath}");
            return null;
        }

        return JsonUtility.FromJson<T>(textAsset.text);
    }

    public NpcProfileData GetNpcProfile(string npcId)
    {
        return npcProfiles.Find(x => x.npcId == npcId);
    }

    public SceneContextData GetSceneContext(string sceneId)
    {
        return sceneContexts.Find(x => x.sceneId == sceneId);
    }

    public NpcPersonalityBuildData GetPersonalityBuild(string npcId)
    {
        return personalityBuilds.Find(x => x.npcId == npcId);
    }

    public DialogueData GetDialogue(string speakerId, string dialogueType)
    {
        return dialogues.Find(x => x.speakerId == speakerId && x.dialogueType == dialogueType);
    }

    public List<DialogueData> GetDialoguesByScene(string sceneId)
    {
        return dialogues.FindAll(x => x.sceneId == sceneId);
    }

    // 같은 speakerId + dialogueType에 해당하는 대사들을 모두 반환
    public List<DialogueData> GetDialogues(string speakerId, string dialogueType)
    {
        return dialogues.FindAll(x => x.speakerId == speakerId && x.dialogueType == dialogueType);
    }

    // 같은 타입의 대사 중 하나를 랜덤으로 반환
    public DialogueData GetRandomDialogue(string speakerId, string dialogueType)
    {
        List<DialogueData> candidates = GetDialogues(speakerId, dialogueType);

        if (candidates == null || candidates.Count == 0)
            return null;

        int randomIndex = Random.Range(0, candidates.Count);
        return candidates[randomIndex];
    }
}
