using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Rysuje standardowe pola Inspektora
        DrawDefaultInspector();

        // Pobieramy referencjê do skryptu GameManager
        GameManager gameManager = (GameManager)target;

        // Dodajemy przycisk do resetowania danych
        if (GUILayout.Button("Resetuj dane gracza"))
        {
            gameManager.ResetPlayerData();
        }
    }
}