using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RhythmAudioManager))]
public class RhythmAudioManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("WAV 기반 채보 생성", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "현재 연결된 audioClip(.wav) 파형에서 온셋을 추정해 topDownBeatMapAsset에 초안 채보를 다시 생성합니다. 완전 수동 채보 전의 1차 초안용입니다.",
            MessageType.Info);

        RhythmAudioManager manager = (RhythmAudioManager)target;
        using (new EditorGUI.DisabledScope(manager == null || manager.AudioClip == null || manager.TopDownBeatMapAsset == null))
        {
            if (GUILayout.Button("WAV 기반 초안 채보 생성"))
            {
                bool generated = Ep3_2WavChartGenerator.GenerateFromManager(manager);
                if (generated)
                {
                    EditorUtility.SetDirty(manager.TopDownBeatMapAsset);
                    Selection.activeObject = manager.TopDownBeatMapAsset;
                }
            }
        }
    }
}
