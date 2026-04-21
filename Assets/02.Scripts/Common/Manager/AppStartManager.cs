using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
public class AppStartManager : MonoBehaviour
{
    public SaveDataObj curData;
    public GameObject CreditPopup;
    private void Awake()
    {
        CreditPopup.SetActive(false);
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
