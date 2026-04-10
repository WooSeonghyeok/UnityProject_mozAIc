using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class TextboxManager : MonoBehaviour
{
    private readonly string playerTag = "Player";
    public PlayerInput user;
    public PlayerMovement userMove;
    public enum TalkType { system, player, voice}
    public enum Talker { self, girl, painter, musician, core };
    public GameObject box_system;
    public GameObject box_player;
    public GameObject box_voice;
    public Text text_system;
    public Text text_player;
    public Text text_voice;
    public Text voice_Name;
    public WaitForSecondsRealtime oneSec = new(1f);
    void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            user = player.GetComponent<PlayerInput>();
            if (user == null) Debug.LogWarning("PlayerInput 컴포넌트를 찾을 수 없습니다.");
            userMove = player.GetComponent<PlayerMovement>();
            if (userMove == null) Debug.LogWarning("PlayerMovement 컴포넌트를 찾을 수 없습니다.");
        }
        else
        {
            Debug.Log("Player 오브젝트가 존재하지 않는 신입니다.");
        }
        box_system.SetActive(false);
        box_player.SetActive(false);
        box_voice.SetActive(false);
    }
    public void UserCtrl(bool b)  //유저 입력 적용 여부 컨트롤
    {
        if (user != null)
            user.enabled = b;
        if (userMove != null)
        {
            userMove.enabled = b;
            userMove.SetMoveLock(!b);
        }
    }
    public IEnumerator TalkSay(TalkType type, string say, Talker talk = Talker.self)
    {
        switch (talk)
        {
            case Talker.girl:       voice_Name.text = "luna";   voice_Name.color = Color.red;   break;
            case Talker.painter:    voice_Name.text = "elio";   voice_Name.color = Color.green; break;
            case Talker.musician:   voice_Name.text = "leon";   voice_Name.color = Color.blue;  break;
            case Talker.core:       voice_Name.text = "???";    voice_Name.color = Color.gray;  break;
            case Talker.self:       voice_Name.text = "";       voice_Name.color = Color.black; break;
        }
        switch (type)
        {
            case TalkType.system: text_system.text = say; box_system.SetActive(true); break;
            case TalkType.player: text_player.text = say; box_player.SetActive(true); break;
            case TalkType.voice: text_voice.text = say; box_voice.SetActive(true); break;
        }
        yield return oneSec;
        box_system.SetActive(false);
        box_player.SetActive(false);
        box_voice.SetActive(false);
    }
}