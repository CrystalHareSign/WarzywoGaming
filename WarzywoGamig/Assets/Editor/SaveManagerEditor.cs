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
        EditorGUILayout.LabelField("=== RESETUJ ZAPIS WYBRANEGO SLOTU ===", EditorStyles.boldLabel);

        // Reset slotów osobno dla ka¿dego
        EditorGUILayout.HelpBox("Te przyciski usuwaj¹ plik zapisu dla wybranego slotu. Ta operacja jest nieodwracalna!", MessageType.Warning);

        if (GUILayout.Button(new GUIContent("Resetuj zapis slotu 0", "Usuwa plik zapisu slotu 0 (pierwszy slot).")))
        {
            gameManager.ResetSaveSlot(0);
        }
        if (GUILayout.Button(new GUIContent("Resetuj zapis slotu 1", "Usuwa plik zapisu slotu 1 (drugi slot).")))
        {
            gameManager.ResetSaveSlot(1);
        }
        if (GUILayout.Button(new GUIContent("Resetuj zapis slotu 2", "Usuwa plik zapisu slotu 2 (trzeci slot).")))
        {
            gameManager.ResetSaveSlot(2);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("=== DEBUG TOOLS ===", EditorStyles.boldLabel);

        // Reset waluty gracza
        EditorGUILayout.HelpBox("Resetuje walutê gracza do zera w bie¿¹cej rozgrywce (nie dotyczy zapisu na dysku).", MessageType.None);
        if (GUILayout.Button(new GUIContent("Resetuj dane gracza", "Ustawia iloœæ waluty gracza na zero tylko w bie¿¹cej sesji.")))
        {
            gameManager.ResetCurrency();
        }

        // Reset pozycji gracza
        EditorGUILayout.HelpBox("Ustawia pozycjê i rotacjê gracza na (0,0,0), ale tylko w aktualnie za³adowanej scenie.", MessageType.None);
        if (GUILayout.Button(new GUIContent("Resetuj pozycjê gracza", "Przesuwa gracza na œrodek œwiata (0,0,0) i zeruje rotacjê.")))
        {
            gameManager.ResetPositionAndRotation();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("=== DODATKOWE OPERACJE ===", EditorStyles.boldLabel);

        // Wczytanie zapisu z ka¿dego slotu osobno
        EditorGUILayout.HelpBox("Wczytuje zapis z wybranego slotu. Uwaga: wczytanie zapisu zmieni scenê i ustawi gracza wed³ug danych z pliku.", MessageType.None);

        if (GUILayout.Button(new GUIContent("Wczytaj zapis slotu 0", "Za³aduj stan gry z slotu 0 (scena, pozycja gracza, waluta itd.).")))
        {
            gameManager.LoadPlayerData(0);
        }
        if (GUILayout.Button(new GUIContent("Wczytaj zapis slotu 1", "Za³aduj stan gry z slotu 1 (scena, pozycja gracza, waluta itd.).")))
        {
            gameManager.LoadPlayerData(1);
        }
        if (GUILayout.Button(new GUIContent("Wczytaj zapis slotu 2", "Za³aduj stan gry z slotu 2 (scena, pozycja gracza, waluta itd.).")))
        {
            gameManager.LoadPlayerData(2);
        }

        // Dodanie 100 waluty
        EditorGUILayout.HelpBox("Dodaje 100 jednostek waluty do bie¿¹cego stanu gracza.", MessageType.None);
        if (GUILayout.Button(new GUIContent("Dodaj 100 waluty", "Dodaje 100 waluty graczowi (tylko w bie¿¹cej sesji).")))
        {
            gameManager.AddCurrency(100f);
        }
    }
}