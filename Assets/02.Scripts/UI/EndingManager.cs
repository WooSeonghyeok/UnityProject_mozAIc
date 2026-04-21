using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class EndingManager : MonoBehaviour
{
    private bool isCompleteEnding;
    [SerializeField] private SoundProfile trueEndingProfile;
    [SerializeField] private SoundProfile normalEndingProfile;
    public Image endingImage;
    public Sprite trueSprite;
    public Sprite normalSprite;
    public Sprite thankstoSprite;
    public GameObject endSkipButton;
    public WaitForSecondsRealtime canSkipWFS;
    public WaitForSecondsRealtime EndingDuration;
    public GameObject RegameButton;
    public GameObject AppEndButton;
    private Coroutine endingPlayCoroutine;  // 실행 중인 엔딩 코루틴 저장용
    private Coroutine cutsceneCoroutine;
    private TextboxCtrl_Ending cutscene;
    void Awake()
    {
        endingImage.enabled = true;
        endSkipButton.SetActive(false);
        RegameButton.SetActive(false);
        AppEndButton.SetActive(false);
        CtrlReset();
        cutscene = gameObject.GetComponent<TextboxCtrl_Ending>();
        canSkipWFS = new WaitForSecondsRealtime(5f);  //엔딩 시작 5초 후 스킵 가능
    }
    private void OnEnable()  //엔딩 신 활성화 시점에 트루엔딩 판정
    {
        int total = SaveManager.instance.TotalScore();
        bool ReconstructionRateCond = total >= EndingPoint();
        bool TagsCond = TagCnt();
        isCompleteEnding = ReconstructionRateCond && TagsCond;
    }
    public static int EndingPoint()
    {
        string path = Path.Combine(Application.dataPath, "Resources/Data/ending_rule.json");
        if (!File.Exists(path))
        {
            Debug.LogError("EndingRule.json 파일을 찾을 수 없습니다: " + path);
            return 80;
        }
        string json = File.ReadAllText(path);
        EndingRuleFile endingRule = JsonUtility.FromJson<EndingRuleFile>(json);
        if (endingRule == null || endingRule.EndingRule == null || endingRule.EndingRule.Count == 0)
        {
            Debug.LogError("EndingRule 데이터 파싱 실패");
            return 80;
        }
        int rate = endingRule.EndingRule[0].minReconstructionRate;
        return rate;
    }
    private static bool TagCnt()  //태그 수집 조건
    {
        if (SaveManager.instance.curData.CoreTag == null || SaveManager.instance.curData.CoreTag.Count == 0)
        {
            Debug.LogError("MemoryTag 리스트를 찾을 수 없습니다.");
            return true;
        }
        if (SaveManager.instance.curData.npcInformations == null || SaveManager.instance.curData.npcInformations.Count == 0)
        {
            Debug.LogError("NPC 리스트를 찾을 수 없습니다.");
            return true;
        }
        int a = 0;
        foreach (IsTagGet tag in SaveManager.instance.curData.CoreTag)
        {
            if (tag.tagGet) a++;
        }
        return (float)((float)a / (float)SaveManager.instance.curData.CoreTag.Count) >= 0.8f;
    }
    private IEnumerator Start()
    {
        if (isCompleteEnding) CompleteEnding();
        else NormalEnding();
        endingImage.enabled = true;
        endingPlayCoroutine = StartCoroutine(EndingPlay());  // EndingPlay 코루틴 시작 및 저장 (스킵 시 중단할 수 있도록)
        yield return canSkipWFS;
        endSkipButton.SetActive(true);
    }
    void CompleteEnding()
    {
        endingImage.sprite = trueSprite;
        ApplyEndingSoundProfile(trueEndingProfile);
        Debug.Log("TRUE ENDING!");
        if (cutscene != null) cutsceneCoroutine = StartCoroutine(cutscene.TrueEndCutscene());
        EndingDuration = new WaitForSecondsRealtime(20f);  //진 엔딩 시작 20초 후 종료
    }
    void NormalEnding()
    {
        endingImage.sprite = normalSprite;
        ApplyEndingSoundProfile(normalEndingProfile);
        Debug.Log("normal ending...");
        if (cutscene != null) cutsceneCoroutine = StartCoroutine(cutscene.NormalEndCutscene());
        EndingDuration = new WaitForSecondsRealtime(10f);  //노멀 엔딩 시작 10초 후 종료
    }
    IEnumerator EndingPlay()
    {
        yield return EndingDuration;
        EndingClear();
    }
    public void OnEndingSkip()
    {
        EndingStop();
        if (cutscene != null && cutscene._manager != null) cutscene._manager.CloseBox();
        EndingClear();
    }
    private void EndingClear()
    {
        EndingStop();
        CtrlReset();
        endingImage.sprite = thankstoSprite;
        endSkipButton.SetActive(false);
        RegameButton.SetActive(true);
        AppEndButton.SetActive(true);
    }
    void CtrlReset()
    {
    }

    private void ApplyEndingSoundProfile(SoundProfile profile)
    {
        if (SoundManager.Instance == null || profile == null)
        {
            return;
        }

        SoundManager.Instance.ApplySoundProfile(profile, false);
    }
    private void EndingStop()
    {
        if (endingPlayCoroutine != null)  // 실행 중인 엔딩 코루틴 중단
        {
            StopCoroutine(endingPlayCoroutine);
            endingPlayCoroutine = null;
        }
        if (cutsceneCoroutine != null)     // 실행 중인 컷씬(대사) 코루틴 중단
        {
            StopCoroutine(cutsceneCoroutine);
            cutsceneCoroutine = null;
        }
        if (cutscene != null && cutscene._manager != null)  cutscene._manager.CloseBox();  // 안전장치: 컷씬 내부에서 열려 있는 대화 박스가 있을 수 있으니 닫음
    }
    public void Regame()
    {
        SaveManager.instance.ResetCurData();
        SceneManager.LoadScene("StartScene");
    }
    public void END()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;  //에디터 종료
#else
        Application.Quit();  //앱 종료
#endif
    }
}
