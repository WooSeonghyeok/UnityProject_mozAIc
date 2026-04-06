using UnityEngine;
public class EP4_Puzzle2Manager : MonoBehaviour
{
    public GameObject obj1;
    public GameObject obj2;
    public GameObject gatePrefab; // 🔥 문
    bool isActivated = false;
    private void Awake()
    {
        gatePrefab.SetActive(true);
    }
    void Update()
    {
        if (isActivated) return;
        if (obj1.activeSelf && obj2.activeSelf)  // 🔥 전부 활성화됐는지 체크
        {
            ActivatePortal();
            isActivated = true;
        }
    }
    void ActivatePortal()
    {
        if (gatePrefab != null)  gatePrefab.SetActive(false);  // 🔥 문 비활성화해서 개방
    }
}