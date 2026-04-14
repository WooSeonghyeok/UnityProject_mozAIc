using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using OpenAI;
using UnityEditor.MPE;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using System;

public class ServerChat : MonoBehaviour
{
    // 사용자가 입력창에 말을 입력하는 경우 메시지를 화면에 표시한 후 OpenAI 서버에 보내며
    // 서버 응답을 다시 화면에 출력하는 것
    [Header("UI")]
    [SerializeField] private TMP_InputField input;
    [SerializeField] private Transform contentParent;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private ChatTextObject chatTextObject;
    [SerializeField] private Color userColor = Color.black;
    [SerializeField] private Color chatColor = Color.white;

    private OpenAIApi openAI;
    private List<ChatMessage> chatMessages = new List<ChatMessage>();
    private bool isWaiting = false;

    [Header("Setting")]
    [SerializeField] private string userName = "나";
    [SerializeField] private string serverName = "NPC";
    [SerializeField] private string model = "helpy-pro";

    private ChatTextObject serverTextObj;
    public NPCData currentNpcData;

    [Header("호감도")]
    [SerializeField] private string[] positiveWords;
    [SerializeField] private string[] negativeWords;

    public int PositiveAffinity = 10;
    public int NegativeAffinity = -10;

    [SerializeField] private TMP_Text AffinityText;

    [Header("기억 재구성 키워드")]
    public MemoryKeyword[] words;

    private void Start()
    {
        openAI = new OpenAIApi();
        input.ActivateInputField();
        input.onSubmit.AddListener(OnEnterSubmit);
    }

    private void OnEnterSubmit(string text)
    {
        ServerMessageSend();
    }

    private async void ServerMessageSend()
    {
        if (isWaiting) return;
        if (string.IsNullOrEmpty(input.text)) return;

        string msg = input.text;

        input.text = "";
        input.interactable = false;

        CreateMessage($"{userName} : {msg}", userColor);

        // 사용자 입력에서 긍정/부정 단어를 검사해 호감도 반영
        CheckWords(msg);

        string finalMsg = msg;
        // 금지 주제 먼저 검사
        if (IsBannedTopic(msg))
        {
            string bannedFallback = GetFallbackText("banned_topic");
            CreateMessage($"{serverName} : {bannedFallback}", chatColor);
            FinishChat();
            return;
        }
        // 힌트 요청 감지
        if (NPCHintHelper.IsHintRequest(msg) && currentNpcData != null)
        {
            string hintContext = NPCHintHelper.BuildHintContext(currentNpcData);

            // 힌트 상황을 AI에게 강하게 전달
            finalMsg = $"{hintContext}\n\n플레이어 질문: {msg}";
        }

        chatMessages.Add(
            new ChatMessage
            {
                Role = "user",
                Content = finalMsg
            }
        );

        isWaiting = true;

        ServerChatMessage();

        CreateChatCompletionRequest rq = new CreateChatCompletionRequest
        {
            Model = model,
            Messages = chatMessages,
            Temperature = 1
        };

        CreateChatCompletionResponse res = default;

        try
        {
            res = await openAI.CreateChatCompletion(rq);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ServerChat] OpenAI 오류: {e.Message}");
            // 생각 중... 메시지를 fallback 문장으로 교체
            string timeoutFallback = GetFallbackText("api_timeout");
            serverTextObj.SendText($"{serverName} : {timeoutFallback}", chatColor);

            // assistant 응답도 기록에 남겨서 대화 흐름 유지
            chatMessages.Add(new ChatMessage
            {
                Role = "assistant",
                Content = timeoutFallback
            });

            FinishChat();
            return;
        }
        string servermsg = res.Choices[0].Message.Content;

        // 응답이 비었을 때 fallback 처리
        if (string.IsNullOrWhiteSpace(servermsg))
        {
            servermsg = GetFallbackText("empty_response");
        }
        serverTextObj.SendText($"{serverName} : {servermsg}", chatColor);

        // assistant 응답도 기록에 저장
        chatMessages.Add(
            new ChatMessage
            {
                Role = "assistant",
                Content = servermsg
            }
        );

        FinishChat();
    }

    // 금지 주제일 때 true 반환
    private bool IsBannedTopic(string msg)
    {
        if (currentNpcData == null || GameDialogueDatabase.Instance == null)
            return false;

        var db = GameDialogueDatabase.Instance;
        var profile = db.GetNpcProfile(currentNpcData.npcId);
        var scene = db.GetSceneContext(currentNpcData.sceneId);

        // NPC 프로필 금지 주제 검사
        if (profile != null && profile.bannedTopics != null)
        {
            foreach (string topic in profile.bannedTopics)
            {
                if (!string.IsNullOrEmpty(topic) && msg.Contains(topic))
                    return true;
            }
        }
        // 현재 씬 금지 주제 검사
        if (scene != null && scene.bannedTopics != null)
        {
            foreach (string topic in scene.bannedTopics)
            {
                if (!string.IsNullOrEmpty(topic) && msg.Contains(topic))
                    return true;
            }
        }

        return false;
    }

    // triggerType에 해당하는 fallback 문장 반환
    private string GetFallbackText(string triggerType)
    {
        if (GameDialogueDatabase.Instance == null || currentNpcData == null)
            return "……";

        AiFallbackData fallback = GameDialogueDatabase.Instance.GetFallback(
            triggerType,
            currentNpcData.npcId
        );

        if (fallback != null && !string.IsNullOrEmpty(fallback.text))
            return fallback.text;

        return "……";
    }

    private void FinishChat()
    {
        isWaiting = false;
        input.interactable = true;
        input.ActivateInputField();
    }

    private void ServerChatMessage()
    {
        serverTextObj = Instantiate(chatTextObject, contentParent);
        serverTextObj.SendText("생각 중...", Color.red);
        StartCoroutine(Scroll());
    }

    public void CreateMessage(string msg, Color textColor)
    {
        ChatTextObject chatObj = Instantiate(chatTextObject, contentParent);
        chatObj.SendText(msg, textColor);
        StartCoroutine(Scroll());
    }

    private IEnumerator Scroll()
    {
        yield return null;
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void ChatReset()
    {
        chatMessages.Clear();

        foreach (Transform message in contentParent)
        {
            Destroy(message.gameObject);
        }

        CreateMessage("대화가 시작됩니다.", Color.black);
    }

    public void NpcTypeChange(string prompt)
    {
        // 현재 NPC가 있으면 호감도 UI 갱신
        if (currentNpcData != null && AffinityText != null)
        {
            AffinityText.text = $"호감도 : {currentNpcData.Affinity}";
        }

        List<ChatMessage> removeList = new List<ChatMessage>();

        foreach (ChatMessage message in chatMessages)
        {
            if (message.Role == "system")
            {
                removeList.Add(message);
            }
        }

        foreach (ChatMessage message in removeList)
        {
            chatMessages.Remove(message);
        }

        ChatMessage systemMsg = new ChatMessage
        {
            Role = "system",
            Content = prompt
        };

        chatMessages.Insert(0, systemMsg);
    }

    public void SetNpcSpeaker(string npcName)
    {
        serverName = npcName;
    }

    public void CheckWords(string msg)
    {
        if (currentNpcData == null) return;
        foreach (MemoryKeyword keyword in words)
        {
            if (msg.ToLower().Contains(keyword.word.ToLower()))
            {
                Debug.Log($"[MemoryKeyword] 발견됨: {keyword.word}, isUsed={keyword.isUsed}");
                if (!keyword.isUsed)
                {
                    SaveManager.instance.curData.memory_reconstruction_rate += keyword.memoryRate;
                    keyword.isUsed = true;
                    Debug.Log($"[MemoryKeyword] 증가! 현재 memory_reconstruction_rate = {SaveManager.instance.curData.memory_reconstruction_rate}");
                }
                else
                {
                    Debug.Log($"[MemoryKeyword] 이미 사용됨. 증가 없음.");
                }
            }
        }
        if (currentNpcData.UseAffinity)
        {
            foreach (string word in positiveWords)
            {
                if (msg.Contains(word))
                {
                    currentNpcData.Affinity += PositiveAffinity;
                    if (AffinityText != null)
                    {
                        AffinityText.text = $"호감도 : {currentNpcData.Affinity}";
                    }
                    currentNpcData.ChangeAffinity();
                    UpdateAIType();
                    return;
                }
            }
            foreach (string word in negativeWords)
            {
                if (msg.Contains(word))
                {
                    currentNpcData.Affinity += NegativeAffinity;
                    if (AffinityText != null)
                    {
                        AffinityText.text = $"호감도 : {currentNpcData.Affinity}";
                    }
                    currentNpcData.ChangeAffinity();
                    UpdateAIType();
                    return;
                }
            }
        }
    }

    private void UpdateAIType()
    {
        if (currentNpcData == null) return;

        // 기존 SO 기반 성격 갱신 대신 PromptBuilder로 현재 상태 기준 프롬프트 재생성
        string systemType = PromptBuilder.BuildPrompt(currentNpcData);
        currentNpcData.CurrentPrompt = systemType;
        NpcTypeChange(systemType);
    }
}