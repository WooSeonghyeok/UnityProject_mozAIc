using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public class InteractManager : MonoBehaviour
{
    public static InteractManager Instance;
    private PlayerInput user;
    private PlayerMovement userMove;
    private Collider col;
    private readonly string openingMidTag = "OpeningMid";
    public Action OpeningMid;
    private readonly string openingExitTag = "OpeningExit";
    public Action OpeningGoal;
    public Checkpoint_Plane cpPlace;
    private readonly string sSelectTag = "StageSelect";
    public event Action<bool> StageSelectOpen;
    private readonly string aSelectTag = "AbilitySelect";
    public event Action<bool> AbilitySelectOpen;
    private readonly string gateTag = "Checkpoint";
    private bool gateContact = false;
    private readonly string saveTag = "SavePoint";
    private bool saveContact = false;
    private readonly string puzzle4RetryTag = "Puzzle4Retry";
    public event Action puzzle4Hint;
    private readonly string goalTag = "GoalPoint";
    public event Action puzzle4Goal;
    private readonly string stage4EndTag = "Stage4EndPoint";
    public event Action stage4End;
    private readonly string endTag = "EndingPoint";
    public event Action gameEnd;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        col = GetComponent<Collider>();
        user = GetComponent<PlayerInput>();
        userMove = GetComponent<PlayerMovement>();
    }
    private void OnEnable()
    {
        if (user != null) user.Interact += UseCheckPoint;
        if (user != null) user.Interact += UseSavePoint;
    }
    private void OnDisable()
    {
        if (user != null) user.Interact -= UseCheckPoint;
        if (user != null) user.Interact -= UseSavePoint;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(openingMidTag))
        {
            OpeningMid?.Invoke();
        }
        if (other.gameObject.CompareTag(openingExitTag))
        {
            OpeningGoal?.Invoke();
        }
        if (other.gameObject.CompareTag(sSelectTag))
        {
            StageSelectOpen?.Invoke(true);
        }
        if (other.gameObject.CompareTag(aSelectTag))
        {
            AbilitySelectOpen?.Invoke(true);
        }
        if (other.CompareTag(puzzle4RetryTag))
        {
            puzzle4Hint?.Invoke();
        }
        if (other.gameObject.CompareTag(gateTag))
        {
            gateContact = true;
            cpPlace = other.GetComponentInParent<Checkpoint_Plane>();
        }
        if (other.gameObject.CompareTag(goalTag))
        {
            Puzzle4Manager.instance.GetComponent<Collider>().isTrigger = false;
            puzzle4Goal?.Invoke();
        }
        if (other.gameObject.CompareTag(stage4EndTag))
        {
            stage4End?.Invoke();
        }
        if (other.gameObject.CompareTag(endTag))
        {
            gameEnd?.Invoke();
        }
        if (other.gameObject.CompareTag(saveTag))
        {
            saveContact = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(sSelectTag))
        {
            StageSelectOpen?.Invoke(false);
        }
        if (other.gameObject.CompareTag(aSelectTag))
        {
            AbilitySelectOpen?.Invoke(false);
        }
        if (other.gameObject.CompareTag(gateTag))
        {
            gateContact = false;
            cpPlace = null;
        }
        if (other.gameObject.CompareTag(saveTag))
        {
            saveContact = false;
        }
    }
    public void UseCheckPoint()
    {
        if (!gateContact) return;
        if (cpPlace != null)
        {
            StageSelectionData.SelectedStage = 3;
            StageSelectionData.SelectedCP = cpPlace.cpNum;
        }
        SceneManager.LoadSceneAsync("LobbyScene");
    }
    public void UseSavePoint()
    {
        if (!saveContact) return;
        SaveUIManager.instance.OpenSavePopup();
    }
}