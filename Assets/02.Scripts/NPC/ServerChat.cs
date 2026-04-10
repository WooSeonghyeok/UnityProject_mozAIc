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
    private bool isSubmitBound;

    [Header("기억 재구성 키워드")]
    public MemoryKeyword[] words;

    private void Start()
    {
        openAI = new OpenAIApi();
        ResolveUiReferences();
        BindInputEvents();
        FocusInputField();
    }

    private void OnDestroy()
    {
        UnbindInputEvents();
    }

    private void OnEnterSubmit(string text)
    {
        ServerMessageSend();
    }

    private async void ServerMessageSend()
    {
        ResolveUiReferences();

        if (isWaiting) return;
        if (input == null) return;
        if (string.IsNullOrEmpty(input.text)) return;

        string msg = input.text;

        input.text = "";
        input.interactable = false;

        CreateMessage($"{userName} : {msg}", userColor);

        // 사용자 입력에서 긍정/부정 단어를 검사해 호감도 반영
        CheckWords(msg);

        string finalMsg = msg;

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
                Content = msg
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
            FinishChat();
            return;
        }

        string servermsg = res.Choices[0].Message.Content;

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

    private void FinishChat()
    {
        isWaiting = false;

        if (input != null)
        {
            input.interactable = true;
            FocusInputField();
        }
    }

    private void ServerChatMessage()
    {
        if (chatTextObject == null || contentParent == null)
        {
            Debug.LogWarning("[ServerChat] chatTextObject 또는 contentParent가 연결되지 않음");
            return;
        }

        serverTextObj = Instantiate(chatTextObject, contentParent);
        serverTextObj.SendText("생각 중...", Color.red);
        StartCoroutine(Scroll());
    }

    public void CreateMessage(string msg, Color textColor)
    {
        if (chatTextObject == null || contentParent == null)
        {
            Debug.LogWarning("[ServerChat] 채팅 UI 참조가 없어 메시지를 만들 수 없음");
            return;
        }

        ChatTextObject chatObj = Instantiate(chatTextObject, contentParent);
        chatObj.SendText(msg, textColor);
        StartCoroutine(Scroll());
    }

    private IEnumerator Scroll()
    {
        yield return null;

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    public void ChatReset()
    {
        ResolveUiReferences();

        if (contentParent == null)
        {
            Debug.LogWarning("[ServerChat] contentParent가 없어 채팅을 초기화할 수 없음");
            return;
        }

        chatMessages.Clear();

        foreach (Transform message in contentParent)
        {
            Destroy(message.gameObject);
        }

        if (input != null)
        {
            input.text = string.Empty;
            input.interactable = true;
        }

        CreateMessage("대화가 시작됩니다.", Color.black);
        FocusInputField();
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

    private void BindInputEvents()
    {
        if (isSubmitBound || input == null)
        {
            return;
        }

        input.onSubmit.AddListener(OnEnterSubmit);
        isSubmitBound = true;
    }

    private void UnbindInputEvents()
    {
        if (!isSubmitBound || input == null)
        {
            return;
        }

        input.onSubmit.RemoveListener(OnEnterSubmit);
        isSubmitBound = false;
    }

    private void FocusInputField()
    {
        if (input == null || !input.gameObject.activeInHierarchy)
        {
            return;
        }

        input.ActivateInputField();
    }

    private void ResolveUiReferences()
    {
        if (input != null && contentParent != null && scrollRect != null && AffinityText != null)
        {
            return;
        }

        Transform chatCanvas = FindChatCanvasRoot();
        if (chatCanvas == null)
        {
            return;
        }

        if (input == null)
        {
            input = chatCanvas.GetComponentInChildren<TMP_InputField>(true);
        }

        if (scrollRect == null)
        {
            scrollRect = chatCanvas.GetComponentInChildren<ScrollRect>(true);
        }

        if (contentParent == null)
        {
            Transform content = FindChildRecursive(chatCanvas, "Content");
            if (content != null)
            {
                contentParent = content;
            }
        }

        if (AffinityText == null)
        {
            Transform affinity = FindChildRecursive(chatCanvas, "Affinity Text");
            if (affinity != null)
            {
                AffinityText = affinity.GetComponent<TMP_Text>();
            }
        }
    }

    private Transform FindChatCanvasRoot()
    {
        GameObject chatCanvas = GameObject.Find("Canvas_NPC_Chat");
        if (chatCanvas != null)
        {
            return chatCanvas.transform;
        }

        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        foreach (Canvas canvas in canvases)
        {
            if (canvas != null && canvas.name == "Canvas_NPC_Chat")
            {
                return canvas.transform;
            }
        }

        return null;
    }

    private Transform FindChildRecursive(Transform parent, string targetName)
    {
        if (parent == null)
        {
            return null;
        }

        foreach (Transform child in parent)
        {
            if (child.name == targetName)
            {
                return child;
            }

            Transform nestedChild = FindChildRecursive(child, targetName);
            if (nestedChild != null)
            {
                return nestedChild;
            }
        }

        return null;
    }
}
