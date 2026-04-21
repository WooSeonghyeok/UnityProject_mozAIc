using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// AI 힌트에 사용할 오브젝트 정보를 담는 컴포넌트
// 각 힌트 대상 오브젝트에 붙여서 사용

public class NPCHintTarget : MonoBehaviour
{
    [Header("AI에게 보여줄 이름")]
    public string targetName = "오브젝트";

    [Header("이 오브젝트를 힌트에 포함할지 여부")]
    public bool includeInHint = true;

    [Header("가까이 갔을 때 사용할 표현")]
    public float nearDistance = 2.5f;

    [Header("이 오브젝트를 설명할 때 쓰는 힌트 문장")]
    [TextArea(2, 4)]
    public string hintFlavorText;

    [Header("추가 설명")]
    [TextArea(2, 4)]
    public string description;
}
