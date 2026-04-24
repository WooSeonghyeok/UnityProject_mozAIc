using System.Collections;
using UnityEngine;
using static TextboxManager;

public class TextboxCtrl_Ep1 : MonoBehaviour
{
    public TextboxManager _manager;

    void Awake()
    {
        if (_manager == null)
            _manager = FindObjectOfType<TextboxManager>();
    }

    // ===============================
    // 🎬 Episode1 시작 텍스트
    // ===============================
    public IEnumerator Episode1Intro()
    {
        _manager.UserCtrl(false);

        yield return _manager.TalkSay(TalkType.system, " 방금까지 있던 곳이랑은 다른데 \n 여긴 또 어디야…", 2.5f);

        _manager.UserCtrl(true);
    }

    // ===============================
    // ⭐ 별 1 텍스트
    // ===============================
    //public IEnumerator Star1Dialogue()
    //{
    //    //GameManager.Instance.CutsceneMode(true);
    //    _manager.UserCtrl(false);

    //    yield return _manager.TalkSay(TalkType.system, " ", 2f);

    //    _manager.UserCtrl(true);
    //    //GameManager.Instance.CutsceneMode(false);
    //}

}