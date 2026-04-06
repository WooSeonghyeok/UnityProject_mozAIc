using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class EndingManager : MonoBehaviour
{
    private AudioSource BGMSource;
    private bool isCompleteEnding;
    public AudioClip CompleteEndingBGM;
    public AudioClip NormalEndingBGM;
    public AudioClip ThankstoBGM;
    public Image endingImage;
    public GameObject endSkipButton;
    public WaitForSecondsRealtime canSkipWFS;
    public WaitForSecondsRealtime EndingDuration;
    public Image thankstoImage;
    public GameObject AppEndButton;
    private Coroutine endingPlayCoroutine;  // 실행 중인 엔딩 코루틴 저장용
    void Awake()
    {
        BGMSource = GetComponent<AudioSource>();
        endingImage.enabled = true;
        thankstoImage.enabled = false;
        endSkipButton.SetActive(false);
        AppEndButton.SetActive(false);
        canSkipWFS = new WaitForSecondsRealtime(5f);  //엔딩 시작 5초 후 스킵 가능
    }
    private void OnEnable()  //엔딩 신 활성화 시점에 트루엔딩 판정
    {
        bool ReconstructionRateCond = EndingConditionData.memory_reconstruction_rate >= EndingPoint();
        bool trueEndTags = EndingConditionData.shared_childhood && EndingConditionData.shared_dream
                    && EndingConditionData.unfinished_confession && EndingConditionData.self_voice;
        isCompleteEnding = ReconstructionRateCond && trueEndTags ;
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
        BGMSource.clip = CompleteEndingBGM;
        endingImage.color = new Color (1f,1f,1f,1f);
        EndingDuration = new WaitForSecondsRealtime(20f);  //엔딩 시작 20초 후 종료
    }
    void NormalEnding()
    {
        BGMSource.clip = NormalEndingBGM;
        endingImage.color = new Color(0.8f, 0.8f, 0.8f, 0.8f);
        EndingDuration = new WaitForSecondsRealtime(10f);  //엔딩 시작 10초 후 종료
    }
    IEnumerator EndingPlay()
    {
        BGMSource.Play();
        yield return EndingDuration;
        EndingClear();
    }
    public void OnEndingSkip()
    {
        EndingStop();
        EndingClear();
    }
    private void EndingClear()
    {
        EndingStop();
        if (BGMSource != null && BGMSource.isPlaying)  // 엔딩 BGM 중지
        {
            BGMSource.Stop();
        }
        endingImage.enabled = false;
        endSkipButton.SetActive(false);
        if (isCompleteEnding && ThankstoBGM != null)  //트루엔딩 후 BGM 재생
        {
            BGMSource.clip = ThankstoBGM;
            BGMSource.Play();
        }
        thankstoImage.enabled = true;
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
    public void END()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;  //에디터 종료
#else
        Application.Quit();  //앱 종료
#endif
    }
}