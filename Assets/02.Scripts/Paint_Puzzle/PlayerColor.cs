using UnityEngine;

public class PlayerColor : MonoBehaviour
{
    public ColorType currentColor;
    public bool hasColor = false;

    public Renderer playerRenderer;

    public Material redMat, blueMat, yellowMat;
    public Material greenMat, orangeMat, purpleMat;
    public Material pinkMat, brownMat, whiteMat;

    public Material defaultMat; // 👉 Canvas_W

    void Start()
    {
        ResetColor();
    }

    // 🎯 색 추가
    public void AddColor(ColorType newColor)
    {
        // 💡 처음 색 먹을 때
        if (!hasColor)
        {
            currentColor = newColor;
            hasColor = true;
            ApplyMaterial(newColor);
            return;
        }

        // 같은 색이면 유지
        if (currentColor == newColor)
            return;

        // 💖 Pink (White + Red)
        if ((currentColor == ColorType.White && newColor == ColorType.Red) ||
            (currentColor == ColorType.Red && newColor == ColorType.White))
        {
            SetColor(ColorType.Pink);
            return;
        }

        // 🎨 기본 혼합
        if ((currentColor == ColorType.Yellow && newColor == ColorType.Blue) ||
            (currentColor == ColorType.Blue && newColor == ColorType.Yellow))
        {
            SetColor(ColorType.Green);
        }
        else if ((currentColor == ColorType.Red && newColor == ColorType.Blue) ||
                 (currentColor == ColorType.Blue && newColor == ColorType.Red))
        {
            SetColor(ColorType.Purple);
        }
        else if ((currentColor == ColorType.Red && newColor == ColorType.Yellow) ||
                 (currentColor == ColorType.Yellow && newColor == ColorType.Red))
        {
            SetColor(ColorType.Orange);
        }
        else if ((currentColor == ColorType.Green && newColor == ColorType.Red) ||
                 (currentColor == ColorType.Purple && newColor == ColorType.Yellow) ||
                 (currentColor == ColorType.Orange && newColor == ColorType.Blue))
        {
            SetColor(ColorType.Brown);
        }
        else
        {
            // 정의 안 된 조합
            SetColor(newColor);
        }
    }

    // 🎯 색 설정
    public void SetColor(ColorType type)
    {
        currentColor = type;
        hasColor = true;
        ApplyMaterial(type);
    }

    // 🎨 머터리얼 적용
    void ApplyMaterial(ColorType type)
    {
        switch (type)
        {
            case ColorType.Red: playerRenderer.material = redMat; break;
            case ColorType.Blue: playerRenderer.material = blueMat; break;
            case ColorType.Yellow: playerRenderer.material = yellowMat; break;
            case ColorType.Green: playerRenderer.material = greenMat; break;
            case ColorType.Orange: playerRenderer.material = orangeMat; break;
            case ColorType.Purple: playerRenderer.material = purpleMat; break;
            case ColorType.Pink: playerRenderer.material = pinkMat; break;
            case ColorType.Brown: playerRenderer.material = brownMat; break;
            case ColorType.White: playerRenderer.material = whiteMat; break;
        }
    }

    // 🧼 초기화 (핵심 ⭐)
    public void ResetColor()
    {
        hasColor = false;
        playerRenderer.material = defaultMat; // 👉 Canvas_W
    }
}