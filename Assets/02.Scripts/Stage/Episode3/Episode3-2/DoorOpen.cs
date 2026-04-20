using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DoorOpen : MonoBehaviour
{
    [Header("회전 설정")]
    [Tooltip("문이 열렸을 때 Y축으로 회전할 각도(도)")]
    [SerializeField] private float openAngle = 90f;

    [Header("시간/이징")]
    [Tooltip("문이 열리는 데 걸리는 시간(초)")]
    [SerializeField] private float openDuration = 0.6f;
    [Tooltip("회전 이징 커브(0:start, 1:end)")]
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("설정")]
    [Tooltip("로컬 회전 사용 여부 (대부분의 문의 경우 true)")]
    [SerializeField] private bool useLocalRotation = true;
    [Tooltip("시작 시 열린 상태로 시작")]
    [SerializeField] private bool startOpened = false;
    [Tooltip("Player 태그가 트리거에 들어오면 자동으로 열기")]
    [SerializeField] private bool openOnTriggerEnter = false;
    [Tooltip("Player가 트리거에서 나가면 자동으로 닫기")]
    [SerializeField] private bool closeOnTriggerExit = false;

    [Header("디자이너 훅")]
    public UnityEvent onOpened;
    public UnityEvent onClosed;

    private Quaternion closedRotation;
    private Quaternion openedRotation;
    private bool isOpen = false;
    private bool isMoving = false;
    private Coroutine moveCoroutine;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        closedRotation = useLocalRotation ? transform.localRotation : transform.rotation;
        openedRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);

        isOpen = startOpened;

        var initial = isOpen ? openedRotation : closedRotation;
        if (useLocalRotation) transform.localRotation = initial;
        else transform.rotation = initial;
    }

    public void Open()
    {
        if (isOpen && !isMoving) return;
        StartMove(openedRotation, true);
    }

    public void Close()
    {
        if (!isOpen && !isMoving) return;
        StartMove(closedRotation, false);
    }

    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    private void StartMove(Quaternion targetRot, bool willBeOpen)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveRoutine(targetRot, willBeOpen));
    }

    private IEnumerator MoveRoutine(Quaternion targetRot, bool willBeOpen)
    {
        isMoving = true;
        float elapsed = 0f;

        Quaternion startRot = useLocalRotation ? transform.localRotation : transform.rotation;
        float dur = Mathf.Max(0.0001f, openDuration);

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dur);
            float ease = easeCurve != null ? easeCurve.Evaluate(t) : t;
            Quaternion q = Quaternion.Slerp(startRot, targetRot, ease);

            if (useLocalRotation) transform.localRotation = q;
            else transform.rotation = q;

            yield return null;
        }

        if (useLocalRotation) transform.localRotation = targetRot;
        else transform.rotation = targetRot;

        isOpen = willBeOpen;
        isMoving = false;
        moveCoroutine = null;

        if (isOpen) onOpened?.Invoke();
        else onClosed?.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!openOnTriggerEnter) return;
        if (!other.CompareTag("Player")) return;
        Open();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!closeOnTriggerExit) return;
        if (!other.CompareTag("Player")) return;
        Close();
    }

    private void OnValidate()
    {
        Quaternion baseRot = useLocalRotation ? transform.localRotation : transform.rotation;
        Quaternion newOpened = baseRot * Quaternion.Euler(0f, openAngle, 0f);
        openedRotation = newOpened;

        if (!Application.isPlaying)
        {
            Quaternion preview = startOpened ? openedRotation : baseRot;
            if (useLocalRotation) transform.localRotation = preview;
            else transform.rotation = preview;
        }
    }
}