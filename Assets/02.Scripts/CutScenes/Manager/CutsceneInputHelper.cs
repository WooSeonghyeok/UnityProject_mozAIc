using UnityEngine.InputSystem;

public static class CutsceneInputHelper
{
    public static bool IsAdvancePressedThisFrame()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.numpadEnterKey.wasPressedThisFrame)
            {
                return true;
            }
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }

    public static bool IsSkipPressedThisFrame()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return true;
        }

        if (Gamepad.current != null &&
            (Gamepad.current.buttonEast.wasPressedThisFrame || Gamepad.current.selectButton.wasPressedThisFrame))
        {
            return true;
        }

        return false;
    }
}
