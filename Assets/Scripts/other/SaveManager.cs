using UnityEngine;

public static class SaveManager
{
    private const string DASH_KEY = "DashUnlocked";
    private const string WALL_JUMP_KEY = "WallJumpUnlocked";
    private const string CHECKPOINT_X = "CheckpointX";
    private const string CHECKPOINT_Y = "CheckpointY";
    private const string LAST_CHECKPOINT_ID = "LastCheckpointID";
    private const string HEALTH_KEY = "PlayerHealth";
    private const string BOSS_DEFEATED_KEY = "BossDefeated";
    private const string SUPER_DASH_KEY = "SuperDashUnlocked";

    public static void SaveGame()
    {
        if (Player.Instance == null) return;
        PlayerPrefs.SetInt(DASH_KEY, Player.Instance.isDashUnlocked ? 1 : 0);
        PlayerPrefs.SetInt(WALL_JUMP_KEY, Player.Instance.isWallJumpUnlocked ? 1 : 0);
        PlayerPrefs.SetInt(HEALTH_KEY, Player.Instance.Health);

        PlayerPrefs.SetFloat(CHECKPOINT_X, Player.Instance.transform.position.x);
        PlayerPrefs.SetFloat(CHECKPOINT_Y, Player.Instance.transform.position.y);
        PlayerPrefs.SetInt(SUPER_DASH_KEY, Player.Instance.isSuperDashUnlocked ? 1 : 0);

        //ayerPrefs.Save();
    }

    public static void SaveCurrentCheckpoint(string id)
    {
        PlayerPrefs.SetString(LAST_CHECKPOINT_ID, id);
        PlayerPrefs.Save();
    }

    public static string GetLastCheckpointID()
    {
        return PlayerPrefs.GetString(LAST_CHECKPOINT_ID, "");
    }

    public static void LoadGame()
    {
        if (Player.Instance == null) return;

        if (PlayerPrefs.HasKey(DASH_KEY))
            Player.Instance.isDashUnlocked = PlayerPrefs.GetInt(DASH_KEY) == 1;

        if (PlayerPrefs.HasKey(WALL_JUMP_KEY))
            Player.Instance.isWallJumpUnlocked = PlayerPrefs.GetInt(WALL_JUMP_KEY) == 1;

        if (PlayerPrefs.HasKey(CHECKPOINT_X) && PlayerPrefs.HasKey(CHECKPOINT_Y))
        {
            float x = PlayerPrefs.GetFloat(CHECKPOINT_X);
            float y = PlayerPrefs.GetFloat(CHECKPOINT_Y);
            Vector3 savedPos = new Vector3(x, y, 0);

            Player.Instance.transform.position = savedPos;
            Player.Instance.UpdateCheckpoint(savedPos);
        }
        if (PlayerPrefs.HasKey(SUPER_DASH_KEY))
            Player.Instance.isSuperDashUnlocked = PlayerPrefs.GetInt(SUPER_DASH_KEY) == 1;
        if (PlayerPrefs.HasKey(HEALTH_KEY))
        {
            int savedHealth = PlayerPrefs.GetInt(HEALTH_KEY);
            Player.Instance.InitializeHealth(savedHealth);
        }
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteAll();    
        //PlayerPrefs.Save();
    }

    public static void SaveBossStatus(bool defeated)
    {
        PlayerPrefs.SetInt(BOSS_DEFEATED_KEY, defeated ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool IsBossDefeated()
    {
        return PlayerPrefs.GetInt(BOSS_DEFEATED_KEY, 0) == 1;
    }
}