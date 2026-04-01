using UnityEngine;
public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;
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
        OnAbilitySelectOpen(false);
        OnStageSelectOpen(false);
    }
    private void OnEnable()
    {
        userAct.AbilitySelectOpen += OnAbilitySelectOpen;
        userAct.StageSelectOpen += OnStageSelectOpen;
    }
    private void OnAbilitySelectOpen(bool b)
    {
        aSelectOpen = b;
        AbilitySelectPanel.SetActive(aSelectOpen);
    }
    private void OnStageSelectOpen(bool b)
    {
        sSelectOpen = b;
        StageSelectPanel.SetActive(sSelectOpen);
    }
}
