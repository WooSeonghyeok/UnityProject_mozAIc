using UnityEngine;
public class EP0_LobbyManager : MonoBehaviour
{
    public static EP0_LobbyManager instance;
    private readonly string playerTag = "Player";
    InteractManager userAct;
    public GameObject AbilitySelectPanel;
    public bool aSelectOpen = false;
    public GameObject StageSelectPanel;
    public bool sSelectOpen = false;
    public Transform spawnPos;
    void Awake()
    {
        if (instance == null) instance = this;
        if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        userAct = GameObject.FindGameObjectWithTag(playerTag).GetComponent<InteractManager>();
        //AbilitySelectPanel.SetActive(false);
        StageSelectPanel.SetActive(false);
        GameManager.Instance.openPopupCnt = 0;
    }
    private void OnEnable()
    {
        //userAct.AbilitySelectOpen += OnAbilitySelectOpen;
        userAct.StageSelectOpen += OnStageSelectOpen;
    }
    private void OnDisable()
    {
        //userAct.AbilitySelectOpen -= OnAbilitySelectOpen;
        userAct.StageSelectOpen -= OnStageSelectOpen;
    }
    //private void OnAbilitySelectOpen(bool b)
    //{
    //    aSelectOpen = b;
    //    MouseStateRenew(b);
    //    AbilitySelectPanel.SetActive(aSelectOpen);
    //}
    private void OnStageSelectOpen(bool b)
    {
        sSelectOpen = b;
        MouseStateRenew(b);
        StageSelectPanel.SetActive(sSelectOpen);
    }

    private static void MouseStateRenew(bool b)
    {
        int addOpenPopupCnt = (b ? 1 : -1);
        GameManager.Instance.openPopupCnt += addOpenPopupCnt;
        GameManager.Instance.OnPopupChanged();
    }
}
