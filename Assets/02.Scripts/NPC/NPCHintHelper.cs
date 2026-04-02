using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCHintHelper
{
    public static bool IsHintRequest(string msg)
    {
        if (string.IsNullOrEmpty(msg)) return false;

        msg = msg.ToLower();

        return msg.Contains("힌트")
            || msg.Contains("퍼즐")
            || msg.Contains("어떻게")
            || msg.Contains("뭘 해야")
            || msg.Contains("어디")
            || msg.Contains("순서")
            || msg.Contains("조합")
            || msg.Contains("별");
    }

    public static string BuildHintContext(NPCData npcData)
    {
        var db = GameDialogueDatabase.Instance;

        var profile = db.GetNpcProfile(npcData.npcId);
        var scene = db.GetSceneContext(npcData.sceneId);

        string goal = scene != null ? scene.goal : "";
        string npcName = profile != null ? profile.displayName : "NPC";

        return $"[힌트 요청]\n" +
               $"NPC: {npcName}\n" +
               $"현재 목표: {goal}\n" +
               $"정답을 말하지 말고 다음 행동을 유도해.";
    }
}
