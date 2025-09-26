using UnityEngine;
using System;

public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance { get; private set; }

    // progresso do set Chest
    public int chestPiecesCollected { get; private set; }
    public bool chestUnlocked { get; private set; }

    public event Action OnProgressChanged;

    const string KEY_CHEST_PIECES = "GP_CHEST_PIECES";
    const string KEY_CHEST_UNLOCK = "GP_CHEST_UNLOCK";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public void AddChestPiece(int amount = 1)
    {
        if (chestUnlocked) return;

        chestPiecesCollected += amount;
        if (chestPiecesCollected >= 3)
        {
            chestPiecesCollected = 3;
            chestUnlocked = true;
        }
        Save();
        OnProgressChanged?.Invoke();
    }

    public void ResetChestProgress()
    {
        chestPiecesCollected = 0;
        chestUnlocked = false;
        Save();
        OnProgressChanged?.Invoke();
    }

    void Save()
    {
        PlayerPrefs.SetInt(KEY_CHEST_PIECES, chestPiecesCollected);
        PlayerPrefs.SetInt(KEY_CHEST_UNLOCK, chestUnlocked ? 1 : 0);
        PlayerPrefs.Save();
    }

    void Load()
    {
        chestPiecesCollected = PlayerPrefs.GetInt(KEY_CHEST_PIECES, 0);
        chestUnlocked = PlayerPrefs.GetInt(KEY_CHEST_UNLOCK, 0) == 1;
    }
}
