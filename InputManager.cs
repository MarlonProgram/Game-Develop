using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    public PlayerInput.OnFootActions OnFoot;

    

    private void Awake()
    {
        playerInput = new PlayerInput();
        OnFoot = playerInput.OnFoot;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        OnFoot.Enable();
    }

    private void OnDisable()
    {
        OnFoot.Disable();
    }
}
