using UnityEngine;

public class PortalMaterialController : MonoBehaviour
{
    public enum PortalType { Space, Paint }
    public PortalType portalType;

    [Header("Materials")]
    public Material incompleteMat;
    public Material completeMat;

    private MeshRenderer rend;

    void Start()
    {
        rend = GetComponent<MeshRenderer>();
        ApplyMaterial();
    }

    void ApplyMaterial()
    {
        if (EP2_PuzzleManager.Instance == null)
        {
            Debug.LogError("PuzzleManager 없음!");
            return;
        }

        bool isCleared = false;

        if (portalType == PortalType.Space)
            isCleared = EP2_PuzzleManager.Instance.spaceClear;
        else if (portalType == PortalType.Paint)
            isCleared = EP2_PuzzleManager.Instance.paintClear;

        Material targetMat = isCleared ? completeMat : incompleteMat;

        // 🔥 핵심: material 교체 X
        rend.material.CopyPropertiesFromMaterial(targetMat);
    }
}