using UnityEngine;
using TMPro;

public class DebugScoreUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public SpaceScoreController spaceTimer;

    void Update()
    {
        if (Episode2ScoreManager.Instance == null) return;

        var score = Episode2ScoreManager.Instance;

        string timeText = "";

        if (spaceTimer != null)
        {
            float current = spaceTimer.CurrentTime;
            float remain = spaceTimer.RemainingTime;

            timeText =
                $"\nTime: {FormatTime(current)}" +
                $"\nNext -1 in: {FormatTime(remain)}";
        }

        scoreText.text =
            $"[DEBUG SCORE]\n" +
            $"Interaction: {score.interactionScore}\n" +   // ⭐ 변경
            $"Space: {score.spaceScore}\n" +
            $"Paint: {score.paintScore}\n" +
            $"NPC: {score.npcScore}\n" +
            $"TOTAL: {score.GetTotalScore()}" +
            timeText;
    }

    string FormatTime(float time)
    {
        int min = Mathf.FloorToInt(time / 60);
        int sec = Mathf.FloorToInt(time % 60);
        return $"{min:00}:{sec:00}";
    }
}