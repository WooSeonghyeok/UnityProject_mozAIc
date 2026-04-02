using UnityEngine;
using UnityEngine.UI;

public class MinimapManager : MonoBehaviour
{
    public Camera mapCam;
    public Button expand;
    public Button shrink;
    public float maxSize;
    public float minSize;
    public void OnExpand()
    {
        mapCam.orthographicSize--;
        mapCam.orthographicSize = Mathf.Max(mapCam.orthographicSize, maxSize);
    }
    public void OnShrink()
    {
        mapCam.orthographicSize++;
        mapCam.orthographicSize = Mathf.Min(mapCam.orthographicSize, minSize);
    }
}
