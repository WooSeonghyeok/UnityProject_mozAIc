using Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(Ep3LobbyIntroCutsceneController))]
public class Ep3LobbyIntroCutsceneControllerEditor : Editor
{
    private static readonly Color CameraHandleColor = new Color(0.2f, 0.85f, 1f, 0.95f);
    private static readonly Color LookAtHandleColor = new Color(1f, 0.6f, 0.15f, 0.95f);
    private static readonly Color AimLineColor = new Color(1f, 0.82f, 0.45f, 0.8f);
    private static readonly Color PathLineColor = new Color(0.2f, 0.85f, 1f, 0.45f);

    private SerializedProperty inspectorShotsProperty;
    private SerializedProperty sceneIntroCameraProperty;
    private SerializedProperty sceneIntroPathProperty;
    private SerializedProperty sceneLookAtTargetProperty;
    private SerializedProperty syncSceneRigWithSequenceProperty;

    private void OnEnable()
    {
        inspectorShotsProperty = serializedObject.FindProperty("inspectorShots");
        sceneIntroCameraProperty = serializedObject.FindProperty("sceneIntroCamera");
        sceneIntroPathProperty = serializedObject.FindProperty("sceneIntroPath");
        sceneLookAtTargetProperty = serializedObject.FindProperty("sceneLookAtTarget");
        syncSceneRigWithSequenceProperty = serializedObject.FindProperty("syncSceneRigWithSequence");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        bool isPlayMode = EditorApplication.isPlayingOrWillChangePlaymode;

        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        bool inspectorChanged = EditorGUI.EndChangeCheck();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Scene View handles: blue cubes are camera positions, orange spheres are LookAt positions. " +
            "Drag them directly in the scene to adjust each shot.",
            MessageType.Info);

        if (isPlayMode)
        {
            EditorGUILayout.HelpBox(
                "Play Mode에서는 컷씬 미리보기와 경로 동기화를 잠시 비활성화합니다. " +
                "샷 수정은 Edit Mode에서 진행해주세요.",
                MessageType.None);
        }

        using (new EditorGUI.DisabledScope(isPlayMode))
        {
            DrawPreviewButtons();

            using (new EditorGUI.DisabledScope(inspectorShotsProperty == null || inspectorShotsProperty.arraySize == 0))
            {
                if (GUILayout.Button("Sync Scene Path From Shots"))
                {
                    serializedObject.ApplyModifiedProperties();
                    SyncSceneRig();
                }
            }
        }

        serializedObject.ApplyModifiedProperties();

        if (!isPlayMode && inspectorChanged && ShouldAutoSyncSceneRig())
        {
            SyncSceneRig();
        }
    }

    private void OnSceneGUI()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        serializedObject.Update();

        if (inspectorShotsProperty == null || inspectorShotsProperty.arraySize == 0)
        {
            serializedObject.ApplyModifiedProperties();
            return;
        }

        Vector3? previousCameraPosition = GetSceneRigStartPosition();
        Vector3? previewLookAtPosition = null;
        int previewShotIndex = -1;

        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < inspectorShotsProperty.arraySize; i++)
        {
            SerializedProperty shotProperty = inspectorShotsProperty.GetArrayElementAtIndex(i);
            SerializedProperty dialogueIdProperty = shotProperty.FindPropertyRelative("dialogueId");
            SerializedProperty cameraPositionProperty = shotProperty.FindPropertyRelative("cameraPosition");
            SerializedProperty lookAtPositionProperty = shotProperty.FindPropertyRelative("lookAtPosition");

            Vector3 cameraPosition = cameraPositionProperty.vector3Value;
            Vector3 lookAtPosition = lookAtPositionProperty.vector3Value;
            string shotLabel = string.IsNullOrWhiteSpace(dialogueIdProperty.stringValue)
                ? $"Shot {i + 1}"
                : dialogueIdProperty.stringValue;

            if (previousCameraPosition.HasValue)
            {
                Handles.color = PathLineColor;
                Handles.DrawAAPolyLine(3f, previousCameraPosition.Value, cameraPosition);
            }

            Handles.color = AimLineColor;
            Handles.DrawDottedLine(cameraPosition, lookAtPosition, 4f);

            float cameraHandleSize = HandleUtility.GetHandleSize(cameraPosition) * 0.14f;
            float lookAtHandleSize = HandleUtility.GetHandleSize(lookAtPosition) * 0.12f;

            Handles.color = CameraHandleColor;
            var fmh_106_17_639113598430123487 = Quaternion.identity; Vector3 updatedCameraPosition = Handles.FreeMoveHandle(
                cameraPosition,
                cameraHandleSize,
                Vector3.zero,
                Handles.CubeHandleCap);
            Handles.Label(
                updatedCameraPosition + (Vector3.up * (cameraHandleSize + 0.15f)),
                $"{shotLabel} Cam");

            Handles.color = LookAtHandleColor;
            var fmh_117_17_639113598430149997 = Quaternion.identity; Vector3 updatedLookAtPosition = Handles.FreeMoveHandle(
                lookAtPosition,
                lookAtHandleSize,
                Vector3.zero,
                Handles.SphereHandleCap);
            Handles.Label(
                updatedLookAtPosition + (Vector3.up * (lookAtHandleSize + 0.15f)),
                $"{shotLabel} Look");

            if (updatedCameraPosition != cameraPosition)
            {
                cameraPositionProperty.vector3Value = updatedCameraPosition;
                previewShotIndex = i;
            }

            if (updatedLookAtPosition != lookAtPosition)
            {
                lookAtPositionProperty.vector3Value = updatedLookAtPosition;
                previewLookAtPosition = updatedLookAtPosition;
                previewShotIndex = i;
            }

            previousCameraPosition = cameraPositionProperty.vector3Value;
        }

        bool sceneHandleChanged = EditorGUI.EndChangeCheck();
        serializedObject.ApplyModifiedProperties();

        if (!sceneHandleChanged)
        {
            return;
        }

        if (previewLookAtPosition.HasValue)
        {
            SyncPreviewLookAt(previewLookAtPosition.Value);
        }

        if (ShouldAutoSyncSceneRig())
        {
            SyncSceneRig();
        }

        if (previewShotIndex >= 0)
        {
            ApplyShotPreview(previewShotIndex, previewLookAtPosition);
        }
        else if (!ShouldAutoSyncSceneRig())
        {
            MarkSceneDirty();
        }

        SceneView.RepaintAll();
    }

    private bool ShouldAutoSyncSceneRig()
    {
        return syncSceneRigWithSequenceProperty != null && syncSceneRigWithSequenceProperty.boolValue;
    }

    private void DrawPreviewButtons()
    {
        if (inspectorShotsProperty == null || inspectorShotsProperty.arraySize == 0)
        {
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Shot Preview", EditorStyles.boldLabel);

        for (int i = 0; i < inspectorShotsProperty.arraySize; i++)
        {
            SerializedProperty shotProperty = inspectorShotsProperty.GetArrayElementAtIndex(i);
            string shotLabel = shotProperty.FindPropertyRelative("dialogueId").stringValue;
            if (string.IsNullOrWhiteSpace(shotLabel))
            {
                shotLabel = $"Shot {i + 1}";
            }

            if (GUILayout.Button($"Preview {shotLabel}"))
            {
                serializedObject.ApplyModifiedProperties();
                if (ShouldAutoSyncSceneRig())
                {
                    SyncSceneRig();
                }

                ApplyShotPreview(i, null);
            }
        }
    }

    private Vector3? GetSceneRigStartPosition()
    {
        CinemachineSmoothPath path = sceneIntroPathProperty != null
            ? sceneIntroPathProperty.objectReferenceValue as CinemachineSmoothPath
            : null;

        if (path == null || path.m_Waypoints == null || path.m_Waypoints.Length == 0)
        {
            return null;
        }

        return path.transform.TransformPoint(path.m_Waypoints[0].position);
    }

    private void SyncSceneRig()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        CinemachineSmoothPath path = sceneIntroPathProperty != null
            ? sceneIntroPathProperty.objectReferenceValue as CinemachineSmoothPath
            : null;

        if (path != null && inspectorShotsProperty != null && inspectorShotsProperty.arraySize > 0)
        {
            Undo.RecordObject(path, "Sync EP3 Intro Path");

            Vector3 startWorldPosition = ResolveStartWorldPosition(path);
            CinemachineSmoothPath.Waypoint[] waypoints = new CinemachineSmoothPath.Waypoint[inspectorShotsProperty.arraySize + 1];
            waypoints[0] = CreateWaypoint(path.transform, startWorldPosition);

            for (int i = 0; i < inspectorShotsProperty.arraySize; i++)
            {
                SerializedProperty shotProperty = inspectorShotsProperty.GetArrayElementAtIndex(i);
                Vector3 cameraPosition = shotProperty.FindPropertyRelative("cameraPosition").vector3Value;
                waypoints[i + 1] = CreateWaypoint(path.transform, cameraPosition);
            }

            path.m_Looped = false;
            path.m_Waypoints = waypoints;
            path.InvalidateDistanceCache();
            EditorUtility.SetDirty(path);
        }

        EditorUtility.SetDirty(target);
        MarkSceneDirty();
    }

    private void ApplyShotPreview(int shotIndex, Vector3? overrideLookAtPosition)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        if (inspectorShotsProperty == null || shotIndex < 0 || shotIndex >= inspectorShotsProperty.arraySize)
        {
            return;
        }

        SerializedProperty shotProperty = inspectorShotsProperty.GetArrayElementAtIndex(shotIndex);
        Vector3 lookAtPosition = overrideLookAtPosition
            ?? shotProperty.FindPropertyRelative("lookAtPosition").vector3Value;
        float fieldOfView = shotProperty.FindPropertyRelative("fieldOfView").floatValue;

        Transform lookAtTarget = sceneLookAtTargetProperty != null
            ? sceneLookAtTargetProperty.objectReferenceValue as Transform
            : null;
        CinemachineVirtualCamera introCamera = sceneIntroCameraProperty != null
            ? sceneIntroCameraProperty.objectReferenceValue as CinemachineVirtualCamera
            : null;

        if (lookAtTarget != null)
        {
            Undo.RecordObject(lookAtTarget, "Preview EP3 Intro LookAt");
            lookAtTarget.position = lookAtPosition;
            EditorUtility.SetDirty(lookAtTarget);
        }

        if (introCamera != null)
        {
            Undo.RecordObject(introCamera, "Preview EP3 Intro Camera");

            if (fieldOfView > 0.01f)
            {
                introCamera.m_Lens.FieldOfView = fieldOfView;
            }

            CinemachineTrackedDolly trackedDolly = introCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
            if (trackedDolly != null)
            {
                Undo.RecordObject(trackedDolly, "Preview EP3 Intro Dolly");
                trackedDolly.m_PathPosition = ResolvePreviewPathPosition(shotIndex);
                EditorUtility.SetDirty(trackedDolly);
            }

            introCamera.LookAt = lookAtTarget;
            introCamera.PreviousStateIsValid = false;
            EditorUtility.SetDirty(introCamera);
        }

        MarkSceneDirty();
    }

    private void SyncPreviewLookAt(Vector3 lookAtPosition)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        Transform lookAtTarget = sceneLookAtTargetProperty != null
            ? sceneLookAtTargetProperty.objectReferenceValue as Transform
            : null;

        if (lookAtTarget == null)
        {
            return;
        }

        Undo.RecordObject(lookAtTarget, "Move EP3 Intro LookAt Preview");
        lookAtTarget.position = lookAtPosition;
        EditorUtility.SetDirty(lookAtTarget);
    }

    private Vector3 ResolveStartWorldPosition(CinemachineSmoothPath path)
    {
        if (path.m_Waypoints != null && path.m_Waypoints.Length > 0)
        {
            return path.transform.TransformPoint(path.m_Waypoints[0].position);
        }

        if (inspectorShotsProperty != null && inspectorShotsProperty.arraySize > 0)
        {
            SerializedProperty firstShot = inspectorShotsProperty.GetArrayElementAtIndex(0);
            return firstShot.FindPropertyRelative("cameraPosition").vector3Value;
        }

        return path.transform.position;
    }

    private static CinemachineSmoothPath.Waypoint CreateWaypoint(Transform pathTransform, Vector3 worldPosition)
    {
        return new CinemachineSmoothPath.Waypoint
        {
            position = pathTransform != null ? pathTransform.InverseTransformPoint(worldPosition) : worldPosition,
            roll = 0f
        };
    }

    private float ResolvePreviewPathPosition(int shotIndex)
    {
        CinemachineSmoothPath path = sceneIntroPathProperty != null
            ? sceneIntroPathProperty.objectReferenceValue as CinemachineSmoothPath
            : null;
        float previewPosition = shotIndex + 1f;
        return path != null ? Mathf.Min(previewPosition, path.MaxPos) : previewPosition;
    }

    private void MarkSceneDirty()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        Ep3LobbyIntroCutsceneController controller = (Ep3LobbyIntroCutsceneController)target;
        if (controller != null && controller.gameObject.scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
        }
    }
}
