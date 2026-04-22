using System.Collections.Generic;
using UnityEngine;

public class Episode2ScoreManager : MonoBehaviour
{
    public static Episode2ScoreManager Instance;

    [Header("Puzzle Score (감점용)")]
    public int spaceScore = 5;
    public int paintScore = 5;

    [Header("NPC Score")]
    public int npcScore = 0;

    [Header("Interaction Score ⭐ 추가")]
    public int interactionScore = 0;

    private HashSet<string> usedKeywords = new HashSet<string>();

    public SaveDataObj CurData;

    [Header("✨ Keyword Effect")]
    public GameObject keywordEffectPrefab;
    public Transform playerTransform;

    void Awake()
    {
        // ⭐ 싱글톤
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

    // 🧠 NPC 점수 (중복 방지 + 이펙트🔥)
    public void AddKeywordScore(MemoryKeyword keyword)
    {
        if (string.IsNullOrEmpty(keyword.word)) return;

        if (!usedKeywords.Contains(keyword.word))
        {
            usedKeywords.Add(keyword.word);
            npcScore += keyword.memoryRate;

            // ⭐ 이펙트 실행
            PlayKeywordEffect();
        }
    }

    // ⭐ 이펙트 함수
    void PlayKeywordEffect()
    {
        if (keywordEffectPrefab == null || playerTransform == null) return;

        for (int i = 0; i < 5; i++) // 여러 개 터지게
        {
            Vector3 offset = Random.insideUnitSphere * 0.5f;
            offset.y = Mathf.Abs(offset.y);

            GameObject effect = Instantiate(
                keywordEffectPrefab,
                playerTransform.position + offset + Vector3.up * 1.2f,
                Quaternion.identity
            );

            Destroy(effect, 2f);
        }
    }

    // ⭐ 상호작용 점수 추가
    public void AddInteractionScore(int value = 1)
    {
        interactionScore = Mathf.Min(5, interactionScore + value);

        if (CurData != null)
        {
            CurData.memory_reconstruction_rate[6] = interactionScore; // 에피소드 2_감정 점수 구획에 저장 처리
        }
    }

    // ⭐ 퍼즐 점수 저장
    public void Ep2_PuzzleScore()
    {
        if (CurData != null)
        {
            CurData.memory_reconstruction_rate[5] = spaceScore + paintScore;
        }
    }

    // ⭐ 총 점수
    public int GetTotalScore()
    {
        return spaceScore + paintScore + npcScore + interactionScore;
    }
}