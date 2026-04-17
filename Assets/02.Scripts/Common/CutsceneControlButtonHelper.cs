using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class CutsceneControlButtonHelper
{
    public static void TryAutoResolve(
        ref Button nextButton,
        ref GameObject nextButtonRoot,
        ref Button skipButton,
        ref GameObject skipButtonRoot)
    {
        if (nextButton != null && nextButtonRoot == null)
        {
            nextButtonRoot = nextButton.gameObject;
        }

        if (skipButton != null && skipButtonRoot == null)
        {
            skipButtonRoot = skipButton.gameObject;
        }

        TextboxManager textboxManager = FindTextboxManager();
        if (textboxManager == null)
        {
            return;
        }

        if (nextButton == null && textboxManager.nextBtn != null)
        {
            nextButtonRoot = textboxManager.nextBtn;
            nextButton = textboxManager.nextBtn.GetComponent<Button>();
        }

        if (skipButton == null)
        {
            Button[] buttons = textboxManager.GetComponentsInChildren<Button>(true);
            foreach (Button candidate in buttons)
            {
                if (candidate == null || candidate == nextButton)
                {
                    continue;
                }

                if (!IsSkipButton(candidate))
                {
                    continue;
                }

                skipButton = candidate;
                break;
            }
        }

        if (nextButton != null && nextButtonRoot == null)
        {
            nextButtonRoot = nextButton.gameObject;
        }

        if (skipButton != null && skipButtonRoot == null)
        {
            skipButtonRoot = skipButton.gameObject;
        }
    }

    public static void Register(Button button, UnityAction callback)
    {
        if (button == null || callback == null)
        {
            return;
        }

        button.onClick.RemoveListener(callback);
        button.onClick.AddListener(callback);
    }

    public static void SetVisible(Button button, GameObject root, bool visible)
    {
        GameObject target = root;
        if (target == null && button != null)
        {
            target = button.gameObject;
        }

        if (target != null)
        {
            target.SetActive(visible);
        }
    }

    private static TextboxManager FindTextboxManager()
    {
        TextboxManager[] managers = Resources.FindObjectsOfTypeAll<TextboxManager>();
        foreach (TextboxManager manager in managers)
        {
            if (manager == null)
            {
                continue;
            }

            if (!manager.gameObject.scene.IsValid())
            {
                continue;
            }

            return manager;
        }

        return null;
    }

    private static bool IsSkipButton(Button button)
    {
        string lowerName = button.gameObject.name.ToLowerInvariant();
        if (lowerName.Contains("skip") || lowerName.Contains("esc"))
        {
            return true;
        }

        TMP_Text tmpText = button.GetComponentInChildren<TMP_Text>(true);
        if (tmpText != null && ContainsSkipKeyword(tmpText.text))
        {
            return true;
        }

        Text legacyText = button.GetComponentInChildren<Text>(true);
        return legacyText != null && ContainsSkipKeyword(legacyText.text);
    }

    private static bool ContainsSkipKeyword(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string lower = value.ToLowerInvariant();
        return lower.Contains("skip") || lower.Contains("esc") || value.Contains("\uC2A4\uD0B5");
    }
}
