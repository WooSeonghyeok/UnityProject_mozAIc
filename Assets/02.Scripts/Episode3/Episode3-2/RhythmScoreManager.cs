using UnityEngine;

public class RhythmScoreManager : MonoBehaviour
{
    [Header("플레이 정보")]
    [SerializeField] private int score = 0;
    [SerializeField] private int missCount = 0;
    [SerializeField] private int wrongCount = 0;
    [SerializeField] private int combo = 0;

    [Header("점수 설정")]
    [SerializeField] private int baseScorePerCorrect = 100;

    public int Score => score;
    public int MissCount => missCount;
    public int WrongCount => wrongCount;
    public int Combo => combo;

    // 점수 관련 상태를 초기값으로 리셋한다.
    // 새 퍼즐 시작 시 호출된다.
    public void ResetState()
    {
        score = 0;
        missCount = 0;
        wrongCount = 0;
        combo = 0;
    }

    // 정답 처리
    //
    // 현재 구조에서는:
    // - 정답 시 콤보 +1
    // - 기본 점수 + 콤보 보너스 점수를 적용한다.
    public void RegisterCorrectStep()
    {
        combo++;

        int comboBonus = GetComboBonus(combo);
        int gainedScore = baseScorePerCorrect + comboBonus;

        score += gainedScore;

        Debug.Log($"[RhythmScoreManager] 정답! gained={gainedScore}, score={score}, combo={combo}, comboBonus={comboBonus}");
    }

    // 오답 처리
    // 콤보를 끊고 오답 횟수를 증가시킨다.
    // 현재 구조에서는 오답에 의한 점수 패널티는 주지 않는다.
    public void RegisterWrongStep()
    {
        combo = 0;
        wrongCount++;

        Debug.Log($"[RhythmScoreManager] 오답! wrong={wrongCount}");
    }

    // 미스 처리
    // 입력이 없거나 판정 시간을 넘긴 경우 콤보를 끊고 미스 횟수를 증가시킨다.
    // 현재 구조에서는 미스에 의한 점수 패널티는 주지 않는다.
    public void RegisterMiss()
    {
        combo = 0;
        missCount++;

        Debug.Log($"[RhythmScoreManager] 미스! miss={missCount}");
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