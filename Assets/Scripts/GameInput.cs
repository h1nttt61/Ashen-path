using UnityEngine;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private PlayerInputAction playerInputAction;

    private void Awake()
    {
        Instance = this;
        playerInputAction = new PlayerInputAction();

        playerInputAction.Enable();
    }

    public Vector2 GetMovementVector()
    {
        Vector2 inputVector = playerInputAction.Player.Move.ReadValue<Vector2>();
        if (inputVector.magnitude > 0.1f)
        {
            Debug.Log($"Input detected: {inputVector}");
        }
        return inputVector;
    }
}
