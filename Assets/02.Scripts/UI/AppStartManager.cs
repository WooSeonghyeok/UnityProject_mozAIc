using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class AppStartManager : MonoBehaviour
{
    public Button NewButton;
    public Button LoadButton;
    public Button OptionButton;
    public Button CreditButton;
    public SaveDataObj curData;
    public GameObject LoadPopup;
    public GameObject OptionPopup;
    public GameObject CreditPopup;
    public GameObject QuitPopup;
    private void Awake()
    {
        LoadPopup.SetActive(false);
        OptionPopup.SetActive(false);
        CreditPopup.SetActive(false);
        QuitPopup.SetActive(false);
        NewgameDataRenual();
    }
    public void StartNewGame()
    {
        NewgameDataRenual();
        SceneManager.LoadSceneAsync($"OpeningScene");
    }
    private void NewgameDataRenual()
    {
        string path = Path.Combine(Application.persistentDataPath, $"CurData.json");
        SaveManager.CreateCurData(path, curData);
    }
}
