using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EP2ProgressData
{
    // 우주 퍼즐 클리어 여부
    public static bool spaceClear = false;

    // 그림 퍼즐 클리어 여부
    public static bool paintClear = false;

    // 중복 적용 방지용
    public static bool midApplied = false;
    public static bool clearApplied = false;

    // 새 게임 시작 시 EP2 진행 상태 초기화
    public static void Reset()
    {
        spaceClear = false;
        paintClear = false;
        midApplied = false;
        clearApplied = false;
    }
}
