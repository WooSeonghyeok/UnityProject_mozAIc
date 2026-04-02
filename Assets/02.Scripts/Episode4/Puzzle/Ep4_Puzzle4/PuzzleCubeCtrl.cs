using PuzzleInfo;
using System;
using UnityEngine;
public class PuzzleCubeCtrl : MonoBehaviour
{
    public GameObject cube;
    private Transform tr;
    public Rigidbody rb;
    public Color cubeColor;
    public GameObject colorSwitch;
    public PuzzleCubeData data;
    public int ID;
    public int row;
    public int column;
    public bool isRed;
    public bool isGreen;
    public bool isBlue;
    public PE4_Puzzle4_Cube.switchCondition condition;
    public Material CondMaterial_near;
    public Material CondMaterial_row;
    public Material CondMaterial_column;
    public Material CondMaterial_color;
    public int switchValue;
    public GameObject hint;
    public GameObject hint_cond;
    public GameObject hint_eff;
    public event Action OnColorChanged;
    public int newHeight;
    public float switchTime = 1f;
    public void SetColor(Color color)
    {
        if (cubeColor != color)
        {
            cubeColor = color;
            OnColorChanged?.Invoke();
        }
    }
    void Awake()
    {
        data = GetComponent<PuzzleCubeData>();
        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        CubeDataSetup();
    }
    public void CubeDataSetup()
    {
        ID = data.cubeData.place[0] * 10 + data.cubeData.place[1];
        row = data.cubeData.place[0];
        column = data.cubeData.place[1];
        isRed = data.cubeData.colorBool[0];
        isGreen = data.cubeData.colorBool[1];
        isBlue = data.cubeData.colorBool[2];
        condition = data.cubeData.cond;
        switchValue = data.cubeData.value;
    }
    void OnEnable()
    {
        transform.position = new Vector3(row * 2.5f, 0f, column * 2.5f);
        cubeColor = new Color(isRed ? 1 : 0, isGreen ? 1 : 0, isBlue ? 1 : 0);
        cube.GetComponent<Renderer>().material.color = cubeColor;
    }
    void Start()
    {
        SwitchSet();
        HintSet();
        if (Puzzle4Manager.instance != null)
        {
            Puzzle4Manager.instance.retryEvent += HandleRetry;
        }
    }
    private void SwitchSet()
    {
        if (colorSwitch != null)
        {
            colorSwitch.gameObject.SetActive(switchValue != 0);
            colorSwitch.GetComponent<BoxCollider>().enabled = (switchValue != 0);
        }
    }
    private void HintSet()
    {
        if (hint != null)
        {
            hint.SetActive(switchValue != 0);
            hint_cond.GetComponent<Renderer>().material = CondSetup(condition);
            hint_eff.GetComponent<Renderer>().material.color = EffColorSetup(switchValue);
        }
    }
    private void OnDisable()
    {
        if (Puzzle4Manager.instance != null)
        {
            Puzzle4Manager.instance.retryEvent -= HandleRetry;
        }
    }
    public void OnColorSwitch(int colorCode)  // colorCode: 1 for red, 2 for green, 4 for blue
    {
        if ((colorCode & 1) == 1) isRed = !isRed;
        if ((colorCode & 2) == 2) isGreen = !isGreen;
        if ((colorCode & 4) == 4) isBlue = !isBlue;
        cubeColor = new Color(isRed ? 1 : 0, isGreen ? 1 : 0, isBlue ? 1 : 0);
        cube.GetComponent<Renderer>().material.color = cubeColor;
        OnColorChanged?.Invoke();
    }
    public Material CondSetup(PE4_Puzzle4_Cube.switchCondition cond)
    {
        switch (cond)
        {
            case PE4_Puzzle4_Cube.switchCondition.near:
                return CondMaterial_near;
            case PE4_Puzzle4_Cube.switchCondition.column:
                return CondMaterial_column;
            case PE4_Puzzle4_Cube.switchCondition.row:
                return CondMaterial_row;
            case PE4_Puzzle4_Cube.switchCondition.color:
                return CondMaterial_color;
            default:
                return null;
        }
    }
    public Color EffColorSetup(int switchValue)
    {
        float redV = (switchValue & 1) == 1 ? 1 : 0;
        float greenV = (switchValue & 2) == 2 ? 1 : 0;
        float blueV = (switchValue & 4) == 4 ? 1 : 0;
        return new Color(redV, greenV, blueV);
    }
    private void HandleRetry()
    {
        CubeDataSetup();  // 데이터 재불러오기
        transform.position = new Vector3(row * 2.5f, 0f, column * 2.5f);  // 위치/색상/힌트 등 Start 및 OnEnable에서 한 설정 재적용
        cubeColor = new Color(isRed ? 1 : 0, isGreen ? 1 : 0, isBlue ? 1 : 0);
        var rend = cube.GetComponent<Renderer>();
        if (rend != null) rend.material.color = cubeColor;
        if (colorSwitch != null) SwitchSet();
        if (hint != null) HintSet();
        OnColorChanged?.Invoke();
    }
}