using NUnit.Framework.Internal.Filters;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

[SelectionBase]
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    private PlayerInputAction playerInputAction;

    private Rigidbody2D rb;
    [SerializeField] private float speed = 5f;
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private float damageRecoveryTime = 0.5f;

    Vector2 inputVector;

    private Camera camera;

    private readonly float minSpeed = 0.1f;
    private bool isRunning = false;

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        camera = Camera.main;
    }



    private void Update()
    {
        inputVector = GameInput.Instance.GetMovementVector();
    }

    public Vector3 GetScreenPlayerPosition()
    {
        Vector3 playerScreenPos = camera.WorldToScreenPoint(transform.position);
        return playerScreenPos;
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        rb.MovePosition(rb.position + inputVector * Time.fixedDeltaTime * speed);
        if (Math.Abs(inputVector.x) > minSpeed)
            isRunning = true;
        else
            isRunning = false;
    }
}