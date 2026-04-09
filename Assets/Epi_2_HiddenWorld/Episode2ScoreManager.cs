using System.Collections.Generic;
using UnityEngine;

public class Episode2ScoreManager : MonoBehaviour
{
    public static Episode2ScoreManager Instance;

    // ✅ 클리어 점수 (고정)
    public int clearScore = 0;

    // ✅ 퍼즐 점수 (감점용)
    public int spaceScore = 5;
    public int paintScore = 5;

    // ✅ NPC 점수
    public int npcScore = 0;
    private HashSet<string> usedKeywords = new HashSet<string>();

    void Awake()
    {
        Instance = this;
    }

    // 🔥 클리어 점수 추가
    public void AddClearScore(int value)
    {
        clearScore += value;
    }

    // 🔵 Space 감점
    public void ReduceSpaceScore()
    {
        spaceScore = Mathf.Max(0, spaceScore - 1);
    }

    // 🎨 Paint 감점
    public void ReducePaintScore()
    {
        paintScore = Mathf.Max(0, paintScore - 1);
    }

    // 🧠 NPC 점수
    public void AddKeywordScore(string keyword)
    {
        if (!usedKeywords.Contains(keyword))
        {
            usedKeywords.Add(keyword);
            npcScore += 1;
        }
    }

    // ⭐ 최종 점수
    public int GetTotalScore()
    {
        return clearScore + spaceScore + paintScore + npcScore;
    }
}