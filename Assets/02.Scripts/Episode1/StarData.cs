using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStarData", menuName = "Game/Star Data")]
public class StarData : ScriptableObject
{
    [Header("별 기본 정보")]
    public string starId;          // 별 고유 ID
    public Color starColor = Color.white;   // 퍼즐 UI에서 사용할 대표 색상
    [TextArea]
    public string description;     // 설명
    public Sprite icon;            // UI 표시용 아이콘
}
