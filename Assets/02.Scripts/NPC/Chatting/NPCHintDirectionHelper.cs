using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 플레이어 기준으로 오브젝트의 상대 방향을 자연어로 변환하는 유틸리티
public static class NPCHintDirectionHelper
{
    // 기준 위치/방향을 바탕으로 대상의 상대 방향을 반환
    // 예: 앞쪽, 오른쪽, 왼쪽 뒤, 뒤쪽
    public static string GetRelativeDirection(Transform 기준점, Vector3 targetPosition)
    {
        // 기준점에서 대상까지의 방향 벡터
        Vector3 toTarget = targetPosition - 기준점.position;

        // 높이 차이는 무시하고 평면(XZ) 기준으로만 판단
        toTarget.y = 0f;

        // 너무 가까우면 방향보다 "바로 근처"처럼 말하는 편이 자연스러움
        if (toTarget.sqrMagnitude < 0.01f)
            return "바로 근처";

        toTarget.Normalize();

        // 기준점의 앞 / 오른쪽 벡터
        Vector3 forward = 기준점.forward;
        Vector3 right = 기준점.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        // 내적을 이용해서 앞뒤 / 좌우 성분 계산
        float forwardDot = Vector3.Dot(forward, toTarget);
        float rightDot = Vector3.Dot(right, toTarget);

        // 8방향처럼 세분화
        // 값이 클수록 해당 방향 성분이 강함
        const float diagonalThreshold = 0.35f;
        const float straightThreshold = 0.75f;

        // 정면 / 후면 위주
        if (forwardDot >= straightThreshold)
        {
            if (rightDot >= diagonalThreshold) return "오른쪽 앞";
            if (rightDot <= -diagonalThreshold) return "왼쪽 앞";
            return "앞쪽";
        }

        if (forwardDot <= -straightThreshold)
        {
            if (rightDot >= diagonalThreshold) return "오른쪽 뒤";
            if (rightDot <= -diagonalThreshold) return "왼쪽 뒤";
            return "뒤쪽";
        }

        // 좌우 위주
        if (rightDot > 0f) return "오른쪽";
        return "왼쪽";
    }

    // 거리 표현을 자연어로 변환
    public static string GetDistanceText(float distance)
    {
        if (distance < 2f) return "아주 가까이";
        if (distance < 10f) return "가까운 곳에";
        if (distance < 30f) return "조금 떨어진 곳에";
        return "꽤 멀리";
    }
}
