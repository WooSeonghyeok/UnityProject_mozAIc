using System.Collections.Generic;
using UnityEngine;

public class Episode2ScoreManager : MonoBehaviour
{
    public static Episode2ScoreManager Instance;

    [Header("Clear Score (고정 점수)")]
    public int clearScore = 0;

    [Header("Puzzle Score (감점용)")]
    public int spaceScore = 5;
    public int paintScore = 5;

    [Header("NPC Score")]
    public int npcScore = 0;

    private HashSet<string> usedKeywords = new HashSet<string>();

    void Awake()
    {
        // ⭐ 싱글톤 + DontDestroyOnLoad (핵심)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 🔥 클리어 점수 추가
    public void AddClearScore(int value)
    {
        clearScore += value;
    }

    // 🔵 Space 감점
    public void ReduceSpaceScore()
    {
        if (spaceScore <= 0) return;

        spaceScore = Mathf.Max(0, spaceScore - 1);
    }

    // 🎨 Paint 감점
    public void ReducePaintScore()
    {
        if (paintScore <= 0) return;

        paintScore = Mathf.Max(0, paintScore - 1);
    }

    // 🧠 NPC 점수 (중복 방지)
    public void AddKeywordScore(string keyword)
    {
        if (string.IsNullOrEmpty(keyword)) return;

        if (!usedKeywords.Contains(keyword))
        {
            usedKeywords.Add(keyword);
            npcScore += 1;
        }
    }

    // ⭐ 총 점수
    public int GetTotalScore()
    {
        return clearScore + spaceScore + paintScore + npcScore;
    }
}