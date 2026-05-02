using UnityEngine;
using System.Collections;

public class SecondLocSpiritTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpiritNPC spirit;
    [SerializeField] private string[] phrase;

    [Header("Settings")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(-3f, 1.5f, 0f);

    private bool hasTriggered = false;
    private const string SECOND_SPIRIT_KEY = "SecondLocSpiritMet";

    private void Start()
    {
        // Если уже встречались, просто помечаем как сработавшее, 
        // но НЕ выключаем объект сразу, чтобы не ломать ссылки других скриптов
        if (PlayerPrefs.GetInt(SECOND_SPIRIT_KEY, 0) == 1)
        {
            hasTriggered = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Проверяем наличие игрока, духа и не сработал ли триггер раньше
        if (collision.CompareTag("Player") && !hasTriggered && spirit != null)
        {
            hasTriggered = true;
            StartCoroutine(SpawnSpiritRoutine());
        }
    }

    private IEnumerator SpawnSpiritRoutine()
    {
        // Сначала включаем объект, а потом запускаем на нем логику[cite: 4, 5]
        spirit.gameObject.SetActive(true);
        spirit.transform.position = Player.Instance.transform.position + spawnOffset;

        NPCDialog dialog = spirit.GetComponent<NPCDialog>();
        if (dialog != null)
        {
            // Настройка диалога[cite: 2]
            dialog.lines = phrase;
            dialog.giveDashOnEnd = false;
            dialog.giveWallJumpOnEnd = false;

            // Запускаем корутину диалога
            yield return StartCoroutine(dialog.DisplayFullDialog());
        }

        // Сохраняем прогресс[cite: 6]
        PlayerPrefs.SetInt(SECOND_SPIRIT_KEY, 1);
        PlayerPrefs.Save();

        // Не уничтожаем объект триггера сразу, чтобы избежать MissingReference
        // Дух сам исчезнет через FadeOut, так как это встроено в его логику[cite: 5]
    }
}