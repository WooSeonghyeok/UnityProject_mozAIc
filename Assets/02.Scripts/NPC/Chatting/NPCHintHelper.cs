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

    public static string BuildHintContext(NPCData npcData)
    {
        var db = GameDialogueDatabase.Instance;
        var profile = db.GetNpcProfile(npcData.npcId);
        var scene = db.GetSceneContext(npcData.sceneId);

        string goal = scene != null ? scene.goal : "";
        string npcName = profile != null ? profile.name : "NPC";

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("[힌트 요청]");
        sb.AppendLine($"NPC: {npcName}");
        sb.AppendLine($"현재 목표: {goal}");

        if (player != null)
        {
            Transform playerTr = player.transform;

            NPCHintTarget[] hintTargets = GameObject.FindObjectsOfType<NPCHintTarget>();

            if (hintTargets != null && hintTargets.Length > 0)
            {
                // 거리 계산 결과를 담을 리스트 생성
                List<HintTargetInfo> sortedTargets = new List<HintTargetInfo>();

                foreach (NPCHintTarget target in hintTargets)
                {
                    // 비활성 오브젝트나 힌트 제외 대상은 건너뜀
                    if (target == null || !target.includeInHint || !target.gameObject.activeInHierarchy)
                        continue;

                    float distance = Vector3.Distance(playerTr.position, target.transform.position);

                    // 오브젝트와 거리 정보를 함께 저장
                    sortedTargets.Add(new HintTargetInfo
                    {
                        target = target,
                        distance = distance
                    });
                }

                // 플레이어와 가까운 오브젝트가 먼저 오도록 정렬
                sortedTargets.Sort((a, b) => a.distance.CompareTo(b.distance));

                sb.AppendLine("현재 보이는 힌트 대상 정보:");
                sb.AppendLine("아래 목록은 플레이어에게 가까운 순서대로 정렬되어 있다.");

                foreach (HintTargetInfo info in sortedTargets)
                {
                    NPCHintTarget target = info.target;
                    float distance = info.distance;

                    // 플레이어 기준 상대 방향 계산
                    string directionText = NPCHintDirectionHelper.GetRelativeDirection(
                        playerTr,
                        target.transform.position
                    );

                    // 거리 표현 계산
                    string distanceText = NPCHintDirectionHelper.GetDistanceText(distance);

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

        sb.AppendLine("규칙:");
        sb.AppendLine("- 가장 가까운 힌트 대상부터 먼저 말한다.");
        sb.AppendLine("- 정답을 직접 말하지 말고, 사람처럼 자연스럽게 방향을 설명한다.");
        sb.AppendLine("- 방향은 '오른쪽', '왼쪽', '앞쪽', '뒤쪽', '오른쪽 앞'처럼 말한다.");
        sb.AppendLine("- 좌표값이나 숫자 벡터는 말하지 않는다.");
        sb.AppendLine("- 한 번에 전부 알려주기보다, 가까운 대상 1~2개 정도만 먼저 알려준다.");

        return sb.ToString();
    }

    // 힌트 대상과 거리 정보를 묶어서 정렬하기 위한 내부 클래스
    private class HintTargetInfo
    {
        public NPCHintTarget target; // 힌트 대상 오브젝트
        public float distance;       // 플레이어와의 거리
    }
}
