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
    public SaveDataObj CurData;
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
        CurData = SaveManager.instance.curData;
    }

    // 🔥 클리어 점수 추가
    public void AddClearScore(int value)
    {
        clearScore += value;
        CurData.memory_reconstruction_rate[4] = clearScore;  //클리어 점수를 Episode2의 관계 점수로 사용
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
    public void AddKeywordScore(MemoryKeyword keyword)
    {
        if (string.IsNullOrEmpty(keyword.word)) return;

        if (!usedKeywords.Contains(keyword.word))
        {
            usedKeywords.Add(keyword.word);
            npcScore += keyword.memoryRate;  //ServerChat에서 이미 점수를 계산했으므로, 여기서는 GetTotalScore에 출력하는 값으로만 사용
        }
    }

    public void Ep2_PuzzleScore()  //Space와 Paint 점수의 합을 Episode2의 퍼즐 점수로 사용
    {
        SaveManager.instance.curData.memory_reconstruction_rate[5] = spaceScore + paintScore;
    }

    // ⭐ 총 점수
    public int GetTotalScore()
    {
        return clearScore + spaceScore + paintScore + npcScore;
    }
}