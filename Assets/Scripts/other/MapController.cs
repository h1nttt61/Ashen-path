using UnityEngine;

public class MapController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private KeyCode mapKey = KeyCode.M;
    [SerializeField] private GameObject mapCanvas; //Raw изображение
    [SerializeField] private Camera mapCamera;

    [Header("Zoom")]
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 25f;
    [SerializeField] private float zoomStep = 2f;

    private bool isMapOpen = false;

    private void Update()
    {
        if (Input.GetKeyDown(mapKey))
            ToggleMap();

        if (isMapOpen)
            HandleZoom();
    }

    private void ToggleMap()
    {
        isMapOpen = !isMapOpen;
        mapCanvas.SetActive(isMapOpen);

        Time.timeScale = isMapOpen ? 0.2f : 1f;
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            mapCamera.orthographicSize = Mathf.Max(mapCamera.orthographicSize - zoomStep, minZoom);
        }
        else if (scroll < 0f)
        {
            mapCamera.orthographicSize = Mathf.Min(mapCamera.orthographicSize + zoomStep, maxZoom);
        }
    }
}
