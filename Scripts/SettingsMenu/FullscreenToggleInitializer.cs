using UnityEngine;
using UnityEngine.UI;

public class FullscreenToggleInitializer : MonoBehaviour
{
    void Start()
    {

        Toggle toggle = GetComponent<Toggle>();

        if (toggle == null)
        {
            Debug.LogError("На этом объекте нет компонента Toggle!");
            return;
        }


        bool isFullscreen;

        if (PlayerPrefs.HasKey("FullscreenPreference"))
        {

            int savedValue = PlayerPrefs.GetInt("FullscreenPreference");
            isFullscreen = (savedValue == 1);
            Debug.Log($"Загружено сохранённое значение: {savedValue} -> {isFullscreen}");
        }
        else
        {

            isFullscreen = Screen.fullScreen;
            Debug.Log($"Используется текущий режим экрана: {isFullscreen}");
        }


        var originalListeners = toggle.onValueChanged;


        toggle.onValueChanged = new Toggle.ToggleEvent();


        toggle.isOn = isFullscreen;


        toggle.onValueChanged = originalListeners;

        Debug.Log($"Toggle '{gameObject.name}' установлен в: {isFullscreen}");
    }


    void OnEnable()
    {

        UpdateToggle();
    }

    void UpdateToggle()
    {
        Toggle toggle = GetComponent<Toggle>();
        if (toggle != null)
        {

            if (toggle.isOn != Screen.fullScreen)
            {

                var originalListeners = toggle.onValueChanged;
                toggle.onValueChanged = new Toggle.ToggleEvent();


                toggle.isOn = Screen.fullScreen;


                toggle.onValueChanged = originalListeners;

                Debug.Log($"Toggle '{gameObject.name}' обновлён к: {Screen.fullScreen}");
            }
        }
    }
}