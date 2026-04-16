using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// NPC 힌트 요청 감지 및 힌트 컨텍스트 생성용 클래스
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

    // 현재 NPC / 씬 / 플레이어 주변 힌트 오브젝트 정보를 바탕으로
    // AI가 자연스럽게 힌트를 줄 수 있도록 프롬프트용 컨텍스트를 생성
    public static string BuildHintContext(NPCData npcData)
    {
        var db = GameDialogueDatabase.Instance;
        var profile = db.GetNpcProfile(npcData.npcId);
        var scene = db.GetSceneContext(npcData.sceneId);

        string goal = scene != null ? scene.goal : "";
        string npcName = profile != null ? profile.name : "NPC";

        // 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("[힌트 요청]");
        sb.AppendLine($"NPC: {npcName}");
        sb.AppendLine($"현재 목표: {goal}");

        // 플레이어가 있으면 주변 힌트 오브젝트들의 상대 위치를 제공
        if (player != null)
        {
            Transform playerTr = player.transform;

            // 씬 내 모든 힌트 대상 검색
            NPCHintTarget[] hintTargets = GameObject.FindObjectsOfType<NPCHintTarget>();

            if (hintTargets != null && hintTargets.Length > 0)
            {
                sb.AppendLine("현재 보이는 힌트 대상 정보:");

                foreach (NPCHintTarget target in hintTargets)
                {
                    // 비활성 오브젝트나 힌트 제외 대상은 건너뜀
                    if (target == null || !target.includeInHint || !target.gameObject.activeInHierarchy)
                        continue;

                    float distance = Vector3.Distance(playerTr.position, target.transform.position);

                    // 상대 방향 계산
                    string directionText = NPCHintDirectionHelper.GetRelativeDirection(playerTr, target.transform.position);

                    // 거리 표현 계산
                    string distanceText = NPCHintDirectionHelper.GetDistanceText(distance);

                    // AI가 쉽게 쓰도록 구조화된 문장 제공
                    if (string.IsNullOrEmpty(target.description))
                    {
                        sb.AppendLine($"- {target.targetName}: 플레이어의 {directionText}, {distanceText}");
                    }
                    else
                    {
                        sb.AppendLine($"- {target.targetName}: 플레이어의 {directionText}, {distanceText}, 설명: {target.description}");
                    }
                }
            }
        }

        // AI 말투 제약 추가
        sb.AppendLine("규칙:");
        sb.AppendLine("- 정답을 직접 말하지 말고, 사람처럼 자연스럽게 방향을 설명한다.");
        sb.AppendLine("- 방향은 '오른쪽', '왼쪽', '앞쪽', '뒤쪽', '오른쪽 앞'처럼 말한다.");
        sb.AppendLine("- 좌표값이나 숫자 벡터는 말하지 않는다.");
        sb.AppendLine("- 한 번에 전부 알려주기보다, 필요한 만큼만 힌트를 준다.");

        return sb.ToString();
    }
}
