using UnityEngine;
using PuzzleInfo;
using UnityEditor;
using System;
using System.Linq;
public class PuzzleInit : EditorWindow
{
    public TextAsset puzzle_stage4;
    public DefaultAsset cubeDataFolder;
    public PuzzleCubeObj[] targetObjects;
    private Vector2 scrollPos;
    [MenuItem("Tools/PuzzleCube CSV Importer")]
    public static void ShowWindow()
    {
        GetWindow<PuzzleInit>("PuzzleCube CSV Importer");
    }
    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        puzzle_stage4 = Resources.Load<TextAsset>("csv/Stage4_CubePuzzle");
        cubeDataFolder = (DefaultAsset)EditorGUILayout.ObjectField("CubeData Folder", cubeDataFolder, typeof(DefaultAsset), false);
        if (GUILayout.Button("Load Objects From Folder", GUILayout.Height(25)))
        {
            LoadObjectsFromFolder();
        }
        EditorGUILayout.Space(10);
        if (targetObjects != null)
        {
            EditorGUILayout.LabelField($"Loaded Objects: {targetObjects.Length}", EditorStyles.boldLabel);

            SerializedObject so = new SerializedObject(this);
            SerializedProperty arrayProp = so.FindProperty("targetObjects");
            EditorGUILayout.PropertyField(arrayProp, true);
            so.ApplyModifiedProperties();
        }
        EditorGUILayout.Space(20);
        if (GUILayout.Button("Import CSV", GUILayout.Height(30)))
        {
            ImportCSV();
        }
        EditorGUILayout.EndScrollView();
    }
    private void LoadObjectsFromFolder()
    {
        if (cubeDataFolder == null)
        {
            Debug.LogError("폴더가 지정되지 않았습니다.");
            return;
        }
        string folderPath = AssetDatabase.GetAssetPath(cubeDataFolder);
        string[] guids = AssetDatabase.FindAssets("t:PuzzleCubeObj", new[] { folderPath });
        targetObjects = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<PuzzleCubeObj>(AssetDatabase.GUIDToAssetPath(guid)))
            .OrderBy(obj => obj.name) // 이름 순 정렬 (원하면 index 순 정렬도 가능)
            .ToArray();
        Debug.Log($"{targetObjects.Length}개의 PuzzleCubeObj를 로드했습니다.");
    }
    void ImportCSV()
    {
        if (puzzle_stage4 == null || targetObjects == null || targetObjects.Length == 0)
        {
            Debug.LogError("CSV 또는 ScriptableObject 배열이 비어있습니다.");
            return;
        }
        string[] lines = puzzle_stage4.text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            string[] cols = line.Split(',');
            if (i >= targetObjects.Length)
            {
                Debug.LogWarning($"CSV 줄 {i}는 ScriptableObject 배열 범위를 초과함");
                break;
            }
            PuzzleCubeObj obj = targetObjects[i];
            obj.place[0] = int.Parse(cols[1]);
            obj.place[1] = int.Parse(cols[2]);
            obj.colorBool[0] = cols[3].ToUpper() == "TRUE";
            obj.colorBool[1] = cols[4].ToUpper() == "TRUE";
            obj.colorBool[2] = cols[5].ToUpper() == "TRUE";
            obj.cond = Enum.Parse<PuzzleCube.switchCondition>(cols[6]);
            obj.value = int.Parse(cols[7]);
            EditorUtility.SetDirty(obj);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("CSV 데이터를 PuzzleCubeObj에 성공적으로 덮어씌웠습니다.");
    }
}