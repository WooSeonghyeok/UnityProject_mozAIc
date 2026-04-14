using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatNPCManager : MonoBehaviour
{
    public static ChatNPCManager instance;

    [SerializeField] private GameObject chatPanel;
    [SerializeField] private ServerChat serverChat;
    [SerializeField] private FollowCamera followCam;

    [Header("플레이어 제어")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInput user;

    [Header("이벤트 말풍선 UI")]
    [SerializeField] private GameObject speechBubbleRoot;   
    [SerializeField] private TMP_Text speechBubbleText;     
    [SerializeField] private float speechBubbleDuration = 5f; 

    public bool isTalking = false;

    private Coroutine bubbleCoroutine;
    private PlayerInput subscribedUser;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;

        TryResolveRuntimeReferences();
    }
    private void OnEnable()
    {
        TryResolveRuntimeReferences();
        RefreshCancelSubscription();
    }
    private void OnDisable()
    {
        RefreshCancelSubscription(clearSubscription: true);
    }
    public void NpcPersonTalk(Transform pos, NPCData npcData)
    {
        if (playerMovement != null && playerMovement.IsMoveLocked)
        {
            Debug.Log("[ChatNPCManager] Player is locked, so NPC chat will not start.");
            return;
        }

        if (npcData == null)
        {
            Debug.LogError("[ChatNPCManager] NPCData가 null임");
            return;
        }

        var db = GameDialogueDatabase.Instance;

        if (db == null)
        {
            Debug.LogError("[ChatNPCManager] GameDialogueDatabase가 없음");
            return;
        }

        TryResolveRuntimeReferences();
        RefreshCancelSubscription();

        if (chatPanel == null || serverChat == null)
        {
            Debug.LogError("[ChatNPCManager] chatPanel 또는 serverChat 참조가 비어 있습니다.");
            return;
        }

        isTalking = true;
        chatPanel.SetActive(true);
        serverChat.ChatReset();

        // 현재 대화 중인 NPCData를 ServerChat에 전달
        serverChat.currentNpcData = npcData;

        // JSON 기반 프롬프트 생성
        string prompt = PromptBuilder.BuildPrompt(npcData);
        npcData.CurrentPrompt = prompt;

        // 디버깅용 로그
        Debug.Log($"[ChatNPCManager] 생성된 Prompt:\n{prompt}");

        // 프로필 정보 가져오기
        var profile = db.GetNpcProfile(npcData.npcId);

        if (profile == null)
        {
            Debug.LogError($"[ChatNPCManager] npcId에 해당하는 프로필이 없음: {npcData.npcId}");
            return;
        }

        // 채팅 표시 이름을 JSON displayName으로 설정
        serverChat.SetNpcSpeaker(profile.displayName);

        // 시스템 프롬프트 적용
        serverChat.NpcTypeChange(prompt);

        // 동굴 진입 이후에는 전용 대사 사용
        string dialogueType = "intro";

        // Full 단계가 되었으면 동굴 진입 여부보다 우선해서 clear 대사 사용
        if (npcData.revealStage == MemoryRevealStage.Full)
        {
            dialogueType = "clear";
        }
        else if (GameManager_Ep1.Instance != null && GameManager_Ep1.Instance.hasEnteredCave)
        {
            dialogueType = "cave_after_talk";
        }

        // intro 대사를 JSON에서 가져옴
        var introDialogue = db.GetDialogue(npcData.npcId, dialogueType);

        if (introDialogue != null)
        {
            serverChat.CreateMessage($"{profile.displayName} : {introDialogue.text}", Color.blue);
        }
        else
        {
            serverChat.CreateMessage($"{profile.displayName} : 안녕.", Color.blue);
        }

        // 대화 시작 시 플레이어 이동 잠금
        if (playerMovement != null)
        {
            playerMovement.SetMoveLock(true);
        }

        // 카메라 전환
        if (followCam != null)
        {
            followCam.isCamModePos = pos;
            followCam.isChatCamMode = true;
        }
        else
        {
            Debug.LogWarning("[ChatNPCManager] FollowCamera 참조가 없어 대화 카메라 전환을 건너뜁니다.");
        }
    }

    public void EndNPCChat()
    {
        if (isTalking == false) return;
        isTalking = false;
        if (followCam != null)
        {
            followCam.isChatCamMode = false;
        }
        if (chatPanel != null)
        {
            chatPanel.SetActive(false);
        }
        if (serverChat != null)
        {
            serverChat.ChatReset();
        }

        // 대화 종료 시 플레이어 이동 해제
        if (playerMovement != null)
        {
            playerMovement.SetMoveLock(false);
        }
        // 대화 종료 후 다시 따라오기 허용
        if (serverChat != null && serverChat.currentNpcData != null)
        {
            NPCFollower follower = serverChat.currentNpcData.GetComponent<NPCFollower>();
            if (follower != null)
            {
                // 동굴 진입 이후에는 다시 추적 시작하지 않음
                bool shouldResumeFollow = true;

                if (GameManager_Ep1.Instance != null && GameManager_Ep1.Instance.hasEnteredCave)
                {
                    shouldResumeFollow = false;
                }

                follower.SetFollow(shouldResumeFollow);
            }
        }
    }
    private void TryResolveRuntimeReferences()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (user == null)
            {
                user = player.GetComponent<PlayerInput>();
            }

            if (playerMovement == null)
            {
                playerMovement = player.GetComponent<PlayerMovement>();
            }
        }

        if (followCam == null)
        {
            followCam = FindObjectOfType<FollowCamera>();
        }
    }

    private void RefreshCancelSubscription(bool clearSubscription = false)
    {
        PlayerInput nextUser = clearSubscription ? null : user;
        if (subscribedUser == nextUser)
        {
            return;
        }

        if (subscribedUser != null)
        {
            subscribedUser.Cancel -= EndNPCChat;
        }

        subscribedUser = nextUser;

        if (subscribedUser != null && isActiveAndEnabled)
        {
            subscribedUser.Cancel += EndNPCChat;
        }
    }
    #region 말풍선 관련 메서드
    // 이벤트성 말풍선 출력
    public void PlayNpcBubbleDialogue(NPCData npcData, string dialogueType)
    {
        if (npcData == null)
        {
            Debug.LogWarning("[ChatNPCManager] 말풍선 출력 실패 - npcData가 null");
            return;
        }

        var db = GameDialogueDatabase.Instance;
        if (db == null)
        {
            Debug.LogWarning("[ChatNPCManager] 말풍선 출력 실패 - DB가 null");
            return;
        }

        var profile = db.GetNpcProfile(npcData.npcId);
        var dialogue = db.GetDialogue(npcData.npcId, dialogueType);

        if (profile == null || dialogue == null)
        {
            Debug.LogWarning($"[ChatNPCManager] 말풍선 대사 없음 - npcId: {npcData.npcId}, type: {dialogueType}");
            return;
        }

        if (speechBubbleRoot == null || speechBubbleText == null)
        {
            Debug.LogWarning("[ChatNPCManager] speechBubbleRoot 또는 speechBubbleText가 연결되지 않음");
            return;
        }

        // 이미 실행 중인 말풍선이 있으면 이전 코루틴 정지
        if (bubbleCoroutine != null)
        {
            StopCoroutine(bubbleCoroutine);
        }

        // NPC 이름 + 대사 표시
        speechBubbleText.text = $"{profile.displayName} : {dialogue.text}";
        speechBubbleRoot.SetActive(true);

        // 5초 뒤 자동 닫기 시작
        bubbleCoroutine = StartCoroutine(CoHideSpeechBubble());
    }

    // 말풍선 자동 닫기 코루틴
    private IEnumerator CoHideSpeechBubble()
    {
        yield return new WaitForSeconds(speechBubbleDuration);

        if (speechBubbleRoot != null)
        {
            speechBubbleRoot.SetActive(false);
        }

        bubbleCoroutine = null;
    }

    // 필요할 때 강제로 말풍선 닫기
    public void HideSpeechBubble()
    {
        if (bubbleCoroutine != null)
        {
            StopCoroutine(bubbleCoroutine);
            bubbleCoroutine = null;
        }

        if (speechBubbleRoot != null)
        {
            speechBubbleRoot.SetActive(false);
        }
    }
    #endregion
}
