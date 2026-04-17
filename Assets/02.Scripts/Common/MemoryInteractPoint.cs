using UnityEngine;
public class MemoryInteractPoint : MonoBehaviour
{
    readonly string playerTag = "Player";
    private PlayerInput user;
    public SaveDataObj CurData;
    bool isThisGet = false;  //1번만 획득하도록 하는 태그
    public int memoryNumber;  //기억 재구성 점수 번호
    public int memoryRateUp;  //기억 재구성 점수 값
    bool isContact = false;
    private void Awake()
    {
        user = GameObject.FindGameObjectWithTag(playerTag).GetComponent<PlayerInput>();
        CurData = SaveManager.instance.curData;
    }
    private void OnEnable()
    {
        if (user != null) user.Interact += GetMemoryPoint;
    }
    private void OnDisable()
    {
        if (user != null) user.Interact -= GetMemoryPoint;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            isContact = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            isContact = false;
        }
    }
    void GetMemoryPoint()
    {
        if (!isContact || isThisGet) return;
        CurData.memory_reconstruction_rate[memoryNumber] += memoryRateUp;  //기억 재구성 점수 업
        isThisGet = true;
    }
}