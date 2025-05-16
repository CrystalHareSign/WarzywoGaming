using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SaveManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Rysuje standardowe pola Inspektora
        DrawDefaultInspector();

        // Referencja do GameManagera
        SaveManager gameManager = (SaveManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("=== DEBUG TOOLS ===", EditorStyles.boldLabel);

        // Slider wyboru slotu
        gameManager.debugSlotIndex = EditorGUILayout.IntSlider("Slot Index", gameManager.debugSlotIndex, 0, 2);

        if (GUILayout.Button("Resetuj dane gracza"))
        {
            gameManager.ResetCurrency();
        }

        if (GUILayout.Button("Resetuj pozycjê gracza"))
        {
            gameManager.ResetPositionAndRotation();
        }

        if (GUILayout.Button("Resetuj zapis wybranego slotu"))
        {
            gameManager.ResetSaveSlot(gameManager.debugSlotIndex);
        }

        if (GUILayout.Button("Wczytaj zapis z wybranego slotu"))
        {
            gameManager.LoadPlayerData(gameManager.debugSlotIndex);
        }

        if (GUILayout.Button("Dodaj 100 waluty"))
        {
            gameManager.AddCurrency(100f);
        }
    }
}
