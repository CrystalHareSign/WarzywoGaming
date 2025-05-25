using UnityEngine;

public static class InputBlocker
{
    /// <summary>
    /// Jeœli true, ¿aden input z klawiatury ani myszy nie przejdzie do gameplayu.
    /// </summary>
    public static bool Active = false;

    // Zamiast Input.GetKey/GetKeyDown u¿ywaj tych metod:
    public static bool GetKey(KeyCode key)
    {
        return !Active && Input.GetKey(key);
    }

    public static bool GetKeyDown(KeyCode key)
    {
        return !Active && Input.GetKeyDown(key);
    }

    public static bool GetKeyUp(KeyCode key)
    {
        return !Active && Input.GetKeyUp(key);
    }

    public static bool GetMouseButton(int button)
    {
        return !Active && Input.GetMouseButton(button);
    }

    public static bool GetMouseButtonDown(int button)
    {
        return !Active && Input.GetMouseButtonDown(button);
    }

    public static bool GetMouseButtonUp(int button)
    {
        return !Active && Input.GetMouseButtonUp(button);
    }

    // Przyk³adowy helper do blokowania osi:
    public static float GetAxis(string axisName)
    {
        return !Active ? Input.GetAxis(axisName) : 0f;
    }
}