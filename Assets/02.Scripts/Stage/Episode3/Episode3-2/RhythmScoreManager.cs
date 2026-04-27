using UnityEngine;

public enum RhythmJudgeGrade
{
    None,
    Bad,
    Good,
    Excellent,
    Wrong,
    Miss
}

public class RhythmScoreManager : MonoBehaviour
{
    [Header("플레이 정보")]
    [SerializeField] private int score = 0;
    [SerializeField] private int missCount = 0;
    [SerializeField] private int wrongCount = 0;
    [SerializeField] private int combo = 0;
    [SerializeField] private int badCount = 0;
    [SerializeField] private int goodCount = 0;
    [SerializeField] private int excellentCount = 0;
    [SerializeField] private RhythmJudgeGrade lastJudgeGrade = RhythmJudgeGrade.None;

    [Header("점수 설정")]
    [SerializeField] private int badScore = 50;
    [SerializeField] private int goodScore = 100;
    [SerializeField] private int excellentScore = 150;
    [SerializeField] private int pointsPerBonusLife = 10000;
    [SerializeField] private int bonusLivesEarned = 0;

    public int Score => score;
    public int MissCount => missCount;
    public int WrongCount => wrongCount;
    public int Combo => combo;
    public int BadCount => badCount;
    public int GoodCount => goodCount;
    public int ExcellentCount => excellentCount;
    public RhythmJudgeGrade LastJudgeGrade => lastJudgeGrade;
    public int BonusLivesEarned => bonusLivesEarned;

    // 점수 관련 상태를 초기값으로 리셋한다.
    // 새 퍼즐 시작 시 호출된다.
    public void ResetState()
    {
        score = 0;
        missCount = 0;
        wrongCount = 0;
        combo = 0;
        badCount = 0;
        goodCount = 0;
        excellentCount = 0;
        bonusLivesEarned = 0;
        lastJudgeGrade = RhythmJudgeGrade.None;
    }

    // 정답 처리
    //
    // 현재 구조에서는:
    // - 정답 시 콤보 +1
    // - 기본 점수 + 콤보 보너스 점수를 적용한다.
    public void RegisterCorrectStep()
    {
        RegisterJudge(RhythmJudgeGrade.Good);
    }

    public void RegisterJudge(RhythmJudgeGrade judgeGrade)
    {
        switch (judgeGrade)
        {
            case RhythmJudgeGrade.Bad:
                badCount++;
                RegisterSuccessfulJudge(judgeGrade, badScore);
                break;

            case RhythmJudgeGrade.Good:
                goodCount++;
                RegisterSuccessfulJudge(judgeGrade, goodScore);
                break;

            case RhythmJudgeGrade.Excellent:
                excellentCount++;
                RegisterSuccessfulJudge(judgeGrade, excellentScore);
                break;

            case RhythmJudgeGrade.Wrong:
                RegisterWrongStep();
                break;

            case RhythmJudgeGrade.Miss:
                RegisterMiss();
                break;
        }
    }

    // 오답 처리
    // 콤보를 끊고 오답 횟수를 증가시킨다.
    // 현재 구조에서는 오답에 의한 점수 패널티는 주지 않는다.
    public void RegisterWrongStep()
    {
        combo = 0;
        wrongCount++;
        lastJudgeGrade = RhythmJudgeGrade.Wrong;

        Debug.Log($"[RhythmScoreManager] 오답! wrong={wrongCount}");
    }

    // 미스 처리
    // 입력이 없거나 판정 시간을 넘긴 경우 콤보를 끊고 미스 횟수를 증가시킨다.
    // 현재 구조에서는 미스에 의한 점수 패널티는 주지 않는다.
    public void RegisterMiss()
    {
        combo = 0;
        missCount++;
        lastJudgeGrade = RhythmJudgeGrade.Miss;

        Debug.Log($"[RhythmScoreManager] 미스! miss={missCount}");
    }

    private void RegisterSuccessfulJudge(RhythmJudgeGrade judgeGrade, int baseScore)
    {
        combo++;

        int comboBonus = GetComboBonus(combo);
        int gainedScore = baseScore + comboBonus;
        score += gainedScore;
        RefreshBonusLifeState();
        lastJudgeGrade = judgeGrade;

        Debug.Log($"[RhythmScoreManager] {judgeGrade}! gained={gainedScore}, score={score}, combo={combo}, comboBonus={comboBonus}");
    }

    private void RefreshBonusLifeState()
    {
        if (pointsPerBonusLife <= 0)
        {
            bonusLivesEarned = 0;
            return;
        }

        int updatedBonusLives = Mathf.Max(0, score / pointsPerBonusLife);
        if (updatedBonusLives > bonusLivesEarned)
        {
            Debug.Log($"[RhythmScoreManager] 보너스 라이프 획득! +{updatedBonusLives - bonusLivesEarned}, totalBonusLives={updatedBonusLives}, score={score}");
        }

        bonusLivesEarned = updatedBonusLives;
    }

    // 현재 콤보에 따라 추가 점수를 계산한다.
    //
    // 예시 규칙:
    // - 1 ~ 4 콤보   : 보너스 0
    // - 5 ~ 9 콤보   : 보너스 20
    // - 10 ~ 14 콤보 : 보너스 50
    // - 15 이상      : 보너스 100
    //
    // 필요하면 여기 숫자만 조정해서 손쉽게 밸런스를 바꿀 수 있다.
    private int GetComboBonus(int currentCombo)
    {
        if (currentCombo >= 15)
        {
            return 100;
        }

        if (currentCombo >= 10)
        {
            return 50;
        }

        if (currentCombo >= 5)
        {
            return 20;
        }

        return 0;
    }
}
