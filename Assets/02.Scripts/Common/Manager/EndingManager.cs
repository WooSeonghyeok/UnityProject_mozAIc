using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class EndingManager : MonoBehaviour
{
    private bool isCompleteEnding;
    public SoundController soundCtrl_true;
    public SoundController soundCtrl_normal;
    public Image endingImage;
    public Sprite trueSprite;
    public Sprite normalSprite;
    public Sprite thankstoSprite;
    public GameObject endSkipButton;
    public WaitForSecondsRealtime canSkipWFS = new(5f);  //엔딩 스킵 가능 시간
    public WaitForSecondsRealtime EndingDuration = new(20f);  //엔딩 최대 재생 시간
    public GameObject RegameButton;
    public GameObject AppEndButton;
    private Coroutine endingPlayCoroutine;  // 실행 중인 엔딩 코루틴 저장용
    private Coroutine cutsceneCoroutine;
    private TextboxCtrl_Ending cutscene;
    void Awake()
    {
        endingImage.enabled = true;
        soundCtrl_true.gameObject.SetActive(false);
        soundCtrl_normal.gameObject.SetActive(false);
        endSkipButton.SetActive(false);
        RegameButton.SetActive(false);
        AppEndButton.SetActive(false);
        CtrlReset();
        cutscene = gameObject.GetComponent<TextboxCtrl_Ending>();
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
        //if (SaveManager.instance.curData.CoreTag == null || SaveManager.instance.curData.CoreTag.Count == 0)
        //{
        //    Debug.LogError("MemoryTag 리스트를 찾을 수 없습니다.");
        //    return true;
        //}
        //int a = 0;
        //foreach (IsTagGet tag in SaveManager.instance.curData.CoreTag)
        //{
        //    if (tag.tagGet) a++;
        //}
        //return (float)((float)a / (float)SaveManager.instance.curData.CoreTag.Count) >= 0.8f;
        return true;  //추후 업데이트로 태그 역할 결정 전 임시로 true 처리
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
        soundCtrl_normal.gameObject.SetActive(false);
        soundCtrl_true.gameObject.SetActive(true);
        Debug.Log("TRUE ENDING!");
        if (cutscene != null) cutsceneCoroutine = StartCoroutine(cutscene.TrueEndCutscene());
    }
    void NormalEnding()
    {
        endingImage.sprite = normalSprite;
        soundCtrl_true.gameObject.SetActive(false);
        soundCtrl_normal.gameObject.SetActive(true);
        Debug.Log("normal ending...");
        if (cutscene != null) cutsceneCoroutine = StartCoroutine(cutscene.NormalEndCutscene());
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
    public void EndingClear()
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
        soundCtrl_true.gameObject.SetActive(false);
        soundCtrl_normal.gameObject.SetActive(false);
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