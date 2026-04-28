using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Ep3_2Manager))]
public class Ep3_2ManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawScriptField();
        EditorGUILayout.Space();

        DrawSection("리듬 퍼즐 설정", new[]
        {
            ("rhythmPuzzleManager", "기존 리듬 퍼즐 매니저"),
            ("useTopDownRhythmPuzzle", "탑다운 리듬 퍼즐 사용"),
            ("topDownRhythmPuzzle", "탑다운 퍼즐 러너"),
        });

        DrawSection("NPC 연출", new[]
        {
            ("stageNpcFollower", "무대 NPC 팔로워"),
            ("npcPuzzleWaitPoint", "퍼즐 대기 위치"),
            ("npcClearWarpPoint", "클리어 이동 위치"),
            ("disableNpcFollowOnStart", "시작 시 NPC 추적 끄기"),
        });

        DrawSection("퍼즐 시작 상호작용", new[]
        {
            ("startPuzzleController", "퍼즐 시작 컨트롤러"),
        });

        DrawSection("출구 문 상호작용", new[]
        {
            ("exitDoorInteractable", "출구 문 인터랙터블"),
        });

        DrawSection("클리어 컷씬", new[]
        {
            ("playClearCutsceneOnPuzzleComplete", "클리어 후 컷씬 재생"),
            ("clearCutscenePlayer", "컷씬 플레이어"),
            ("clearCutsceneStartIndex", "컷씬 시작 인덱스"),
            ("clearCutsceneStepCount", "컷씬 길이"),
        });

        DrawSection("AI / 힌트 기록", new[]
        {
            ("hintCount", "힌트 요청 횟수"),
            ("hintIntensity", "힌트 강도 단계"),
            ("aiInteractionCount", "AI 상호작용 횟수"),
            ("collectedTags", "수집 태그 목록"),
        });

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSection(string title, (string propertyName, string label)[] fields)
    {
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        foreach ((string propertyName, string label) in fields)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, new GUIContent(label), true);
            }
        }

        EditorGUILayout.Space(4f);
    }

    private void DrawScriptField()
    {
        using (new EditorGUI.DisabledScope(true))
        {
            MonoScript script = MonoScript.FromMonoBehaviour((MonoBehaviour)target);
            EditorGUILayout.ObjectField("스크립트", script, typeof(MonoScript), false);
        }
    }
}

[CustomEditor(typeof(Ep3_2TopDownRhythmPuzzle))]
public class Ep3_2TopDownRhythmPuzzleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawScriptField();
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("탑다운 5레인 리듬 퍼즐입니다. D / F / Space / J / K 입력, 레인 비주얼, 판정 범위, 정보 박스를 여기서 조절합니다.", MessageType.Info);

        DrawSection("연결", new[]
        {
            ("stageManager", "스테이지 매니저"),
            ("audioManager", "리듬 오디오 매니저"),
            ("scoreManager", "리듬 점수 매니저"),
            ("effectManager", "리듬 이펙트 매니저"),
            ("playerMovement", "플레이어 이동"),
            ("playerInput", "플레이어 입력"),
            ("startPuzzleController", "퍼즐 시작 컨트롤러"),
            ("puzzleReferenceTransform", "기준 트랜스폼"),
        });

        DrawSection("플레이어 고정", new[]
        {
            ("lockPlayerDuringPuzzle", "퍼즐 중 플레이어 고정"),
            ("teleportPlayerToHoldPoint", "고정 위치로 즉시 이동"),
            ("playerHoldPoint", "플레이어 고정 위치"),
            ("playerHoldHeightOffset", "플레이어 높이 보정"),
            ("fallbackHoldDistanceFromJudge", "기본 고정 거리"),
            ("fallbackHoldLocalOffset", "기본 고정 오프셋"),
        });

        DrawSection("판정선 / 레인", new[]
        {
            ("judgeCenterAnchor", "판정선 중심 앵커"),
            ("fallbackJudgeCenterOffset", "기본 판정선 오프셋"),
            ("laneSpacing", "레인 간격"),
            ("laneForwardSpacing", "레인 전후 간격"),
            ("noteTravelDistance", "노트 이동 거리"),
            ("noteHeightOffset", "노트 높이 보정"),
        });

        DrawSection("카메라", new[]
        {
            ("useRuntimePuzzleCamera", "퍼즐 전용 카메라 사용"),
            ("useOrthographicPuzzleCamera", "정사영 카메라 사용"),
            ("orthographicSize", "정사영 크기"),
            ("cameraHeight", "카메라 높이"),
            ("cameraDistance", "카메라 거리"),
            ("cameraFocusOffset", "카메라 초점 오프셋"),
            ("cameraFollowSmooth", "카메라 보간 속도"),
        });

        DrawVisualSection();
        DrawScoreUiSection();
        DrawJudgementSection();
        DrawFeedbackSection();

        DrawSection("실패 조건", new[]
        {
            ("maxAllowedMistakes", "허용 실수 횟수"),
        });

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawVisualSection()
    {
        EditorGUILayout.LabelField("비주얼", EditorStyles.boldLabel);

        DrawProperty("noteVisualPrefab", "기본 노트 프리팹");
        DrawProperty("useSpriteNoteVisuals", "스프라이트 음표 사용");

        SerializedProperty useLaneSpecific = serializedObject.FindProperty("useLaneSpecificNotePrefabs");
        if (useLaneSpecific != null)
        {
            EditorGUILayout.PropertyField(useLaneSpecific, new GUIContent("레인별 노트 프리팹 사용"));
            if (useLaneSpecific.boolValue)
            {
                EditorGUI.indentLevel++;
                DrawProperty("leftLaneNotePrefab", "D 레인 노트 프리팹");
                DrawProperty("upLaneNotePrefab", "F 레인 노트 프리팹");
                DrawProperty("downLaneNotePrefab", "Space 레인 노트 프리팹");
                DrawProperty("rightLaneNotePrefab", "J 레인 노트 프리팹");
                DrawProperty("extraLaneNotePrefab", "K 레인 노트 프리팹");
                EditorGUI.indentLevel--;
            }
        }

        DrawProperty("lanePadPrefab", "공통 레인 패드 프리팹");
        DrawProperty("dLanePadPrefab", "D 레인 발판 프리팹");
        DrawProperty("fLanePadPrefab", "F 레인 발판 프리팹");
        DrawProperty("spaceLanePadPrefab", "Space 레인 발판 프리팹");
        DrawProperty("jLanePadPrefab", "J 레인 발판 프리팹");
        DrawProperty("kLanePadPrefab", "K 레인 발판 프리팹");
        DrawProperty("useSimpleLanePadVisuals", "프리팹이 없을 때 단순 패드 사용");
        DrawProperty("lanePadOpacity", "단순 패드 투명도");
        DrawProperty("showLaneTracks", "레인 검은 박스 표시");
        DrawProperty("laneTrackWidth", "레인 박스 폭");
        DrawProperty("laneTrackLength", "레인 박스 길이");
        DrawProperty("laneTrackHeightOffset", "레인 박스 높이");
        DrawProperty("laneTrackOpacity", "레인 박스 투명도");
        DrawProperty("laneTrackLineWidth", "레인 중앙선 두께");
        DrawProperty("laneTrackLineColor", "레인 중앙선 색");
        DrawProperty("laneTrackEulerOffset", "레인 박스 회전 보정");
        DrawProperty("noteScale", "노트 크기");
        DrawProperty("noteVisualEulerOffset", "노트 회전 보정");
        DrawProperty("prefabLanePadEulerOffset", "발판 프리팹 회전 보정");
        DrawProperty("lanePadEulerOffset", "단순 패드 회전 보정");
        DrawProperty("lanePadScale", "레인 패드 크기");
        DrawProperty("showLaneLabels", "레인 키 글자 표시");
        DrawProperty("laneLabelHeightOffset", "키 글자 높이");
        DrawProperty("laneLabelFontSize", "키 글자 폰트 크기");
        DrawProperty("laneLabelCharacterSize", "키 글자 월드 크기");
        DrawProperty("laneLabelColor", "키 글자 색");
        DrawProperty("showJudgeInfoPanels", "판정/실수 정보 박스 표시");
        DrawProperty("judgeInfoPanelOffset", "정보 박스 아래 오프셋");
        DrawProperty("judgeInfoPanelHeightOffset", "정보 박스 높이");
        DrawProperty("judgeInfoPanelGap", "정보 박스 간격");
        DrawProperty("judgeInfoPanelCharacterSize", "정보 박스 월드 크기");
        DrawProperty("judgeInfoPanelSize", "정보 박스 크기");
        DrawProperty("judgeInfoPanelBackgroundColor", "정보 박스 배경색");
        DrawProperty("judgeInfoTextColor", "정보 박스 기본 글자색");
        DrawProperty("leftLaneColor", "D 레인 색상");
        DrawProperty("upLaneColor", "F 레인 색상");
        DrawProperty("downLaneColor", "Space 레인 색상");
        DrawProperty("rightLaneColor", "J 레인 색상");
        DrawProperty("extraLaneColor", "K 레인 색상");

        EditorGUILayout.Space(4f);
    }

    private void DrawJudgementSection()
    {
        EditorGUILayout.LabelField("판정 설정", EditorStyles.boldLabel);
        DrawProperty("topDownChartGlobalJudgeOffset", "채보 전체 판정 오프셋");
        DrawProperty("excellentJudgeWindow", "Excellent 판정 범위");
        DrawProperty("goodJudgeWindow", "Good 판정 범위");
        DrawProperty("badJudgeWindow", "Bad 판정 범위");
        EditorGUILayout.Space(4f);
    }

    private void DrawScoreUiSection()
    {
        EditorGUILayout.LabelField("점수 UI", EditorStyles.boldLabel);
        DrawProperty("showScorePanel", "오른쪽 위 점수 패널 표시");
        DrawProperty("scorePanelScreenOffset", "화면 오른쪽 위 오프셋");
        DrawProperty("scorePanelSize", "점수 패널 크기");
        DrawProperty("scorePanelBackgroundColor", "점수 패널 배경색");
        DrawProperty("scorePanelTitleColor", "제목/만 단위 색");
        DrawProperty("scorePanelMajorDigitColor", "앞자리 강조 색");
        DrawProperty("scorePanelMinorDigitColor", "나머지 점수 색");
        DrawProperty("scorePanelHintColor", "설명 문구 색");
        EditorGUILayout.Space(4f);
    }

    private void DrawFeedbackSection()
    {
        EditorGUILayout.LabelField("입력 피드백", EditorStyles.boldLabel);
        DrawProperty("showLaneInputFeedback", "레인 입력 피드백 사용");
        DrawProperty("playSuccessEffectOnCorrectInput", "정답 입력 시 성공 이펙트");
        DrawProperty("lanePressDepth", "눌림 깊이");
        DrawProperty("lanePressedScaleMultiplier", "눌림 세로 배율");
        DrawProperty("lanePressRecoverSpeed", "눌림 복귀 속도");
        DrawProperty("laneFlashRecoverSpeed", "색 복귀 속도");
        DrawProperty("laneFlashIntensity", "색 플래시 강도");
        DrawProperty("laneEmissionBoost", "밝기 강화");
        DrawProperty("badLaneFlashColor", "Bad 플래시 색");
        DrawProperty("goodLaneFlashColor", "Good 플래시 색");
        DrawProperty("excellentLaneFlashColor", "Excellent 플래시 색");
        DrawProperty("wrongLaneFlashColor", "오답 플래시 색");
        EditorGUILayout.Space(4f);
    }

    private void DrawSection(string title, (string propertyName, string label)[] fields)
    {
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        foreach ((string propertyName, string label) in fields)
        {
            DrawProperty(propertyName, label);
        }

        EditorGUILayout.Space(4f);
    }

    private void DrawProperty(string propertyName, string label)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            EditorGUILayout.PropertyField(property, new GUIContent(label), true);
        }
    }

    private void DrawScriptField()
    {
        using (new EditorGUI.DisabledScope(true))
        {
            MonoScript script = MonoScript.FromMonoBehaviour((MonoBehaviour)target);
            EditorGUILayout.ObjectField("스크립트", script, typeof(MonoScript), false);
        }
    }
}

[CustomEditor(typeof(Ep3_2StartPuzzle))]
public class Ep3_2StartPuzzleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawScriptField();
        EditorGUILayout.Space();

        DrawSection("연결", new[]
        {
            ("ep3_2Manager", "3-2 매니저"),
            ("musicSource", "배경 음악 소스"),
            ("backgroundDecorController", "배경 장식 컨트롤러"),
        });

        DrawSection("카운트다운 UI", new[]
        {
            ("countdownPanel", "카운트다운 패널"),
            ("countdownText", "카운트다운 텍스트"),
            ("countdownCanvas", "카운트다운 캔버스"),
            ("autoCreateCountdownUi", "카운트다운 UI 자동 생성"),
            ("countdownPanelSize", "패널 크기"),
            ("countdownPanelOffset", "패널 위치 오프셋"),
            ("countdownPanelColor", "패널 색상"),
            ("countdownFontSize", "카운트다운 글자 크기"),
        });

        DrawSection("추가 연출 / 트리거", new[]
        {
            ("eventTriggerToEnable", "시작 후 켤 이벤트"),
            ("interactableToDisable", "시작 후 끌 오브젝트"),
        });

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSection(string title, (string propertyName, string label)[] fields)
    {
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        foreach ((string propertyName, string label) in fields)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, new GUIContent(label), true);
            }
        }

        EditorGUILayout.Space(4f);
    }

    private void DrawScriptField()
    {
        using (new EditorGUI.DisabledScope(true))
        {
            MonoScript script = MonoScript.FromMonoBehaviour((MonoBehaviour)target);
            EditorGUILayout.ObjectField("스크립트", script, typeof(MonoScript), false);
        }
    }
}

[CustomEditor(typeof(BeatMapData))]
public class BeatMapDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawScriptField();
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("탑다운 수동 채보 에셋입니다. 각 노트의 judgeTimeSeconds에 실제 재생 시간(초)을 넣고, laneType에 D / F / Space / J / K를 지정하면 멜로디에 맞는 채보를 직접 만들 수 있습니다.", MessageType.Info);

        SerializedProperty topDownChartNotes = serializedObject.FindProperty("topDownChartNotes");
        if (topDownChartNotes != null)
        {
            EditorGUILayout.PropertyField(topDownChartNotes, new GUIContent("탑다운 채보 노트 목록"), true);
        }

        EditorGUILayout.Space(6f);

        SerializedProperty beatEvents = serializedObject.FindProperty("beatEvents");
        if (beatEvents != null)
        {
            EditorGUILayout.PropertyField(beatEvents, new GUIContent("기존 비트 이벤트(레거시)"), true);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawScriptField()
    {
        using (new EditorGUI.DisabledScope(true))
        {
            MonoScript script = MonoScript.FromScriptableObject((ScriptableObject)target);
            EditorGUILayout.ObjectField("스크립트", script, typeof(MonoScript), false);
        }
    }
}
