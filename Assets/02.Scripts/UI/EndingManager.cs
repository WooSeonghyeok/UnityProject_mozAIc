using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class EndingManager : MonoBehaviour
{
    private bool isCompleteEnding;
    public Color trueColor = new Color(1f, 1f, 1f,1f);
    public Color normalColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    public SoundController soundCtrl_true;
    public SoundController soundCtrl_normal;
    public Image endingImage;
    public GameObject endSkipButton;
    public WaitForSecondsRealtime canSkipWFS;
    public WaitForSecondsRealtime EndingDuration;
    public Image thankstoImage;
    public GameObject RegameButton;
    public GameObject AppEndButton;
    private Coroutine endingPlayCoroutine;  // 실행 중인 엔딩 코루틴 저장용
    void Awake()
    {
        endingImage.enabled = true;
        thankstoImage.enabled = false;
        soundCtrl_true.gameObject.SetActive(false);
        soundCtrl_normal.gameObject.SetActive(false);
        endSkipButton.SetActive(false);
        RegameButton.SetActive(false);
        AppEndButton.SetActive(false);
        CtrlReset();
        canSkipWFS = new WaitForSecondsRealtime(5f);  //엔딩 시작 5초 후 스킵 가능
    }
    private void OnEnable()  //엔딩 신 활성화 시점에 트루엔딩 판정
    {
        bool ReconstructionRateCond = SaveManager.instance.curData.memory_reconstruction_rate >= EndingPoint();
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
        if (SaveManager.instance.curData.npcAffinity == null || SaveManager.instance.curData.npcAffinity.Count == 0)
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
        endingPlayCoroutine = StartCoroutine(EndingPlay());  // EndingPlay 코루틴 시작 및 저장 (스킵 시 중단할 수 있도록)
        yield return canSkipWFS;
        endSkipButton.SetActive(true);
    }
    void CompleteEnding()
    {
        soundCtrl_normal.gameObject.SetActive(false);
        soundCtrl_true.gameObject.SetActive(true);
        Debug.Log("TRUE ENDING!");
        endingImage.color = trueColor;
        thankstoImage.color = trueColor;
        EndingDuration = new WaitForSecondsRealtime(20f);  //진 엔딩 시작 20초 후 종료
    }
    void NormalEnding()
    {
        soundCtrl_true.gameObject.SetActive(false);
        soundCtrl_normal.gameObject.SetActive(true);
        Debug.Log("normal ending...");
        endingImage.color = normalColor;
        thankstoImage.color = normalColor;
        EndingDuration = new WaitForSecondsRealtime(10f);  //노멀 엔딩 시작 10초 후 종료
    }
    IEnumerator EndingPlay()
    {
        yield return EndingDuration;
        EndingClear();
    }
    void CtrlReset()
    {
        soundCtrl_true.gameObject.SetActive(false);
        soundCtrl_normal.gameObject.SetActive(false);
    }
    public void OnEndingSkip()
    {
        EndingStop();
        EndingClear();
    }
    private void EndingClear()
    {
        EndingStop();
        CtrlReset();
        endingImage.enabled = false;
        endSkipButton.SetActive(false);
        thankstoImage.enabled = true;
        RegameButton.SetActive(true);
        AppEndButton.SetActive(true);
    }
    private void EndingStop()
    {
        if (endingPlayCoroutine != null)  // 실행 중인 엔딩 코루틴 중단
        {
            StopCoroutine(endingPlayCoroutine);
            endingPlayCoroutine = null;
        }
    }
    public void Regame()
    {
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