using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChatTextObject : MonoBehaviour
{
    private TMP_Text chatText;

    private void OnEnable()
    {
        chatText = GetComponent<TMP_Text>();
    }

    public void SendText(string message, Color color)
    {
        if (chatText != null)
        {
            chatText.text = message;
            chatText.color = color;
        }
    }

}
