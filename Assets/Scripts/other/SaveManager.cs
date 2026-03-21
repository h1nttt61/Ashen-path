using UnityEngine;

public static class SaveManager
{
    private const string DASH_KEY = "DashUnlocked";
    private const string WALL_JUMP_KEY = "WallJumpUnlocked"; 
    private const string POS_X = "PlayerX";
    private const string POS_Y = "PlayerY";
    private const string HEALTH_KEY = "PlayerHealth";

    public static void SaveGame()
    {
        if (Player.Instance == null) return;
        PlayerPrefs.SetInt(DASH_KEY, Player.Instance.isDashUnlocked ? 1 : 0);
        PlayerPrefs.SetInt(WALL_JUMP_KEY, Player.Instance.isWallJumpUnlocked ? 1 : 0); 
        PlayerPrefs.SetInt(HEALTH_KEY, Player.Instance.Health);

        PlayerPrefs.SetFloat(POS_X, Player.Instance.transform.position.x);
        PlayerPrefs.SetFloat(POS_Y, Player.Instance.transform.position.y);

        PlayerPrefs.Save();
    }

    public static void LoadGame()
    {
        if (Player.Instance == null) return;

        if (PlayerPrefs.HasKey(DASH_KEY))
            Player.Instance.isDashUnlocked = PlayerPrefs.GetInt(DASH_KEY) == 1;

        if (PlayerPrefs.HasKey(WALL_JUMP_KEY)) // Загружаем Wall Jump
            Player.Instance.isWallJumpUnlocked = PlayerPrefs.GetInt(WALL_JUMP_KEY) == 1;

        if (PlayerPrefs.HasKey(POS_X) && PlayerPrefs.HasKey(POS_Y))
        {
            float x = PlayerPrefs.GetFloat(POS_X);
            float y = PlayerPrefs.GetFloat(POS_Y);
            Player.Instance.transform.position = new Vector3(x, y, 0);
        }
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}