using System.Collections.Generic;
using UnityEngine;

public class Ep3_2FloatingDecorController : MonoBehaviour
{
    private enum FlowAxis
    {
        LocalX,
        LocalY,
        LocalZ
    }

    private sealed class DecorState
    {
        public Transform target;
        public Vector3 anchorLocalPosition;
        public Quaternion baseLocalRotation;
        public Vector3 baseLocalScale;
        public float bobHeight;
        public float bobSpeed;
        public float bobPhase;
        public float swayAmount;
        public float swaySpeed;
        public float swayPhase;
        public float rotationSpeed;
        public Vector3 rotationAxis;
        public float flowSpeed;
        public float flowOffset;
    }

    [Header("Targets")]
    [SerializeField] private Transform decorRoot;
    [SerializeField] private BoxCollider scatterArea;
    [SerializeField] private bool includeInactiveChildren = false;
    [SerializeField] private List<Transform> excludedChildren = new List<Transform>();

    [Header("Layout")]
    [SerializeField] private bool scatterOnAwake = true;
    [SerializeField] private bool useFixedSeed = true;
    [SerializeField] private int randomSeed = 3202;
    [SerializeField] private float edgePadding = 1.2f;
    [SerializeField] private Vector2 randomScaleRange = new Vector2(0.92f, 1.12f);
    [SerializeField] private bool randomYaw = true;
    [SerializeField] private Vector2 randomPitchRollRange = new Vector2(-6f, 6f);

    [Header("Idle Float")]
    [SerializeField] private Vector2 bobHeightRange = new Vector2(0.12f, 0.28f);
    [SerializeField] private Vector2 bobSpeedRange = new Vector2(0.55f, 1.1f);
    [SerializeField] private Vector2 swayAmountRange = new Vector2(0.05f, 0.14f);
    [SerializeField] private Vector2 swaySpeedRange = new Vector2(0.35f, 0.75f);
    [SerializeField] private Vector2 rotationSpeedRange = new Vector2(-8f, 8f);

    [Header("Flow")]
    [SerializeField] private bool loopWithinScatterArea = true;
    [SerializeField] private FlowAxis flowAxis = FlowAxis.LocalZ;
    [SerializeField] private bool flowNegativeDirection = true;
    [SerializeField] private Vector2 flowSpeedRange = new Vector2(1.1f, 2.1f);
    [SerializeField] private float flowBlendTime = 1.8f;

    private readonly List<DecorState> decorStates = new List<DecorState>();
    private float currentFlowWeight;
    private float targetFlowWeight;

    private void Awake()
    {
        CacheDecorTargets();

        if (scatterOnAwake)
        {
            ScatterChildrenNow();
        }
        else
        {
            CaptureCurrentLayout();
        }
    }

    private void Update()
    {
        if (!Application.isPlaying || decorStates.Count == 0)
        {
            return;
        }

        UpdateFlowWeight();
        UpdateDecorMotion();
    }

    public void BeginFlow()
    {
        targetFlowWeight = 1f;
    }

    public void StopFlow()
    {
        targetFlowWeight = 0f;
    }

    [ContextMenu("Scatter Children Now")]
    public void ScatterChildrenNow()
    {
        CacheDecorTargets();

        if (scatterArea == null)
        {
            Debug.LogWarning("[Ep3_2FloatingDecorController] Scatter Area BoxCollider is missing.");
            CaptureCurrentLayout();
            return;
        }

        RunWithSeed(() =>
        {
            for (int i = 0; i < decorStates.Count; i++)
            {
                DecorState state = decorStates[i];
                if (state.target == null)
                {
                    continue;
                }

                state.anchorLocalPosition = GetRandomLocalPositionInArea();
                state.baseLocalRotation = GetRandomizedRotation(state.target.localRotation);
                state.baseLocalScale = state.target.localScale * Random.Range(randomScaleRange.x, randomScaleRange.y);
                RandomizeMotion(state);
                state.flowOffset = 0f;

                ApplyImmediatePose(state);
            }
        });

        currentFlowWeight = 0f;
        targetFlowWeight = 0f;
    }

    [ContextMenu("Refresh Decor Targets")]
    public void RefreshDecorTargets()
    {
        CacheDecorTargets();
        CaptureCurrentLayout();
    }

    private void CacheDecorTargets()
    {
        decorStates.Clear();

        Transform root = decorRoot != null ? decorRoot : transform;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child == null)
            {
                continue;
            }

            if (scatterArea != null && child == scatterArea.transform)
            {
                continue;
            }

            if (!includeInactiveChildren && !child.gameObject.activeSelf)
            {
                continue;
            }

            if (excludedChildren.Contains(child))
            {
                continue;
            }

            decorStates.Add(new DecorState
            {
                target = child
            });
        }
    }

    private void CaptureCurrentLayout()
    {
        RunWithSeed(() =>
        {
            for (int i = 0; i < decorStates.Count; i++)
            {
                DecorState state = decorStates[i];
                if (state.target == null)
                {
                    continue;
                }

                state.anchorLocalPosition = state.target.localPosition;
                state.baseLocalRotation = state.target.localRotation;
                state.baseLocalScale = state.target.localScale;
                RandomizeMotion(state);
                state.flowOffset = 0f;
            }
        });

        currentFlowWeight = 0f;
        targetFlowWeight = 0f;
    }

    private void RandomizeMotion(DecorState state)
    {
        state.bobHeight = Random.Range(bobHeightRange.x, bobHeightRange.y);
        state.bobSpeed = Random.Range(bobSpeedRange.x, bobSpeedRange.y);
        state.bobPhase = Random.Range(0f, 100f);
        state.swayAmount = Random.Range(swayAmountRange.x, swayAmountRange.y);
        state.swaySpeed = Random.Range(swaySpeedRange.x, swaySpeedRange.y);
        state.swayPhase = Random.Range(0f, 100f);
        state.rotationSpeed = Random.Range(rotationSpeedRange.x, rotationSpeedRange.y);
        state.rotationAxis = GetRandomRotationAxis();
        state.flowSpeed = Random.Range(flowSpeedRange.x, flowSpeedRange.y);
    }

    private Quaternion GetRandomizedRotation(Quaternion currentRotation)
    {
        Vector3 randomEuler = new Vector3(
            Random.Range(randomPitchRollRange.x, randomPitchRollRange.y),
            randomYaw ? Random.Range(0f, 360f) : 0f,
            Random.Range(randomPitchRollRange.x, randomPitchRollRange.y));

        return currentRotation * Quaternion.Euler(randomEuler);
    }

    private Vector3 GetRandomRotationAxis()
    {
        Vector3 axis = new Vector3(
            Random.Range(-0.35f, 0.35f),
            1f,
            Random.Range(-0.35f, 0.35f));

        return axis.normalized;
    }

    private Vector3 GetRandomLocalPositionInArea()
    {
        if (scatterArea == null)
        {
            return Vector3.zero;
        }

        Vector3 halfSize = scatterArea.size * 0.5f;
        float paddedX = Mathf.Max(0f, halfSize.x - edgePadding);
        float paddedY = Mathf.Max(0f, halfSize.y - edgePadding);
        float paddedZ = Mathf.Max(0f, halfSize.z - edgePadding);

        Vector3 localPointInBox = scatterArea.center + new Vector3(
            Random.Range(-paddedX, paddedX),
            Random.Range(-paddedY, paddedY),
            Random.Range(-paddedZ, paddedZ));

        Vector3 worldPoint = scatterArea.transform.TransformPoint(localPointInBox);
        return transform.InverseTransformPoint(worldPoint);
    }

    private void UpdateFlowWeight()
    {
        if (flowBlendTime <= 0f)
        {
            currentFlowWeight = targetFlowWeight;
            return;
        }

        currentFlowWeight = Mathf.MoveTowards(
            currentFlowWeight,
            targetFlowWeight,
            Time.deltaTime / flowBlendTime);
    }

    private void UpdateDecorMotion()
    {
        float time = Time.time;
        Vector3 swayAxis = GetSwayAxis();
        int axisIndex = GetFlowAxisIndex();
        GetFlowBounds(axisIndex, out float axisMin, out float axisMax);

        for (int i = 0; i < decorStates.Count; i++)
        {
            DecorState state = decorStates[i];
            if (state.target == null)
            {
                continue;
            }

            float flowDirection = flowNegativeDirection ? -1f : 1f;
            state.flowOffset += state.flowSpeed * flowDirection * currentFlowWeight * Time.deltaTime;

            Vector3 localPosition = state.anchorLocalPosition;
            float axisValue = GetAxisValue(localPosition, axisIndex) + state.flowOffset;

            if (loopWithinScatterArea && scatterArea != null)
            {
                axisValue = WrapAxisValue(axisValue, axisMin, axisMax);
            }

            SetAxisValue(ref localPosition, axisIndex, axisValue);

            float bobOffset = Mathf.Sin((time + state.bobPhase) * state.bobSpeed) * state.bobHeight;
            float swayOffset = Mathf.Sin((time + state.swayPhase) * state.swaySpeed) * state.swayAmount;
            localPosition += Vector3.up * bobOffset;
            localPosition += swayAxis * swayOffset;

            state.target.localPosition = localPosition;

            Quaternion localRotation = state.baseLocalRotation;
            if (Mathf.Abs(state.rotationSpeed) > 0.01f)
            {
                float rotationAngle = (time + state.bobPhase) * state.rotationSpeed;
                localRotation *= Quaternion.AngleAxis(rotationAngle, state.rotationAxis);
            }

            state.target.localRotation = localRotation;
            state.target.localScale = state.baseLocalScale;
        }
    }

    private void ApplyImmediatePose(DecorState state)
    {
        if (state.target == null)
        {
            return;
        }

        state.target.localPosition = state.anchorLocalPosition;
        state.target.localRotation = state.baseLocalRotation;
        state.target.localScale = state.baseLocalScale;
    }

    private Vector3 GetSwayAxis()
    {
        switch (flowAxis)
        {
            case FlowAxis.LocalX:
                return Vector3.forward;
            case FlowAxis.LocalY:
                return Vector3.right;
            default:
                return Vector3.right;
        }
    }

    private int GetFlowAxisIndex()
    {
        switch (flowAxis)
        {
            case FlowAxis.LocalX:
                return 0;
            case FlowAxis.LocalY:
                return 1;
            default:
                return 2;
        }
    }

    private void GetFlowBounds(int axisIndex, out float min, out float max)
    {
        if (scatterArea == null)
        {
            min = -10f;
            max = 10f;
            return;
        }

        Vector3 center = scatterArea.center;
        Vector3 extents = scatterArea.size * 0.5f;
        bool initialized = false;
        min = 0f;
        max = 0f;

        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 localCorner = center + Vector3.Scale(extents, new Vector3(x, y, z));
                    Vector3 worldCorner = scatterArea.transform.TransformPoint(localCorner);
                    Vector3 controllerLocalCorner = transform.InverseTransformPoint(worldCorner);
                    float axisValue = GetAxisValue(controllerLocalCorner, axisIndex);

                    if (!initialized)
                    {
                        min = axisValue;
                        max = axisValue;
                        initialized = true;
                    }
                    else
                    {
                        min = Mathf.Min(min, axisValue);
                        max = Mathf.Max(max, axisValue);
                    }
                }
            }
        }
    }

    private float WrapAxisValue(float value, float min, float max)
    {
        float range = max - min;
        if (range <= 0.001f)
        {
            return value;
        }

        while (value < min)
        {
            value += range;
        }

        while (value > max)
        {
            value -= range;
        }

        return value;
    }

    private float GetAxisValue(Vector3 value, int axisIndex)
    {
        switch (axisIndex)
        {
            case 0:
                return value.x;
            case 1:
                return value.y;
            default:
                return value.z;
        }
    }

    private void SetAxisValue(ref Vector3 value, int axisIndex, float axisValue)
    {
        switch (axisIndex)
        {
            case 0:
                value.x = axisValue;
                break;
            case 1:
                value.y = axisValue;
                break;
            default:
                value.z = axisValue;
                break;
        }
    }

    private void RunWithSeed(System.Action action)
    {
        if (action == null)
        {
            return;
        }

        if (!useFixedSeed)
        {
            action.Invoke();
            return;
        }

        Random.State previousState = Random.state;
        Random.InitState(randomSeed);
        action.Invoke();
        Random.state = previousState;
    }
}
