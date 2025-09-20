using UnityEngine;

public static class SaveSystem
{
    const string KEY_HAS_SAVE = "HAS_SAVE";
    const string KEY_LEVEL = "LAST_LEVEL";

    public static bool HasSave => PlayerPrefs.GetInt(KEY_HAS_SAVE, 0) == 1;
    public static int  LastLevel => PlayerPrefs.GetInt(KEY_LEVEL, 1);

    public static void WriteProgress(int lastLevel)
    {
        PlayerPrefs.SetInt(KEY_LEVEL, Mathf.Max(1, lastLevel));
        PlayerPrefs.SetInt(KEY_HAS_SAVE, 1);
        PlayerPrefs.Save();
    }

    public static void Wipe()
    {
        PlayerPrefs.DeleteKey(KEY_LEVEL);
        PlayerPrefs.DeleteKey(KEY_HAS_SAVE);
        PlayerPrefs.Save();
    }
}
